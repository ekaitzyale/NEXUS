using System.Drawing;
using System.Windows.Forms;

namespace WindowsForms;

// Primera versión jugable: flujo inicial y exploración del tutorial.
// No reemplaza los recursos artísticos finales ni simula persistencia.
public sealed class IrisGameForm : Form
{
    private readonly Color cyan = Color.FromArgb(92, 220, 245);
    private readonly Color bg = Color.FromArgb(9, 17, 31);
    private readonly System.Windows.Forms.Timer loadingTimer = new() { Interval = 35 };
    private readonly ProgressBar progress = new() { Minimum = 0, Maximum = 100 };
    private Panel? board;
    private Label? status;
    private int playerX = 1, playerY = 3, room = 1;
    private bool hasKit, robotDefeated, circuitRestored;
    private const int Tile = 48;
    private readonly string[] roomNames = { "MANTENIMIENTO", "ENTRENAMIENTO", "ACCESO PRINCIPAL" };

    public IrisGameForm()
    {
        Text = "NEXUS — PROTOCOLO IRIS";
        ClientSize = new Size(1060, 700);
        MinimumSize = new Size(880, 620);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = bg;
        ForeColor = Color.White;
        KeyPreview = true;
        KeyDown += OnKeyDown;
        loadingTimer.Tick += (_, _) =>
        {
            progress.Value = Math.Min(100, progress.Value + 2);
            if (progress.Value == 100)
            {
                loadingTimer.Stop();
                Menu();
            }
        };
        Splash();
    }

    private void ResetScreen(string heading, string subtitle)
    {
        Controls.Clear();
        BackColor = bg;
        var title = new Label { Text = heading, Dock = DockStyle.Top, Height = 110,
            TextAlign = ContentAlignment.MiddleCenter, ForeColor = cyan,
            Font = new Font("Segoe UI", 33, FontStyle.Bold) };
        var sub = new Label { Text = subtitle, Dock = DockStyle.Top, Height = 54,
            TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.LightSteelBlue,
            Font = new Font("Segoe UI", 13) };
        Controls.Add(sub);
        Controls.Add(title);
    }

