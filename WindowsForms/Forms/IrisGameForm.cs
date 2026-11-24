namespace WindowsForms;

public sealed partial class IrisGameForm : Form
{
    private static readonly Color Background = Color.FromArgb(6, 15, 28);
    private static readonly Color Surface = Color.FromArgb(13, 34, 51);
    private static readonly Color Cyan = Color.FromArgb(88, 224, 249);
    private static readonly Color Muted = Color.FromArgb(175, 200, 217);
    private static readonly Color Red = Color.FromArgb(255, 112, 121);
    private readonly NarrativeCatalog catalog = new();
    private readonly AudioManager audio = new();
    private readonly SaveRepository saves = new(Environment.GetEnvironmentVariable("NEXUS_SQL_CONNECTION")
        ?? SaveRepository.LocalConnection);
    private readonly Random combatRandom;
    private readonly Panel root = new() { Dock = DockStyle.Fill };
    private readonly System.Windows.Forms.Timer loading = new() { Interval = 25 };
    private GameSession? session;
    private CombatTurn? combat;
    private RoomEnemy? combatEnemy;
    private Panel? board;
    private Button? faceEnemy;
    private Label? interactHint;
    private Action? refreshNarrative;
    private bool busy;
    private bool completionSaved;
    private bool exiting;
    private bool inventoryAvailable;
    private GameSession Session => session ?? throw new InvalidOperationException();
    private CombatTurn Encounter => combat ?? throw new InvalidOperationException();
    private static readonly string[] PhaseNames = ["TUTORIAL", "SEÑAL PERDIDA", "PROTOCOLO IRIS", "ÚLTIMA TRANSMISIÓN"];

    public IrisGameForm() : this(Random.Shared) { }

