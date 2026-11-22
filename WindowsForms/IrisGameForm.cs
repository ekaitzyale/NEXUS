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
    private bool robotDefeated, circuitRestored;
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
        room = 1; x = 1; y = 3; hp = 100; kits = 0; robotDefeated = circuitRestored = false; jarvisCharges = 6;
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
        using var floor = new SolidBrush(Surface);
        using var gridPen = new Pen(Color.FromArgb(46, 75, 102));
        for (int ix = 0; ix < 9; ix++)
        for (int iy = 0; iy < 7; iy++)
        {
            var r = new Rectangle(left + ix * tile, top + iy * tile, tile, tile);
            e.Graphics.FillRectangle(floor, r);
            e.Graphics.DrawRectangle(gridPen, r);
        }
        void Marker(int px, int py, Color color, string label)
        {
            var r = new Rectangle(left + px * tile + tile / 5, top + py * tile + tile / 5, tile * 3 / 5, tile * 3 / 5);
            using var brush = new SolidBrush(color);
            e.Graphics.FillEllipse(brush, r);
            using var font = new Font("Segoe UI", Math.Max(10, tile / 5), FontStyle.Bold);
            TextRenderer.DrawText(e.Graphics, label, font, r, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        if (room == 1 && kits == 0) Marker(4, 3, Color.SeaGreen, "+");
        if (room == 2 && !robotDefeated) Marker(5, 3, Color.Firebrick, "R");
        if (room == 3 && !circuitRestored) Marker(5, 3, Color.DarkOrange, "C");
        Marker(7, 3, Color.MediumPurple, ">");
        Marker(Math.Max(0, x - 1), y, Color.Teal, "J");
        Marker(x, y, Cyan, "C");
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
            Text = $"PA DISPONIBLES: {remainingPa}     |     ATAQUES: {actions.Count(a => a == "ATACAR")}     DEFENSAS: {actions.Count(a => a == "DEFENDER")}     RESERVAS: {actions.Count(a => a == "RESERVAR")}     KITS: {kits}",
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
        if (remainingPa > 0)
        {
            if (kits > 0 && hp < 100) area.Controls.Add(ActionButton("USAR KIT · 1 PA (+30 HP)", (_, _) => Choose("KIT")));
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
            area.Controls.Add(ActionButton("CONFIRMAR TURNO", (_, _) => ResolveCombat()));
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
        string report =
            $"TURNO {turnNumber} · RESULTADOS\n\n" +
            $"CADETE: ATACAR ×{playerAttacks} · DEFENDER ×{playerDefenses} · RESERVAR ×{playerReserved} · KIT ×{usedKits}\n" +
            $"ROBOT: ATACAR ×{robotAttacks} · DEFENDER ×{robotDefenses} · RESERVAR ×{robotReservedPa}\n\n" +
            $"Ataques bloqueados: Cadete {Math.Min(robotAttacks, playerDefenses)} · Robot {Math.Min(playerAttacks, robotDefenses)}\n" +
            $"Daño causado: {dealt} · Daño recibido: {received} · Curación: +{healing}\n" +
            $"Vida Cadete: {hp}/100 · Vida Robot: {robotHp}/40\n" +
            $"PA siguiente turno: Cadete {nextPa} · Robot {nextRobotPa}";
        Page("RESULTADO DEL TURNO", "RESOLUCIÓN TÁCTICA");
        var content = new Label { Text = report, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 14),
            ForeColor = Muted, Padding = new Padding(50, 20, 50, 20), TextAlign = ContentAlignment.TopCenter };
        root.Controls.Add(content);
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