    private FlowLayoutPanel Buttons()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 220,
            FlowDirection = FlowDirection.TopDown, WrapContents = false,
            Padding = new Padding(20), AutoScroll = true };
        Controls.Add(panel);
        return panel;
    }

    private void AddButton(FlowLayoutPanel panel, string text, Action action)
    {
        var button = new Button { Text = text, Width = 320, Height = 45,
            FlatStyle = FlatStyle.Flat, ForeColor = Color.White,
            BackColor = Color.FromArgb(22, 56, 81), Margin = new Padding(8) };
        button.FlatAppearance.BorderColor = cyan;
        button.Click += (_, _) => action();
        panel.Controls.Add(button);
    }

    private void Splash()
    {
        ResetScreen("N E X U S", "PROTOCOLO IRIS · INICIALIZANDO SISTEMAS");
        progress.Dock = DockStyle.Bottom;
        progress.Height = 22;
        progress.Value = 0;
        Controls.Add(progress);
        loadingTimer.Start();
    }

    private void Menu()
    {
        ResetScreen("N E X U S", "PROTOCOLO IRIS · AÑO 2187");
        var panel = Buttons();
        AddButton(panel, "NUEVA PARTIDA", SelectPhase);
        AddButton(panel, "CARGAR PARTIDA", () => Info("El guardado se implementará con SQL Server."));
        AddButton(panel, "CONFIGURACIÓN", Settings);
        AddButton(panel, "SALIR", Close);
    }

    private void Settings()
    {
        ResetScreen("CONFIGURACIÓN", "Audio y opciones de pantalla");
        var panel = Buttons();
        AddButton(panel, "VOLVER", Menu);
        var label = new Label { Text = "Volumen general (ajuste visual provisional)",
            Dock = DockStyle.Top, Height = 45, TextAlign = ContentAlignment.MiddleCenter };
        Controls.Add(label);
        var slider = new TrackBar { Minimum = 0, Maximum = 100, Value = 75,
            Dock = DockStyle.Top, TickFrequency = 10 };
        Controls.Add(slider);
        slider.BringToFront();
    }

    private void SelectPhase()
    {
        ResetScreen("SELECCIÓN DE FASE", "Las fases posteriores se desbloquean al progresar");
        var panel = Buttons();
        AddButton(panel, "FASE 0 · TUTORIAL", BeginTutorial);
        AddButton(panel, "FASE 1 · BLOQUEADA", () => Info("Completa el tutorial primero."));
        AddButton(panel, "FASE 2 · BLOQUEADA", () => Info("Fase bloqueada."));
        AddButton(panel, "FASE 3 · BLOQUEADA", () => Info("Fase bloqueada."));
        AddButton(panel, "VOLVER AL MENÚ", Menu);
    }

    private void BeginTutorial()
    {
        room = 1; playerX = 1; playerY = 3;
        hasKit = robotDefeated = circuitRestored = false;
        ResetScreen("FASE 0 · TUTORIAL", "INTRODUCCIÓN");
        var narrative = new Label { Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter, Padding = new Padding(110, 30, 110, 30),
            Font = new Font("Segoe UI", 16), ForeColor = Color.Gainsboro,
            Text = "Año 2187. La nave de exploración IRIS lleva un mes sin responder. " +
            "La red NEXUS ha enviado a un cadete para investigar la pérdida de contacto. " +
            "Accederás por mantenimiento. Soy JARVIS, tu asistente de misión. " +
            "Avanza con cautela: desconocemos el estado de los sistemas a bordo." };
        Controls.Add(narrative);
        var panel = Buttons();
        AddButton(panel, "CONTINUAR", Explore);
        panel.BringToFront();
    }

    private void Explore()
    {
        ResetScreen($"FASE 0 · SALA {room:00}", roomNames[room - 1]);
        var panel = Buttons();
        panel.Height = 135;
        AddButton(panel, "INTERACTUAR (E)", Interact);
        AddButton(panel, "PAUSA", Pause);
        status = new Label { Dock = DockStyle.Bottom, Height = 50, ForeColor = cyan,
            Font = new Font("Segoe UI", 12), TextAlign = ContentAlignment.MiddleCenter };
        Controls.Add(status);
        board = new Panel { Size = new Size(9 * Tile, 7 * Tile), BackColor = Color.FromArgb(20, 32, 49) };
        board.Location = new Point((ClientSize.Width - board.Width) / 2, 177);
        board.Paint += PaintBoard;
        Controls.Add(board);
        UpdateStatus();
        board.Invalidate();
    }

    private void PaintBoard(object? sender, PaintEventArgs e)
    {
        if (board is null) return;
        using var line = new Pen(Color.FromArgb(47, 70, 91));
        for (int x = 0; x < 9; x++)
        for (int y = 0; y < 7; y++)
        {
            var rect = new Rectangle(x * Tile, y * Tile, Tile, Tile);
            e.Graphics.DrawRectangle(line, rect);
        }
        void Mark(int x, int y, Color color, string letter)
        {
            using var brush = new SolidBrush(color);
            e.Graphics.FillEllipse(brush, x * Tile + 8, y * Tile + 8, Tile - 16, Tile - 16);
            using var font = new Font("Segoe UI", 10, FontStyle.Bold);
            e.Graphics.DrawString(letter, font, Brushes.White, x * Tile + 16, y * Tile + 13);
        }
        if (room == 1 && !hasKit) Mark(4, 3, Color.SeaGreen, "+");
        if (room == 2 && !robotDefeated) Mark(5, 3, Color.DarkRed, "R");
        if (room == 3 && !circuitRestored) Mark(5, 3, Color.DarkOrange, "C");
        Mark(7, 3, Color.DarkSlateBlue, ">");
        Mark(playerX, playerY, cyan, "C");
        Mark(Math.Max(0, playerX - 1), playerY, Color.DarkCyan, "J");
    }

    private void UpdateStatus()
    {
        if (status is null) return;
        string goal = room switch
        {
            1 => hasKit ? "Dirígete a la salida (>)" : "Recoge el kit médico (+) con E",
            2 => robotDefeated ? "Dirígete a la salida (>)" : "Acércate al robot (R) y presiona E",
            _ => circuitRestored ? "Dirígete a la salida (>)" : "Activa el circuito (C) con E"
        };
        status.Text = $"OBJETIVO: {goal}     |     KIT: {(hasKit ? 1 : 0)}     |     WASD / FLECHAS";
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (board is null || !Controls.Contains(board)) return;
        int dx = e.KeyCode is Keys.A or Keys.Left ? -1 : e.KeyCode is Keys.D or Keys.Right ? 1 : 0;
        int dy = e.KeyCode is Keys.W or Keys.Up ? -1 : e.KeyCode is Keys.S or Keys.Down ? 1 : 0;
        if (e.KeyCode == Keys.E) { Interact(); return; }
        if (dx == 0 && dy == 0) return;
        playerX = Math.Clamp(playerX + dx, 0, 8);
        playerY = Math.Clamp(playerY + dy, 0, 6);
        board.Invalidate();
    }

    private void Interact()
    {
        bool Near(int x, int y) => Math.Abs(playerX - x) + Math.Abs(playerY - y) <= 1;
        if (room == 1 && !hasKit && Near(4, 3)) { hasKit = true; Info("Kit médico obtenido."); }
        else if (room == 2 && !robotDefeated && Near(5, 3))
            Info("Encuentro detectado. El combate por PA se integrará en la próxima entrega.");
        else if (room == 3 && !circuitRestored && Near(5, 3))
            Info("Panel de circuitos detectado. El rompecabezas se integrará en la próxima entrega.");
        else if (Near(7, 3))
        {
            if (room == 1 && hasKit) { room = 2; playerX = 1; Explore(); }
            else if (room == 2 && robotDefeated) { room = 3; playerX = 1; Explore(); }
            else if (room == 3 && circuitRestored) Info("FASE 0 COMPLETADA");
            else Info("Acceso bloqueado. Completa el objetivo de la sala.");
        }
        else Info("No hay ningún objeto interactivo al alcance.");
        UpdateStatus();
        board?.Invalidate();
    }

    private void Pause()
    {
        var result = MessageBox.Show("¿Guardar y volver al menú?\nNo se guardará el progreso en esta versión.",
            "PAUSA · Sí = Menú / No = Reanudar", MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes) { board = null; Menu(); }
    }

    private static void Info(string text) =>
        MessageBox.Show(text, "NEXUS — PROTOCOLO IRIS", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
