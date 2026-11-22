using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsForms;

// Prototipo técnico v2: interfaz adaptable, navegación y controles de exploración.
public sealed class IrisGameForm : Form
{
    private static readonly Color Background = Color.FromArgb(7, 15, 29);
    private static readonly Color Surface = Color.FromArgb(15, 31, 51);
    private static readonly Color Cyan = Color.FromArgb(83, 220, 246);
    private static readonly Color Muted = Color.FromArgb(169, 191, 210);
    private readonly System.Windows.Forms.Timer startup = new() { Interval = 40 };
    private readonly System.Windows.Forms.Timer voiceHint = new() { Interval = 1300 };
    private readonly Panel root = new() { Dock = DockStyle.Fill };
    private readonly Label footer = new() { Dock = DockStyle.Bottom, Height = 38, ForeColor = Cyan, TextAlign = ContentAlignment.MiddleCenter };
    private GameBoard? board;
    private int progress, room = 1, x = 1, y = 3, hp = 100, kits;
    private bool robotDefeated, circuitRestored, tutorialJarvisUsed;
    private int remainingPa, robotHp, robotReservedPa, turnNumber, jarvisCharges = 6;
    private readonly List<string> actions = new();
    private readonly string[] rooms = { "MANTENIMIENTO", "ENTRENAMIENTO", "ACCESO PRINCIPAL" };
    private readonly Font bodyFont = new("Segoe UI", 12);

