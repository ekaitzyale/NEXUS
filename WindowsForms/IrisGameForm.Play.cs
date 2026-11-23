namespace WindowsForms;

public sealed partial class IrisGameForm
{
    private Label? explorationHud;
    private string EnemyName => combatEnemy?.Name ?? throw new InvalidOperationException();

    private void Speak(int room, string trigger)
    {
        var line = catalog.Line(Session.Phase, room, trigger);
        if (Session.SeenEvents.Contains(line.Id) || Session.PendingEvents.Contains(line.Id)) return;
        audio.QueueVoice(line.Id);
        if (line.Blocking)
        {
            using var dialog = Hologram("JARVIS", line.Text, "CONTINUAR", portrait: true);
            dialog.ShowDialog(this);
            Session.SeenEvents.Add(line.Id);
        }
        else
        {
            Session.PendingEvents.Add(line.Id);
            refreshNarrative?.Invoke();
        }
    }

    private void AddNarrative(Control page)
    {
        var panel = new Panel { Dock = DockStyle.Right, Width = 330, BackColor = Surface, BorderStyle = BorderStyle.FixedSingle };
        var art = Portrait();
        art.Height = 82;
        art.Dock = DockStyle.Top;
        var header = Caption("JARVIS", 17, Cyan);
        header.Dock = DockStyle.Top;
        header.Height = 35;
        var text = new TextBox
        {
            Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackColor = Surface, ForeColor = Color.White,
            Multiline = true, ReadOnly = true, Margin = new Padding(10), TabStop = false
        };
        var padded = new Panel { Dock = DockStyle.Fill, Padding = new Padding(14, 8, 14, 8) };
        padded.Controls.Add(text);
        var next = Button("CONTINUAR", () =>
        {
            var id = Session.PendingEvents[0];
            Session.PendingEvents.RemoveAt(0);
            Session.SeenEvents.Add(id);
            refreshNarrative?.Invoke();
            board?.Focus();
        });
        next.Dock = DockStyle.Bottom;
        panel.Controls.Add(padded);
        panel.Controls.Add(next);
        panel.Controls.Add(header);
        panel.Controls.Add(art);
        page.Controls.Add(panel);
        panel.SendToBack();

        refreshNarrative = () =>
        {
            var lines = Session.PendingEvents.Select(catalog.FindLine).OfType<JarvisLine>().ToArray();
            panel.Visible = lines.Length > 0;
            text.Text = string.Join(Environment.NewLine + Environment.NewLine, lines.Select(line => line.Text));
            text.ScrollBars = lines.Length > 2 ? ScrollBars.Vertical : ScrollBars.None;
        };
        refreshNarrative();
    }

    private void Explore()
    {
        var definition = catalog.Room(Session.Room);
        var page = Page($"FASE {definition.Phase} · {PhaseNames[definition.Phase]}  /  SALA {definition.Number:00}", definition.Name);
        var footer = Bar(page);
        var labels = catalog.Labels("EXPLORACION");
        interactHint = Caption("", 10, Muted);
        interactHint.Size = new Size(270, 44);
        footer.Controls.Add(interactHint);
        faceEnemy = Button(labels[6], BeginEncounter, 145, Red);
        footer.Controls.Add(faceEnemy);
        footer.Controls.Add(InventoryButton());
        footer.Controls.Add(Button(labels[7], Pause, 135));

        explorationHud = Caption("", 11, Cyan);
        explorationHud.Dock = DockStyle.Top;
        explorationHud.Height = 62;
        page.Controls.Add(explorationHud);

        board = new ArtPanel { Dock = DockStyle.Fill, TabStop = true, AccessibleName = definition.Name };
        board.Paint += PaintRoom;
        page.Controls.Add(board);
        AddNarrative(page);
        board.BringToFront();
        footer.SendToBack();
        explorationHud.SendToBack();
        ObserveRoom();
        UpdateExploration();
        board.Focus();
    }