    internal IrisGameForm(Random combatRandom)
    {
        this.combatRandom = combatRandom;
        Text = "NEXUS — PROTOCOLO IRIS";
        MinimumSize = new Size(940, 660);
        ClientSize = new Size(1180, 760);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Background;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 11);
        AutoScaleMode = AutoScaleMode.Dpi;
        KeyPreview = true;
        Controls.Add(root);
        FormClosing += (_, e) =>
        {
            if (exiting || e.CloseReason != CloseReason.UserClosing) return;
            e.Cancel = busy || !AskToExit();
        };
        Loading("PROTOCOLO IRIS", "INICIALIZANDO SISTEMAS", () =>
        {
            audio.QueueVoice(catalog.Line(-1, 0, "primer_inicio_tras_carga_100").Id);
            MainMenu();
        });
    }

    protected override bool ProcessCmdKey(ref Message message, Keys key)
    {
        bool repeated = (message.LParam.ToInt64() & (1L << 30)) != 0;
        if (!busy && inventoryAvailable && key == Keys.I)
        {
            if (!repeated) OpenInventory();
            return true;
        }
        if (board is null || busy) return base.ProcessCmdKey(ref message, key);
        if (key == Keys.E)
        {
            if (!repeated) Interact();
            return true;
        }

        var movement = key switch
        {
            Keys.A or Keys.Left => new Point(-1, 0),
            Keys.D or Keys.Right => new Point(1, 0),
            Keys.W or Keys.Up => new Point(0, -1),
            Keys.S or Keys.Down => new Point(0, 1),
            _ => Point.Empty
        };
        if (movement.IsEmpty) return base.ProcessCmdKey(ref message, key);
        if (repeated) return true;
        Session.Move(movement.X, movement.Y);
        ObserveRoom();
        UpdateExploration();
        return true;
    }

    private Panel Page(string title, string subtitle, Color? accent = null)
    {
        board = null;
        inventoryAvailable = false;
        refreshNarrative = null;
        root.SuspendLayout();
        var previous = root.Controls.Cast<Control>().ToArray();
        root.Controls.Clear();
        foreach (var control in previous) control.Dispose();
        var header = new Panel { Dock = DockStyle.Top, Height = 103, BackColor = Background };
        var label = Caption(title, 29, accent ?? Cyan);
        label.Height = 62;
        label.Dock = DockStyle.Top;
        var detail = Caption(subtitle, 11, Muted);
        detail.Dock = DockStyle.Fill;
        header.Controls.Add(detail);
        header.Controls.Add(label);
        var body = new Panel { Dock = DockStyle.Fill, BackColor = Background, Padding = new Padding(22, 10, 22, 16) };
        root.Controls.Add(body);
        root.Controls.Add(header);
        root.ResumeLayout();
        return body;
    }

    private static Label Caption(string text, int size = 12, Color? color = null) => new()
    {
        Text = text, ForeColor = color ?? Color.White, TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", size, size >= 17 ? FontStyle.Bold : FontStyle.Regular)
    };

    private static Button Button(string title, Action action, int width = 245, Color? accent = null)
    {
        var button = new Button
        {
            Text = title, Width = width, Height = 44, Margin = new Padding(5), FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Surface, ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderColor = accent ?? Cyan;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(19, 67, 87);
        button.Click += (_, _) => action();
        return button;
    }

    private static FlowLayoutPanel Stack(Control parent)
    {
        var column = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            AutoScroll = true, Padding = new Padding(15)
        };
        parent.Controls.Add(column);
        return column;
    }

    private static FlowLayoutPanel Bar(Control parent)
    {
        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 57, WrapContents = false, AutoScroll = true };
        parent.Controls.Add(bar);
        return bar;
    }

    private void Loading(string title, string subtitle, Action next)
    {
        var page = Page(title, subtitle);
        var progress = new ProgressBar { Dock = DockStyle.Bottom, Height = 22 };
        var percent = Caption("0 %", 18, Cyan);
        percent.Dock = DockStyle.Fill;
        page.Controls.Add(percent);
        page.Controls.Add(progress);
        progress.SendToBack();
        loading.Tick += Tick;
        loading.Start();

        void Tick(object? sender, EventArgs args)
        {
            if (progress.Value < 100)
            {
                progress.Value += 5;
                percent.Text = $"{progress.Value} %";
                return;
            }
            loading.Stop();
            loading.Tick -= Tick;
            next();
        }
    }

    private void MainMenu()
    {
        session = null;
        combat = null;
        combatEnemy = null;
        var page = Page("N E X U S", "PROTOCOLO IRIS   /   2187");
        var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        page.Controls.Add(split);
        var rail = new Panel { Dock = DockStyle.Fill };
        split.Controls.Add(rail, 0, 0);
        var menu = Stack(rail);
        var labels = catalog.Labels("MENU_PRINCIPAL");
        menu.Controls.Add(Button(labels[0], Identify));
        menu.Controls.Add(Button(labels[1], () => _ = LoadScreen()));
        menu.Controls.Add(Button(labels[2], () => Settings(MainMenu)));
        menu.Controls.Add(Button(labels[3], ConfirmExit));
        var art = new ArtPanel { Dock = DockStyle.Fill };
        art.Paint += PaintShip;
        split.Controls.Add(art, 1, 0);
    }

    private void Identify()
    {
        var labels = catalog.Labels("NOMBRE");
        var column = Stack(Page(labels[0], labels[1]));
        var name = new TextBox { Width = 360, MaxLength = 60, BackColor = Surface, ForeColor = Color.White };
        column.Controls.Add(name);
        var confirm = Button(labels[2], () => _ = CreateGame(name.Text.Trim()));
        confirm.Enabled = false;
        name.TextChanged += (_, _) => confirm.Enabled = !string.IsNullOrWhiteSpace(name.Text);
        name.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { confirm.PerformClick(); e.SuppressKeyPress = true; } };
        column.Controls.Add(confirm);
        column.Controls.Add(Button(labels[3], MainMenu));
        name.Focus();
    }

    private async Task CreateGame(string name)
    {
        var newGame = new GameSession { Id = Guid.NewGuid(), Name = name };
        if (!await Database(() => saves.SaveAsync(newGame, create: true))) return;
        session = newGame;
        completionSaved = false;
        PhaseSelect();
    }

    private void PhaseSelect()
    {
        var labels = catalog.Labels("FASES");
        var page = Page(labels[0], Session.Name);
        var cards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
        for (int i = 0; i < PhaseNames.Length; i++)
        {
            int phase = i;
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            bool unlocked = Session.IsUnlocked(i);
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Margin = new Padding(6), BorderStyle = BorderStyle.FixedSingle };
            var title = Caption($"FASE {i}\n{PhaseNames[i]}", 16, unlocked ? Cyan : Muted);
            title.Dock = DockStyle.Fill;
            card.Controls.Add(title);
            var start = Button(unlocked ? labels[2] : labels[1], () =>
            {
                if (phase == 0 || Session.StartNextPhase(phase)) StartPhase();
            });
            start.Dock = DockStyle.Bottom;
            start.Enabled = unlocked && i <= GameSession.LastPlayablePhase && !Session.CompletedPhases.Contains(i);
            card.Controls.Add(start);
            cards.Controls.Add(card, i, 0);
        }

        var bar = Bar(page);
        bar.Controls.Add(Button(labels[3], MainMenu));
        page.Controls.Add(cards);
        bar.SendToBack();
    }

    private async Task LoadScreen()
    {
        IReadOnlyList<SavedGame> entries = [];
        if (!await Database(async () => entries = await saves.ListAsync())) return;
        var labels = catalog.Labels("CARGA");
        var page = Page(labels[0], entries.Count == 0 ? labels[4] : "SELECCIONA UN GUARDADO");
        var list = new ListBox { Dock = DockStyle.Fill, BackColor = Surface, ForeColor = Color.White };
        foreach (var entry in entries) list.Items.Add(entry);
        var bar = Bar(page);
        var load = Button(labels[1], () => { if (list.SelectedItem is SavedGame entry) _ = Resume(entry.Id); }, 145);
        var delete = Button(labels[2], () => { if (list.SelectedItem is SavedGame entry) _ = DeleteSave(entry); }, 145, Red);
        load.Enabled = delete.Enabled = false;
        list.SelectedIndexChanged += (_, _) => load.Enabled = delete.Enabled = list.SelectedItem is SavedGame;
        list.DoubleClick += (_, _) => load.PerformClick();
        bar.Controls.Add(load);
        bar.Controls.Add(delete);
        bar.Controls.Add(Button(labels[3], MainMenu, 145));
        page.Controls.Add(list);
        bar.SendToBack();
    }

    private async Task Resume(Guid id)
    {
        GameSession? loaded = null;
        if (!await Database(async () => loaded = await saves.LoadAsync(id))) return;
        session = loaded;
        completionSaved = Session.PhaseComplete;
        if (Session.MissionComplete) MissionCompleteScreen();
        else if (Session.PhaseComplete) PhaseCompleteScreen();
        else if (Session.Room == GameSession.FirstRoom(Session.Phase)
            && !Session.SeenEvents.Contains($"JARVIS_F{Session.Phase}_INICIO")) StartPhase();
        else Explore();
    }

    private async Task DeleteSave(SavedGame entry)
    {
        if (!Confirm("ELIMINAR", $"¿Eliminar la partida de {entry.Name}?", "ELIMINAR")) return;
        if (await Database(() => saves.DeleteAsync(entry.Id))) await LoadScreen();
    }

    private async Task<bool> Database(Func<Task> action)
    {
        if (busy) return false;
        busy = true;
        root.Enabled = false;
        UseWaitCursor = true;
        try
        {
            await action();
            return true;
        }
        catch (Exception error) when (error is Microsoft.Data.SqlClient.SqlException or InvalidDataException
            or System.Text.Json.JsonException or ArgumentException)
        {
            System.Diagnostics.Trace.TraceError("{0}", error);
            Notice("PARTIDA NO DISPONIBLE", error is InvalidDataException ? error.Message
                : "No se pudo acceder a la partida en SQL Server. Comprueba la conexión y vuelve a intentarlo.");
            return false;
        }
        finally
        {
            busy = false;
            root.Enabled = true;
            UseWaitCursor = false;
        }
    }

    private void Settings(Action back)
    {
        var labels = catalog.Labels("CONFIG");
        var column = Stack(Page(labels[0], "WASD / FLECHAS   ·   E INTERACTUAR   ·   I INVENTARIO"));
        TrackBar Volume(string title, int value)
        {
            var label = Caption($"{title}  ·  {value} %", 11);
            label.AutoSize = true;
            var track = new TrackBar { Width = 360, Minimum = 0, Maximum = 100, Value = value, TickFrequency = 10 };
            track.ValueChanged += (_, _) => label.Text = $"{title}  ·  {track.Value} %";
            column.Controls.Add(label);
            column.Controls.Add(track);
            return track;
        }

        var music = Volume(labels[1], audio.Music);
        var effects = Volume(labels[2], audio.Effects);
        var voice = Volume(labels[3], audio.Voice);
        var fullscreen = new CheckBox { Text = "PANTALLA COMPLETA", AutoSize = true, Checked = FormBorderStyle == FormBorderStyle.None };
        column.Controls.Add(fullscreen);
        column.Controls.Add(Button(labels[4], () =>
        {
            audio.Music = music.Value;
            audio.Effects = effects.Value;
            audio.Voice = voice.Value;
            if (fullscreen.Checked != (FormBorderStyle == FormBorderStyle.None))
            {
                WindowState = FormWindowState.Normal;
                FormBorderStyle = fullscreen.Checked ? FormBorderStyle.None : FormBorderStyle.Sizable;
                WindowState = fullscreen.Checked ? FormWindowState.Maximized : FormWindowState.Normal;
            }

            back();
        }));
        column.Controls.Add(Button(labels[5], back));
    }

    private void ConfirmExit()
    {
        if (!AskToExit()) return;
        exiting = true;
        Close();
    }

    private bool AskToExit()
    {
        var labels = catalog.Labels("CONFIRMAR_SALIDA");
        return Confirm(labels[0], "Se perderá el progreso no guardado.", labels[2]);
    }

    private void StartPhase()
    {
        completionSaved = false;
        Loading($"FASE {Session.Phase} · {PhaseNames[Session.Phase]}", "CARGANDO FASE", () =>
        {
            Speak(Session.Room, $"entrada_fase_{Session.Phase}");
            Explore();
        });
    }

    private void Pause()
    {
        var labels = catalog.Labels("PAUSA");
        var page = Page(labels[0], $"FASE {Session.Phase}  /  SALA {Session.Room:00}  /  {Session.Name}");
        var bar = Bar(page);
        var status = Caption("", 11, Cyan);
        status.Width = 250;
        status.Height = 44;
        bar.Controls.Add(Button(labels[3], () => _ = SaveFromPause(status)));
        bar.Controls.Add(status);
        var composition = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3 };
        composition.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        composition.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420));
        composition.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        composition.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        composition.RowStyles.Add(new RowStyle(SizeType.Absolute, 218));
        composition.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        var frame = new ArtPanel { Dock = DockStyle.Fill, BackColor = Surface };
        frame.Paint += PaintHolographicFrame;
        var column = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            Padding = new Padding(28, 25, 28, 20), BackColor = Color.Transparent
        };
        column.Controls.Add(Button(catalog.Labels("RESULTADO")[4], Explore, 354));
        column.Controls.Add(Button(labels[2], () => Settings(Pause), 354));
        column.Controls.Add(Button(labels[4], () =>
        {
            if (Confirm("¿SALIR AL MENÚ?", "Se perderá el progreso no guardado.", labels[4])) MainMenu();
        }, 354));
        frame.Controls.Add(column);
        composition.Controls.Add(frame, 1, 1);
        page.Controls.Add(composition);
        bar.SendToBack();
    }

    private async Task SaveFromPause(Label status)
    {
        if (await Database(() => saves.SaveAsync(Session))) status.Text = "PARTIDA GUARDADA";
    }

    private void PhaseCompleteScreen()
    {
        var labels = catalog.Labels("FINAL_FASE");
        var page = Page(labels[0], $"FASE {Session.Phase}  /  {PhaseNames[Session.Phase]}");
        var bar = Bar(page);
        var art = Portrait();
        art.Dock = DockStyle.Fill;
        art.Paint += PaintHolographicFrame;
        page.Controls.Add(art);
        var next = Button(labels[1], PhaseSelect);
        next.Enabled = completionSaved;
        bar.Controls.Add(next);
        bar.Controls.Add(Button(labels[2], () => _ = SaveCompletion(MainMenu)));
        bar.SendToBack();
        if (!completionSaved) _ = SaveCompletion(() => next.Enabled = true);
    }

    private async Task SaveCompletion(Action next)
    {
        if (!await Database(() => saves.SaveAsync(Session))) return;
        completionSaved = true;
        next();
    }

    private void MissionCompleteScreen()
    {
        var labels = catalog.Labels("FINAL_JUEGO");
        var page = Page(labels[0], labels[1], Color.Orange);
        var art = Portrait();
        art.Dock = DockStyle.Fill;
        art.Paint += PaintHolographicFrame;
        page.Controls.Add(art);
        var bar = Bar(page);
        bar.Controls.Add(Button(labels[2], () =>
        {
            if (completionSaved) MainMenu();
            else _ = SaveCompletion(MainMenu);
        }));
        bar.SendToBack();
        if (!completionSaved) _ = SaveCompletion(() => { });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) loading.Dispose();
        base.Dispose(disposing);
    }

    private sealed class ArtPanel : Panel
    {
        public ArtPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }
}