    public IrisGameForm()
    {
        Text = "NEXUS — PROTOCOLO IRIS";
        MinimumSize = new Size(900, 640);
        ClientSize = new Size(1180, 760);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Background;
        ForeColor = Color.White;
        KeyPreview = true;
        Controls.Add(root);
        startup.Tick += (_, _) =>
        {
            progress = Math.Min(100, progress + 2);
            if (root.Controls.OfType<ProgressBar>().FirstOrDefault() is { } bar) bar.Value = progress;
            if (progress == 100) { startup.Stop(); Menu(); }
        };
        Splash();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (board is not null)
        {
            int dx = keyData is Keys.A or Keys.Left ? -1 : keyData is Keys.D or Keys.Right ? 1 : 0;
            int dy = keyData is Keys.W or Keys.Up ? -1 : keyData is Keys.S or Keys.Down ? 1 : 0;
            if (keyData == Keys.E) { Interact(); return true; }
            if (dx != 0 || dy != 0)
            {
                int nx = Math.Clamp(x + dx, 0, 8), ny = Math.Clamp(y + dy, 0, 6);
                if (!(room == 2 && !robotDefeated && nx == 5 && ny == 3))
                { x = nx; y = ny; board.Invalidate(); UpdateFooter(); }
                return true;
            }
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void Page(string title, string subtitle)
    {
        board = null;
        root.Controls.Clear();
        var header = new TableLayoutPanel { Dock = DockStyle.Top, Height = 155, ColumnCount = 1, RowCount = 2, BackColor = Background };
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        header.Controls.Add(new Label { Text = title, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomCenter,
            Font = new Font("Segoe UI", 32, FontStyle.Bold), ForeColor = Cyan }, 0, 0);
        header.Controls.Add(new Label { Text = subtitle, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 13), ForeColor = Muted }, 0, 1);
        root.Controls.Add(header);
    }

    private static Button ActionButton(string title, EventHandler onClick)
    {
        var button = new Button { Text = title, Width = 300, Height = 50, Margin = new Padding(7),
            FlatStyle = FlatStyle.Flat, BackColor = Surface, ForeColor = Color.White,
            Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
        button.FlatAppearance.BorderColor = Cyan;
        button.Click += onClick;
        return button;
    }

    private FlowLayoutPanel ButtonArea()
    {
        var area = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, AutoScroll = true, Padding = new Padding(12), BackColor = Background };
        root.Controls.Add(area);
        area.BringToFront();
        return area;
    }

    private void Splash()
    {
        Page("N E X U S", "PROTOCOLO IRIS · INICIALIZANDO SISTEMAS");
        var bar = new ProgressBar { Dock = DockStyle.Bottom, Height = 20, Maximum = 100 };
        root.Controls.Add(bar);
        startup.Start();
    }

    private void Menu()
    {
        Page("N E X U S", "PROTOCOLO IRIS · AÑO 2187");
        var center = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
        center.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        center.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 335));
        center.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.Controls.Add(center);
        center.BringToFront();
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, AutoScroll = true, Padding = new Padding(8, 25, 8, 8) };
        center.Controls.Add(buttons, 1, 0);
        buttons.Controls.Add(ActionButton("NUEVA PARTIDA", (_, _) => PhaseSelect()));
        buttons.Controls.Add(ActionButton("CARGAR PARTIDA", (_, _) => Notice("El guardado en SQL Server aún no está implementado.")));
        buttons.Controls.Add(ActionButton("CONFIGURACIÓN", (_, _) => Settings()));
        buttons.Controls.Add(ActionButton("SALIR", (_, _) => Close()));
    }

    private void Settings()
    {
        Page("CONFIGURACIÓN", "AUDIO · PANTALLA · CONTROLES");
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(90, 15, 90, 25) };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        root.Controls.Add(content);
        content.BringToFront();
        content.Controls.Add(new Label { Text = "VOLUMEN GENERAL (CONTROL PROVISIONAL)", Dock = DockStyle.Fill,
            Font = bodyFont, ForeColor = Muted, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        content.Controls.Add(new TrackBar { Dock = DockStyle.Fill, Minimum = 0, Maximum = 100, Value = 75, TickFrequency = 10 }, 0, 1);
        content.Controls.Add(new Label { Text = "Mover: WASD / Flechas    ·    Interactuar: E    ·    Pausa: botón en pantalla",
            Dock = DockStyle.Fill, Font = bodyFont, ForeColor = Muted, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        var back = ActionButton("VOLVER", (_, _) => Menu());
        content.Controls.Add(back, 0, 4);
    }

    private void PhaseSelect()
    {
        Page("SELECCIÓN DE FASE", "Completa una fase para desbloquear la siguiente");
        var area = ButtonArea();
        area.Controls.Add(ActionButton("FASE 0 · TUTORIAL", (_, _) => Introduction()));
        foreach (var name in new[] { "FASE 1 · SEÑAL PERDIDA", "FASE 2 · PROTOCOLO IRIS", "FASE 3 · ÚLTIMA TRANSMISIÓN" })
        {
            var b = ActionButton("🔒 " + name, (_, _) => Notice("Fase bloqueada."));
            b.Enabled = false;
            area.Controls.Add(b);
        }
        area.Controls.Add(ActionButton("VOLVER", (_, _) => Menu()));
    }

    private void Introduction()
    {
        room = 1; x = 1; y = 3; hp = 100; kits = 0; robotDefeated = circuitRestored = tutorialJarvisUsed = false; jarvisCharges = 6;
        Page("FASE 0 · TUTORIAL", "INTRODUCCIÓN");
        var narrative = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(65), Font = new Font("Segoe UI", 17), ForeColor = Muted,
            Text = "Año 2187. La nave IRIS lleva un mes sin responder.\n\n" +
                "La red NEXUS te envía a investigar la pérdida de contacto. " +
                "Entrarás por el módulo de mantenimiento.\n\n" +
                "JARVIS: «Te acompañaré durante la misión, cadete. Mantente alerta.»" };
        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 90 };
        var next = ActionButton("CONTINUAR", (_, _) => Explore());
        next.Location = new Point(18, 18);
        bottom.Controls.Add(next);
        root.Controls.Add(narrative);
        root.Controls.Add(bottom);
        bottom.BringToFront();
    }

    private void Explore()
    {
        Page($"FASE 0 · SALA {room:00}", rooms[room - 1]);
        var bottom = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 112, ColumnCount = 2 };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        bottom.Controls.Add(ActionButton("INTERACTUAR (E)", (_, _) => Interact()), 0, 0);
        bottom.Controls.Add(ActionButton("PAUSA", (_, _) => Pause()), 1, 0);
        root.Controls.Add(bottom);
        footer.Text = "";
        root.Controls.Add(footer);
        footer.BringToFront();
        board = new GameBoard { Dock = DockStyle.Fill, BackColor = Background };
        board.Paint += PaintBoard;
        root.Controls.Add(board);
        board.BringToFront();
        UpdateFooter();
        board.Focus();
    }

    private void PaintBoard(object? sender, PaintEventArgs e)
    {
        if (board is null) return;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        int tile = Math.Max(35, Math.Min((board.ClientSize.Width - 100) / 9, (board.ClientSize.Height - 45) / 7));
        int left = (board.ClientSize.Width - tile * 9) / 2;
        int top = (board.ClientSize.Height - tile * 7) / 2;
        using var floor = new SolidBrush(Color.FromArgb(18, 33, 52));
        using var wall = new SolidBrush(Color.FromArgb(32, 49, 70));
        using var gridPen = new Pen(Color.FromArgb(46, 75, 102));
        for (int ix = 0; ix < 9; ix++)
        for (int iy = 0; iy < 7; iy++)
        {
            var r = new Rectangle(left + ix * tile, top + iy * tile, tile, tile);
            e.Graphics.FillRectangle(ix == 0 || ix == 8 || iy == 0 || iy == 6 ? wall : floor, r);
            e.Graphics.DrawRectangle(gridPen, r);
        }
        void Marker(int px, int py, Color color, string label)
        {
            var r = new Rectangle(left + px * tile + tile / 5, top + py * tile + tile / 5, tile * 3 / 5, tile * 3 / 5);
            using var brush = new SolidBrush(color);
            e.Graphics.FillRectangle(brush, r);
            using var font = new Font("Segoe UI", Math.Max(10, tile / 5), FontStyle.Bold);
            TextRenderer.DrawText(e.Graphics, label, font, r, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        if (room == 1 && kits == 0) Marker(4, 3, Color.SeaGreen, "+");
        if (room == 2 && !robotDefeated) Marker(5, 3, Color.Firebrick, "R");
        if (room == 3 && !circuitRestored) Marker(5, 3, Color.DarkOrange, "C");
        Marker(7, 3, Color.MediumPurple, ">");
        DrawOrb(e.Graphics, left + Math.Max(0, x - 1) * tile + tile / 2, top + y * tile + tile / 2, tile);
        DrawCadet(e.Graphics, left + x * tile + tile / 2, top + y * tile + tile / 2, tile);
    }

    private static void DrawOrb(Graphics g, int x, int y, int tile)
    {
        using var glow = new SolidBrush(Color.FromArgb(55, 45, 213, 239));
        using var core = new SolidBrush(Cyan);
        g.FillEllipse(glow, x - tile / 3, y - tile / 3, tile * 2 / 3, tile * 2 / 3);
        g.FillEllipse(core, x - tile / 8, y - tile / 8, tile / 4, tile / 4);
    }

    private static void DrawCadet(Graphics g, int x, int y, int tile)
    {
        using var suit = new SolidBrush(Color.FromArgb(220, 230, 240));
        using var visor = new SolidBrush(Cyan);
        g.FillRectangle(suit, x - tile / 5, y - tile / 10, tile * 2 / 5, tile / 2);
        g.FillEllipse(suit, x - tile / 5, y - tile * 2 / 5, tile * 2 / 5, tile * 2 / 5);
        g.FillEllipse(visor, x - tile / 7, y - tile / 3, tile * 2 / 7, tile / 6);
    }

    private void UpdateFooter()
    {
        string goal = room switch
        {
            1 => kits > 0 ? "DIRÍGETE A LA SALIDA (>)" : "RECOGE EL KIT MÉDICO (+)",
            2 => robotDefeated ? "DIRÍGETE A LA SALIDA (>)" : "ACÉRCATE AL ROBOT (R)",
            _ => circuitRestored ? "DIRÍGETE A LA SALIDA (>)" : "RESTAURA EL CIRCUITO (C)"
        };
        footer.Text = $"OBJETIVO: {goal}    |    VIDA: {hp}/100    |    KITS: {kits}    |    WASD / FLECHAS · E";
    }

    private void Interact()
    {
        bool Near(int px, int py) => Math.Abs(x - px) + Math.Abs(y - py) <= 1;
        if (room == 1 && kits == 0 && Near(4, 3)) { kits = 1; Notice("Kit médico obtenido."); }
        else if (room == 2 && !robotDefeated && Near(5, 3)) EncounterPrompt();
        else if (room == 3 && !circuitRestored && Near(5, 3)) Circuit();
        else if (Near(7, 3))
        {
            if (room == 1 && kits > 0 || room == 2 && robotDefeated)
            { room++; x = 1; y = 3; Explore(); return; }
            if (room == 3 && circuitRestored) { PhaseComplete(); return; }
            Notice("Acceso bloqueado. Completa el objetivo.");
        }
        else Notice("No hay elementos interactivos al alcance.");
        UpdateFooter();
        board?.Invalidate();
    }

    private void EncounterPrompt()
    {
        if (MessageBox.Show("Amenaza detectada. ¿ENFRENTAR?", "NEXUS", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        robotHp = 40;
        remainingPa = 3;
        robotReservedPa = 0;
        turnNumber = 1;
        actions.Clear();
        Combat();
    }

    private void Combat()
    {
        Page("ENCUENTRO · ROBOT DE SEGURIDAD", $"TURNO {turnNumber}   ·   CADETE {hp}/100 HP (DAÑO 20)   ·   ROBOT {robotHp}/40 HP (DAÑO 10)");
        var area = ButtonArea();
        var summary = new Label
        {
            Text = $"PA DISPONIBLES: {remainingPa}     |     ATAQUES: {actions.Count(a => a == "ATACAR")}     DEFENSAS: {actions.Count(a => a == "DEFENDER")}     RESERVAS: {actions.Count(a => a == "RESERVAR")}     KITS: {kits}     JARVIS: {jarvisCharges}/6",
            ForeColor = Cyan, Font = bodyFont, AutoSize = true, Margin = new Padding(12)
        };
        area.Controls.Add(summary);
        void Choose(string action)
        {
            if (remainingPa <= 0) return;
            actions.Add(action);
            remainingPa--;
            Combat();
        }
        if (jarvisCharges > 0)
            area.Controls.Add(ActionButton($"JARVIS · PA ESPECIALES ({jarvisCharges})", (_, _) => JarvisSelector()));
        if (remainingPa > 0)
        {
            if (kits > actions.Count(a => a == "KIT"))
            {
                var kit = ActionButton("USAR KIT · 1 PA (+30 HP)", (_, _) => Choose("KIT"));
                kit.Enabled = hp < 100;
                area.Controls.Add(kit);
            }
            area.Controls.Add(ActionButton("ATACAR · 1 PA", (_, _) => Choose("ATACAR")));
            area.Controls.Add(ActionButton("DEFENDER · 1 PA", (_, _) => Choose("DEFENDER")));
            area.Controls.Add(ActionButton("RESERVAR · 1 PA", (_, _) => Choose("RESERVAR")));
        }
        if (actions.Count > 0)
            area.Controls.Add(ActionButton("DESHACER ÚLTIMA ACCIÓN", (_, _) =>
            {
                actions.RemoveAt(actions.Count - 1);
                remainingPa++;
                Combat();
            }));
        if (remainingPa == 0)
            area.Controls.Add(ActionButton("CONFIRMAR TURNO", (_, _) => { if (room == 2 && !tutorialJarvisUsed) { Notice("TUTORIAL: utiliza al menos un PA especial de JARVIS."); return; } ResolveCombat(); }));
    }

    private void JarvisSelector()
    {
        using var dialog = new Form { Text = "NEXUS · JARVIS", ClientSize = new Size(450, 330),
            StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false, BackColor = Background, ForeColor = Color.White };
        int selected = 0;
        var info = new Label { Text = "Selecciona las cargas una por una. Cada carga concede +1 PA.",
            Dock = DockStyle.Top, Height = 75, ForeColor = Muted, TextAlign = ContentAlignment.MiddleCenter };
        var count = new Label { Dock = DockStyle.Top, Height = 55, ForeColor = Cyan,
            Font = new Font("Segoe UI", 18, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
        void UpdateCount() => count.Text = $"PA seleccionados: {selected} / {jarvisCharges}";
        UpdateCount();
        var area = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), AutoScroll = true };
        foreach (var (title, change) in new[] { ("+ 1 PA", 1), ("- 1 PA", -1) })
        {
            var button = ActionButton(title, (_, _) => { selected = Math.Clamp(selected + change, 0, jarvisCharges); UpdateCount(); });
            button.Width = 195;
            area.Controls.Add(button);
        }
        var confirm = ActionButton("CONFIRMAR", (_, _) => { if (selected > 0) dialog.DialogResult = DialogResult.OK; });
        confirm.Width = 195;
        var cancel = ActionButton("CANCELAR", (_, _) => dialog.DialogResult = DialogResult.Cancel);
        cancel.Width = 195;
        area.Controls.Add(confirm);
        area.Controls.Add(cancel);
        dialog.Controls.Add(area);
        dialog.Controls.Add(count);
        dialog.Controls.Add(info);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        jarvisCharges -= selected;
        remainingPa += selected;
        tutorialJarvisUsed = true;
        Combat();
    }

    private void ResolveCombat()
    {
        int playerAttacks = actions.Count(a => a == "ATACAR");
        int playerDefenses = actions.Count(a => a == "DEFENDER");
        int playerReserved = actions.Count(a => a == "RESERVAR");
        int usedKits = actions.Count(a => a == "KIT");
        int healing = Math.Min(100 - hp, usedKits * 30);
        hp += healing;
        kits -= usedKits;
        int robotPa = 3 + robotReservedPa;
        robotReservedPa = 0;
        var enemyActions = new List<string>();
        // The robot evaluates its own survival and the player's visible defenses.
        // Its choices are not fixed; each action consumes one PA.
        for (int i = 0; i < robotPa; i++)
        {
            int roll = Random.Shared.Next(100);
            if (robotHp <= 20 && playerAttacks > 0 && roll < 65)
                enemyActions.Add("DEFENDER");
            else if (playerDefenses >= 2 && roll < 45)
                enemyActions.Add("RESERVAR");
            else if (roll < 62)
                enemyActions.Add("ATACAR");
            else if (roll < 82)
                enemyActions.Add("DEFENDER");
            else
                enemyActions.Add("RESERVAR");
        }
        int robotAttacks = enemyActions.Count(a => a == "ATACAR");
        int robotDefenses = enemyActions.Count(a => a == "DEFENDER");
        robotReservedPa = enemyActions.Count(a => a == "RESERVAR");
        int effectiveAttacks = Math.Max(0, playerAttacks - robotDefenses);
        int effectiveRobotAttacks = Math.Max(0, robotAttacks - playerDefenses);
        int dealt = Math.Min(robotHp, effectiveAttacks * 20);
        int received = Math.Min(hp, effectiveRobotAttacks * 10);
        robotHp -= dealt;
        hp -= received;
        int nextPa = 3 + playerReserved;
        int nextRobotPa = 3 + robotReservedPa;
        Page("RESULTADO DEL TURNO", "RESOLUCIÓN TÁCTICA");
        var results = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3,
            Padding = new Padding(36, 12, 36, 12), BackColor = Background };
        results.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        results.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        results.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
        results.RowStyles.Add(new RowStyle(SizeType.Percent, 32));
        results.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        Label ResultCard(string heading, string details, Color accent)
        {
            return new Label { Text = heading + "\n\n" + details, Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = accent,
                BackColor = Surface, TextAlign = ContentAlignment.MiddleCenter, Margin = new Padding(9),
                BorderStyle = BorderStyle.FixedSingle };
        }
        results.Controls.Add(ResultCard("CADETE · TURNO " + turnNumber,
            $"⚔ ATACAR: {playerAttacks}\n🛡 DEFENDER: {playerDefenses}\n↗ RESERVAR: {playerReserved}\n✚ KIT: {usedKits}",
            Cyan), 0, 0);
        results.Controls.Add(ResultCard("ROBOT DE SEGURIDAD",
            $"⚔ ATACAR: {robotAttacks}\n🛡 DEFENDER: {robotDefenses}\n↗ RESERVAR: {robotReservedPa}",
            Color.FromArgb(244, 119, 125)), 1, 0);
        results.Controls.Add(ResultCard("DAÑO Y CURACIÓN",
            $"Infligido: {dealt} HP    Recibido: {received} HP\nCuración: +{healing} HP\nBloqueos: Cadete {Math.Min(robotAttacks, playerDefenses)} · Robot {Math.Min(playerAttacks, robotDefenses)}",
            Color.White), 0, 1);
        results.Controls.Add(ResultCard("ESTADO ACTUAL",
            $"Cadete: {hp}/100 HP    Robot: {robotHp}/40 HP\nPróximo turno: Cadete {nextPa} PA · Robot {nextRobotPa} PA",
            Color.White), 1, 1);
        var hint = new Label { Text = "Revisa las acciones de ambos combatientes y pulsa CONTINUAR cuando estés listo.",
            Dock = DockStyle.Fill, ForeColor = Muted, TextAlign = ContentAlignment.MiddleCenter };
        results.Controls.Add(hint, 0, 2);
        results.SetColumnSpan(hint, 2);
        root.Controls.Add(results);
        results.BringToFront();
        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 85 };
        var next = ActionButton("CONTINUAR", (_, _) =>
        {
            if (hp <= 0) { Failure(); return; }
            if (robotHp <= 0) { robotDefeated = true; Explore(); return; }
            remainingPa = nextPa;
            turnNumber++;
            actions.Clear();
            Combat();
        });
        next.Location = new Point(30, 14);
        bottom.Controls.Add(next);
        root.Controls.Add(bottom);
        bottom.BringToFront();
    }

    private void Failure()
    {
        Page("MISIÓN FALLIDA", "EL CADETE HA SIDO ELIMINADO");
        var area = ButtonArea();
        area.Controls.Add(ActionButton("REINTENTAR FASE", (_, _) => Introduction()));
        area.Controls.Add(ActionButton("SALIR AL MENÚ", (_, _) => Menu()));
    }

    private void Circuit()
    {
        using var puzzle = new CircuitPuzzle();
        if (puzzle.ShowDialog(this) == DialogResult.OK && puzzle.Solved)
        {
            circuitRestored = true;
            UpdateFooter();
            board?.Invalidate();
        }
    }

    private void PhaseComplete()
    {
        Page("FASE 0 COMPLETADA", "SISTEMAS DE ACCESO RESTABLECIDOS");
        var area = ButtonArea();
        area.Controls.Add(ActionButton("CONTINUAR · FASE 1 (EN DESARROLLO)", (_, _) => Notice("La Fase 1 todavía no está implementada. No se puede avanzar aún.")));
        area.Controls.Add(ActionButton("VOLVER AL MENÚ", (_, _) => Menu()));
    }

    private void Pause()
    {
        if (MessageBox.Show("¿Volver al menú? El guardado aún no está disponible.",
            "PAUSA", MessageBoxButtons.YesNo) == DialogResult.Yes) Menu();
    }

    private static void Notice(string text) => MessageBox.Show(text, "NEXUS — PROTOCOLO IRIS");

    private sealed class GameBoard : Panel
    {
        public GameBoard() { DoubleBuffered = true; TabStop = true; }
    }
}