    private void ObserveRoom()
    {
        if (Session.Room == 1)
        {
            if (!Session.KitCollected && Session.InReach(GameSession.Kit)) Speak(1, "evento_kit");
            if (Session.KitCollected && Session.InReach(GameSession.Exit)) Speak(1, "evento_salida");
        }
        else if (Session.Room == 2 && Session.ActiveEnemies().Any()) Speak(2, "evento_enemigo");
        else if (Session.HasCircuit && !Session.CircuitRestored && Session.IsAdjacent(Session.CircuitPosition))
            Speak(Session.Room, "evento_circuito");
        else if (Session.Room == 4)
        {
            Speak(4, "evento_entrada");
            if (Session.IsAdjacent(GameSession.Terminal)) Speak(4, "evento_registro");
        }
        else if (Session.Room == 5) Speak(5, "evento_entrada");
        else if (Session.Room == 7 && !Session.ChipCollected && Session.InReach(GameSession.Chip))
            Speak(7, "evento_chip");
        else if (Session.Room == 8) Speak(8, "evento_acceso");
        else if (Session.Room == 9) Speak(9, "evento_suministros");
        else if (Session.Room == 11) Speak(11, "evento_alerta");
        else if (Session.Room == 12) Speak(12, "evento_archivos");
        else if (Session.HasGenerator && !Session.GeneratorDisabled) Speak(Session.Room, "evento_generador");
        else if (Session.Room == 17 && Session.ActiveEnemies().Any()) Speak(17, "evento_jefe");
        else if (Session.Room == 18 && Session.IsAdjacent(GameSession.Device)) Speak(18, "evento_nucleo");
    }

    private void UpdateExploration()
    {
        var labels = catalog.Labels("EXPLORACION");
        explorationHud!.Text = $"{Session.Name}    ·    {labels[1]} {Session.Hp}/100    ·    {labels[2]} {CombatTurn.PlayerDamage}"
            + $"    ·    {labels[3]} {Session.Kits}/5    ·    JARVIS {Session.Charges}/6"
            + (Session.ChipCollected ? "    ·    CHIP DE INFORMACIÓN\n" : "\n")
            + $"{labels[0]}   {catalog.Room(Session.Room).Objective}";
        string prompt = Session.HasTerminal && Session.IsAdjacent(GameSession.Terminal) ? labels[5] : labels[4];
        interactHint!.Text = $"WASD / FLECHAS\n{prompt}";
        faceEnemy!.Visible = Session.AdjacentEnemy() is not null;
        board?.Invalidate();
    }

    private void Interact()
    {
        if (Session.HasTerminal && Session.IsAdjacent(GameSession.Terminal))
        {
            foreach (var document in catalog.Documents(Session.Room))
            {
                OpenDocument(document);
                Session.ReadDocument(document.Id);
            }
        }
        else if (Session.HasKit && !Session.KitCollected && Session.InReach(GameSession.Kit))
        {
            Session.CollectKit();
        }
        else if (Session.HasCircuit && !Session.CircuitRestored && Session.IsAdjacent(Session.CircuitPosition))
        {
            using var puzzle = new CircuitPuzzle();
            if (puzzle.ShowDialog(this) == DialogResult.OK && puzzle.Solved) Session.RestoreCircuit();
        }
        else if (Session.Room == 7 && !Session.ChipCollected && Session.InReach(GameSession.Chip))
        {
            Session.CollectChip();
        }
        else if (Session.Room == 13 && !Session.InvestigationComplete && Session.Investigate())
        {
            Speak(13, "evento_revelacion");
        }
        else if (Session.HasGenerator && Session.DisableGenerator())
        {
            Speak(Session.Room, "evento_ok");
        }
        else if (Session.Room == 18 && Session.CompleteMission())
        {
            Speak(18, "evento_nucleo");
            Speak(18, "evento_final");
            MissionCompleteScreen();
            return;
        }
        else if (Session.InReach(GameSession.Exit))
        {
            if (!Session.CanExit) Notice("ACCESO BLOQUEADO", "Completa el objetivo de la sala.");
            else if (Session.Advance())
            {
                Session.SeenEvents.UnionWith(Session.PendingEvents);
                Session.PendingEvents.Clear();
                if (Session.PhaseComplete)
                {
                    Speak(Session.Room, "evento_fin");
                    PhaseCompleteScreen();
                }
                else Explore();
                return;
            }
        }

        UpdateExploration();
        board?.Focus();
    }

    private void BeginEncounter()
    {
        if (Session.AdjacentEnemy() is not { } enemy) return;
        combatEnemy = enemy;
        combat = new CombatTurn(Session.Hp, Session.Kits, Session.Charges, enemy.MaxHp, enemy.Damage);
        if (Session.RequiresTutorialCharge)
        {
            Speak(2, "evento_pa");
            Speak(2, "evento_carga");
        }
        CombatScreen();
    }

    private static string ActionText(CombatAction action) => action switch
    {
        CombatAction.Attack => "ATACAR",
        CombatAction.Defend => "DEFENDER",
        CombatAction.Reserve => "RESERVAR",
        CombatAction.UseKit => "USAR KIT",
        _ => throw new ArgumentOutOfRangeException(nameof(action))
    };

