using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsForms;

/// <summary>Rotating circuit puzzle. Connect the power source at left to the receiver at right.</summary>
public sealed class CircuitPuzzle : Form
{
    private const int North = 1, East = 2, South = 4, West = 8;
    private readonly int[,] pieces = new int[5, 5];
    private readonly Button[,] tiles = new Button[5, 5];
    private readonly Label feedback = new();
    public bool Solved { get; private set; }

    public CircuitPuzzle()
    {
        Text = "NEXUS · PANEL DE ENERGÍA";
        ClientSize = new Size(610, 720);
        MinimumSize = new Size(610, 720);
        MaximumSize = new Size(610, 720);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(7, 15, 29);
        ForeColor = Color.White;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        var heading = new Label { Text = "RESTABLECER CIRCUITO", Font = new Font("Segoe UI", 21, FontStyle.Bold),
            ForeColor = Color.FromArgb(83, 220, 246), TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top, Height = 70 };
        Controls.Add(heading);
        var help = new Label { Text = "Haz clic para girar las conexiones. Une ENTRADA con SALIDA.",
            ForeColor = Color.LightSteelBlue, TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top, Height = 45 };
        Controls.Add(help);
        var grid = new TableLayoutPanel { ColumnCount = 5, RowCount = 5, Size = new Size(475, 425),
            Location = new Point(67, 160), CellBorderStyle = TableLayoutPanelCellBorderStyle.Single };
        for (int i = 0; i < 5; i++)
        {
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        }
        Controls.Add(grid);
        var source = new Label { Text = "ENTRADA  →", ForeColor = Color.LimeGreen, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        source.SetBounds(67, 127, 200, 28); Controls.Add(source);
        var target = new Label { Text = "→  SALIDA", ForeColor = Color.Gold, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        target.SetBounds(342, 127, 200, 28); Controls.Add(target);
        var entryArrow = new Label { Text = "➜", ForeColor = Color.LimeGreen, TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 22, FontStyle.Bold) };
        entryArrow.SetBounds(20, 365, 46, 40); Controls.Add(entryArrow);
        var exitArrow = new Label { Text = "➜", ForeColor = Color.Orange, TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 22, FontStyle.Bold) };
        exitArrow.SetBounds(542, 365, 46, 40); Controls.Add(exitArrow);
        var tip = new Label { Text = "Conecta la casilla central izquierda con la central derecha.\nLas piezas conectadas a la energía se iluminan.",
            ForeColor = Color.LightSteelBlue, TextAlign = ContentAlignment.MiddleCenter };
        tip.SetBounds(40, 600, 530, 60); Controls.Add(tip);
        // Guaranteed solvable route; the rest are decoy connections.
        var solution = new int[5, 5];
        solution[2, 0] = West | East;
        solution[2, 1] = West | North;
        solution[1, 1] = South | East;
        solution[1, 2] = West | East;
        solution[1, 3] = West | South;
        solution[2, 3] = North | East;
        solution[2, 4] = West | East;
        for (int row = 0; row < 5; row++)
        for (int col = 0; col < 5; col++)
        {
            int value = solution[row, col] != 0 ? solution[row, col] : (row + col) % 3 == 0 ? North | East | South : North | South;
            for (int i = 0, turns = Random.Shared.Next(1, 4); i < turns; i++) value = Rotate(value);
            pieces[row, col] = value;
            int r = row, c = col;
            var button = new Button { Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat,
                Margin = new Padding(2), BackColor = Color.FromArgb(18, 39, 60),
                ForeColor = Color.FromArgb(83, 220, 246), Font = new Font("Consolas", 28, FontStyle.Bold) };
            button.FlatAppearance.BorderColor = Color.FromArgb(52, 103, 135);
            button.Click += (_, _) => { pieces[r, c] = Rotate(pieces[r, c]); DrawTiles(); };
            tiles[row, col] = button;
            grid.Controls.Add(button, col, row);
        }
        feedback.Text = "ENERGÍA: DESCONECTADA";
        feedback.ForeColor = Color.Orange;
        feedback.TextAlign = ContentAlignment.MiddleCenter;
        feedback.SetBounds(55, 666, 500, 35);
        Controls.Add(feedback);
        DrawTiles();
    }

    private static int Rotate(int value) => ((value << 1) & 15) | ((value >> 3) & 1);
    private static string Symbol(int mask) => mask switch
    {
        5 => "│", 10 => "─", 3 => "└", 6 => "┌", 12 => "┐", 9 => "┘",
        7 => "├", 11 => "┴", 13 => "┤", 14 => "┬", 15 => "┼", _ => "·"
    };
    private void DrawTiles()
    {
        var powered = PoweredTiles();
        for (int row = 0; row < 5; row++)
        for (int col = 0; col < 5; col++)
        {
            tiles[row, col].Text = Symbol(pieces[row, col]);
            bool lit = powered[row, col];
            tiles[row, col].BackColor = lit ? Color.FromArgb(12, 105, 111) : Color.FromArgb(18, 39, 60);
            tiles[row, col].ForeColor = lit ? Color.White : Color.FromArgb(83, 220, 246);
        }
        if (!Connected()) return;
        Solved = true;
        feedback.Text = "CIRCUITO RESTABLECIDO";
        feedback.ForeColor = Color.LimeGreen;
        DialogResult = DialogResult.OK;
        Close();
    }
    private bool[,] PoweredTiles()
    {
        var seen = new bool[5, 5];
        if ((pieces[2, 0] & West) == 0) return seen;
        var queue = new Queue<(int r, int c)>();
        queue.Enqueue((2, 0)); seen[2, 0] = true;
        (int dr, int dc, int bit, int opposite)[] dirs =
        { (-1, 0, North, South), (0, 1, East, West), (1, 0, South, North), (0, -1, West, East) };
        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();
            foreach (var (dr, dc, bit, opposite) in dirs)
            {
                int nr = r + dr, nc = c + dc;
                if (nr < 0 || nr >= 5 || nc < 0 || nc >= 5 || seen[nr, nc]) continue;
                if ((pieces[r, c] & bit) == 0 || (pieces[nr, nc] & opposite) == 0) continue;
                seen[nr, nc] = true; queue.Enqueue((nr, nc));
            }
        }
        return seen;
    }

    private bool Connected()
    {
        if ((pieces[2, 0] & West) == 0 || (pieces[2, 4] & East) == 0) return false;
        var seen = new bool[5, 5];
        var queue = new Queue<(int r, int c)>();
        queue.Enqueue((2, 0));
        seen[2, 0] = true;
        (int dr, int dc, int bit, int opposite)[] directions =
        {
            (-1, 0, North, South), (0, 1, East, West), (1, 0, South, North), (0, -1, West, East)
        };
        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();
            if (r == 2 && c == 4) return true;
            foreach (var (dr, dc, bit, opposite) in directions)
            {
                int nr = r + dr, nc = c + dc;
                if (nr < 0 || nr >= 5 || nc < 0 || nc >= 5 || seen[nr, nc]) continue;
                if ((pieces[r, c] & bit) == 0 || (pieces[nr, nc] & opposite) == 0) continue;
                seen[nr, nc] = true;
                queue.Enqueue((nr, nc));
            }
        }
        return false;
    }
}