    private void CombatScreen()
    {
        var labels = catalog.Labels("COMBATE");
        var page = Page("COMBATE", $"{EnemyName}  /  TURNO {Encounter.Number}");
        var cards = new TableLayoutPanel { Dock = DockStyle.Top, Height = 170, ColumnCount = 2 };
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        cards.Controls.Add(FighterCard("CADETE", Encounter.Hp, 100, CombatTurn.PlayerDamage, false), 0, 0);
        cards.Controls.Add(FighterCard(EnemyName, Encounter.EnemyHp, Encounter.EnemyMaxHp, Encounter.EnemyDamage, true), 1, 0);
        var controls = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(8) };
        var moves = Caption($"PA DISPONIBLES {Encounter.RemainingPa}  ·  MÁXIMO {CombatTurn.MaxPa} POR TURNO  ·  BOTIQUINES {Encounter.Kits}\n"
            + string.Join("   /   ", Encounter.Actions.Select(ActionText)), 11, Cyan);
        moves.Height = 68;
        moves.TextAlign = ContentAlignment.MiddleLeft;
        moves.Dock = DockStyle.Top;

        foreach (var action in Enum.GetValues<CombatAction>())
        {
            var button = Button(ActionText(action), () => { if (Encounter.Choose(action)) CombatScreen(); }, 130);
            button.Enabled = Encounter.RemainingPa > 0 && (action != CombatAction.UseKit || Encounter.CanUseKit);
            controls.Controls.Add(button);
        }
        var jarvis = Button($"{labels[4]} ({Encounter.Charges})", SelectCharges, 220, Color.MediumPurple);
        jarvis.Enabled = Encounter.SelectableCharges > 0;
        controls.Controls.Add(jarvis);
        var undo = Button("DESHACER ÚLTIMA ACCIÓN", () => { Encounter.Undo(); CombatScreen(); }, 230);
        undo.Enabled = Encounter.Actions.Count > 0;
        controls.Controls.Add(undo);
        var finish = Button(labels[7], ResolveCombat, 190);
        finish.Enabled = Encounter.RemainingPa == 0 && (!Session.RequiresTutorialCharge || Encounter.UsedCharge);
        controls.Controls.Add(finish);
        controls.Controls.Add(InventoryButton());

        page.Controls.Add(controls);
        page.Controls.Add(moves);
        page.Controls.Add(cards);
        AddNarrative(page);
    }

    private void SelectCharges()
    {
        int selectable = Encounter.SelectableCharges;
        if (selectable == 0) return;
        var labels = catalog.Labels("COMBATE");
        using var dialog = Dialog(labels[4], new Size(490, 292));
        var count = Caption($"0 / {Encounter.Charges} PA", 19, Cyan);
        count.Height = 55;
        count.Dock = DockStyle.Top;
        dialog.Controls.Add(count);
        var bar = Bar(dialog);
        var adjust = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56 };
        dialog.Controls.Add(adjust);
        adjust.BringToFront();
        int amount = 0;
        var plus = Button("+", () => { }, 104);
        var minus = Button("−", () => { }, 104);
        var confirm = Button(labels[5], () => dialog.DialogResult = DialogResult.OK, 145);
        var cancel = Button(labels[6], () => dialog.DialogResult = DialogResult.Cancel, 145);
        adjust.Controls.Add(minus);
        adjust.Controls.Add(plus);
        plus.Click += (_, _) => Adjust(1);
        minus.Click += (_, _) => Adjust(-1);
        bar.Controls.Add(confirm);
        bar.Controls.Add(cancel);
        dialog.CancelButton = cancel;
        Adjust(0);
        if (dialog.ShowDialog(this) == DialogResult.OK && Encounter.AllocateCharges(amount)) CombatScreen();

        void Adjust(int delta)
        {
            amount = Math.Clamp(amount + delta, 0, selectable);
            count.Text = $"+{amount} PA / {selectable} CARGAS\nMÁXIMO {CombatTurn.MaxPa} PA POR TURNO";
            plus.Enabled = amount < selectable;
            minus.Enabled = confirm.Enabled = amount > 0;
        }
    }

    private void ResolveCombat()
    {
        if (Encounter.RemainingPa != 0 || (Session.RequiresTutorialCharge && !Encounter.UsedCharge)) return;
        var result = Encounter.Resolve(combatRandom);
        var labels = catalog.Labels("RESULTADO");
        var page = Page(labels[0], $"TURNO {Encounter.Number}");
        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        AddCard(Session.Name, string.Join("\n", result.Player.GroupBy(action => action).Select(g => $"{ActionText(g.Key)} × {g.Count()}")), Cyan, 0, 0);
        AddCard(EnemyName, string.Join("\n", result.Enemy.GroupBy(action => action).Select(g => $"{ActionText(g.Key)} × {g.Count()}")), Red, 1, 0);
        string opponent = Session.Room == 17 ? EnemyName : "Robot";
        AddCard(labels[3], $"Cadete {Encounter.Hp}/100 HP  ·  {opponent} {Encounter.EnemyHp}/{Encounter.EnemyMaxHp} HP\n"
            + $"Curación: +{result.Healing} HP\nPróximo turno: Cadete {result.NextPlayerPa} PA  ·  {opponent} {result.NextEnemyPa} PA", Cyan, 0, 1);
        AddCard("DAÑO Y BLOQUEOS", $"{labels[1]}: {result.DamageDealt} HP\n{labels[2]}: {result.DamageTaken} HP\n"
            + $"Bloqueos: Cadete {result.PlayerBlocks}  ·  Robot {result.EnemyBlocks}", Red, 1, 1);
        var bar = Bar(page);
        bar.Controls.Add(Button(labels[4], ContinueCombat));
        bar.Controls.Add(InventoryButton());
        page.Controls.Add(body);
        bar.SendToBack();

        void AddCard(string title, string detail, Color color, int column, int row)
        {
            var card = Caption($"{title}\n\n{detail}", 11, color);
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(7);
            card.BackColor = Surface;
            body.Controls.Add(card, column, row);
        }
    }

    private void ContinueCombat()
    {
        var enemy = combatEnemy ?? throw new InvalidOperationException();
        Session.ApplyCombat(Encounter, enemy);
        if (Session.Hp == 0) Failure();
        else if (Session.IsEnemyDefeated(Session.Room, enemy.Id))
        {
            combat = null;
            combatEnemy = null;
            if (Session.RequiresTutorialCharge) Speak(2, "evento_victoria");
            if (Session.Room == 17) Speak(17, "evento_ok");
            Explore();
        }
        else
        {
            Encounter.Next();
            CombatScreen();
        }
    }

    private void Failure()
    {
        combat = null;
        combatEnemy = null;
        var labels = catalog.Labels("GAME_OVER");
        var page = Page(labels[0], "EL CADETE HA SIDO ELIMINADO", Red);
        var art = Portrait();
        art.Dock = DockStyle.Fill;
        var bar = Bar(page);
        bar.Controls.Add(Button(labels[1], () => { session = Session.RetryPhase(); StartPhase(); }));
        bar.Controls.Add(Button(labels[2], MainMenu));
        page.Controls.Add(art);
        bar.SendToBack();
    }

    private static Form Dialog(string title, Size size) => new()
    {
        Text = title, ClientSize = size, StartPosition = FormStartPosition.CenterParent,
        FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
        ShowInTaskbar = false, BackColor = Surface, ForeColor = Color.White, Padding = new Padding(14),
        AutoScaleMode = AutoScaleMode.Dpi
    };

    private static Form Hologram(string title, string text, string close, string? metadata = null, bool portrait = false)
    {
        var dialog = Dialog(title, new Size(portrait ? 735 : 650, 375));
        var frame = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, BackColor = Background, Padding = new Padding(12) };
        var header = Caption(title, 19, Cyan);
        header.Dock = DockStyle.Top;
        header.Height = 56;
        var content = new TextBox
        {
            Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true, BorderStyle = BorderStyle.None,
            BackColor = Background, ForeColor = Color.White, Font = new Font("Segoe UI", 13), Text = text, TabStop = false
        };
        var bar = Bar(frame);
        var button = Button(close, () => dialog.DialogResult = DialogResult.OK, 170);
        bar.Controls.Add(button);
        dialog.AcceptButton = button;
        var main = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 8, 12, 8) };
        main.Controls.Add(content);
        if (metadata is not null)
        {
            var sub = Caption(metadata, 11, Muted);
            sub.Dock = DockStyle.Top;
            sub.Height = 38;
            main.Controls.Add(sub);
        }

        frame.Controls.Add(main);
        frame.Controls.Add(header);
        if (portrait)
        {
            var art = Portrait();
            art.Dock = DockStyle.Left;
            art.Width = 230;
            frame.Controls.Add(art);
        }
        bar.SendToBack();
        dialog.Controls.Add(frame);
        return dialog;
    }

    private void Notice(string title, string text)
    {
        using var dialog = Hologram(title, text, "CERRAR");
        dialog.ShowDialog(this);
    }

    private bool Confirm(string title, string text, string action)
    {
        using var dialog = Hologram(title, text, action);
        var frame = dialog.Controls[0];
        var bar = frame.Controls.OfType<FlowLayoutPanel>().Single();
        var cancel = Button("CANCELAR", () => dialog.DialogResult = DialogResult.Cancel, 160);
        bar.Controls.Add(cancel);
        dialog.CancelButton = cancel;
        dialog.ActiveControl = cancel;
        return dialog.ShowDialog(this) == DialogResult.OK;
    }
}
