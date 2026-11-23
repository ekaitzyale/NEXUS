using System.Drawing.Drawing2D;

namespace WindowsForms;

public sealed partial class IrisGameForm
{
    private static ArtPanel Portrait()
    {
        var art = new ArtPanel { BackColor = Background };
        art.Paint += (_, e) =>
        {
            var bounds = art.ClientRectangle;
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int scale = Math.Min(bounds.Width, bounds.Height) * 3 / 4;
            DrawCadet(g, bounds.Width * 0.41f, bounds.Height * 0.59f, scale);
            DrawOrb(g, bounds.Width * 0.68f, bounds.Height * 0.35f, scale * 0.5f);
        };
        return art;
    }

    private static Control FighterCard(string name, int hp, int maximum, int damage, bool robot)
    {
        var panel = new ArtPanel { Dock = DockStyle.Fill, BackColor = Surface, Margin = new Padding(5) };
        var color = robot ? Red : Cyan;
        var stats = Caption($"{name}   ·   {hp}/{maximum} HP\nDAÑO {damage}", 11, color);
        stats.Dock = DockStyle.Bottom;
        stats.Height = 51;
        panel.Controls.Add(stats);
        panel.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float x = panel.Width / 2f;
            float y = (panel.Height - stats.Height) / 2f;
            if (robot) DrawRobot(g, x, y, 85);
            else DrawCadet(g, x, y, 85);
            using var border = new Pen(color);
            g.DrawRectangle(border, 0, 0, panel.Width - 1, panel.Height - 1);
        };
        return panel;
    }

    private void PaintRoom(object? sender, PaintEventArgs e)
    {
        if (sender is not Panel map) return;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        int tile = Math.Min((map.Width - 16) / 9, (map.Height - 16) / 7);
        if (tile < 8) return;
        int left = (map.Width - tile * 9) / 2;
        int top = (map.Height - tile * 7) / 2;

        using var wall = new SolidBrush(Color.FromArgb(42, 65, 85));
        using var wallInset = new SolidBrush(Color.FromArgb(18, 36, 54));
        using var floor = new SolidBrush(Color.FromArgb(21, 41, 59));
        using var alternateFloor = new SolidBrush(Color.FromArgb(24, 45, 66));
        using var grid = new Pen(Color.FromArgb(46, 75, 101));
        using var rail = new Pen(Color.FromArgb(34, 181, 214), Math.Max(1, tile / 18));
        Color fixtureAccent = Session.Room switch
        {
            4 or 6 => Color.Goldenrod,
            5 => Color.PaleTurquoise,
            _ => Cyan
        };
        using var fixtureLight = new Pen(fixtureAccent, Math.Max(1, tile / 22));

        for (int row = 0; row < 7; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                var rect = new Rectangle(left + column * tile, top + row * tile, tile, tile);
                if (GameSession.IsWall(new Point(column, row)))
                {
                    g.FillRectangle(wall, rect);
                    rect.Inflate(-tile / 7, -tile / 7);
                    g.FillRectangle(wallInset, rect);
                    if ((row is 0 or 6) && column % 2 != 0)
                        g.DrawLine(rail, rect.Left, rect.Top, rect.Right, rect.Top);
                }
                else
                {
                    g.FillRectangle((row + column) % 2 == 0 ? floor : alternateFloor, rect);
                    g.DrawRectangle(grid, rect);
                    if (Session.IsFixture(new Point(column, row)))
                    {
                        rect.Inflate(-tile / 8, -tile / 8);
                        g.FillRectangle(wallInset, rect);
                        g.DrawRectangle(fixtureLight, rect);
                        if (Session.Room == 5)
                            g.DrawLine(fixtureLight, rect.Left, rect.Top + rect.Height / 3, rect.Right, rect.Top + rect.Height / 3);
                        else
                            g.DrawLine(fixtureLight, rect.Left, rect.Bottom, rect.Right, rect.Top);
                    }
                }
            }
        }

        PointF Center(Point point) => new(left + point.X * tile + tile / 2f, top + point.Y * tile + tile / 2f);
        void Symbol(Point point, string symbol, Color color)
        {
            var p = Center(point);
            int side = tile * 2 / 3;
            var rect = new Rectangle((int)p.X - side / 2, (int)p.Y - side / 2, side, side);
            using var border = new Pen(color, 2);
            using var font = new Font("Segoe UI", Math.Max(8, tile / 4), FontStyle.Bold);
            g.FillRectangle(wallInset, rect);
            g.DrawRectangle(border, rect);
            TextRenderer.DrawText(g, symbol, font, rect, color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        if (Session.Room != 18) Symbol(GameSession.Exit, "»", Session.CanExit ? Cyan : Color.Orange);
        if (Session.HasTerminal)
            Symbol(GameSession.Terminal, "≡", Cyan);
        if (Session.HasKit && !Session.KitCollected)
            Symbol(GameSession.Kit, "+", Color.SpringGreen);
        foreach (var enemy in Session.ActiveEnemies())
        {
            var robot = Center(enemy.Position);
            DrawRobot(g, robot.X, robot.Y, tile);
        }
        if (Session.HasCircuit)
            Symbol(Session.CircuitPosition, "⌁", Session.CircuitRestored ? Color.SpringGreen : Cyan);
        if (Session.Room == 7 && !Session.ChipCollected)
            Symbol(GameSession.Chip, "▣", Color.SpringGreen);
        if (Session.Room == 13)
            Symbol(GameSession.Device, "⌁", Session.InvestigationComplete ? Color.SpringGreen : Cyan);
        if (Session.HasGenerator)
            Symbol(GameSession.Generator, "ϟ", Session.GeneratorDisabled ? Color.SpringGreen : Color.Orange);
        if (Session.Room == 18)
            Symbol(GameSession.Device, "◇", Color.Orange);

        var player = Center(Session.Position);
        DrawOrb(g, player.X - tile * 0.36f, player.Y - tile * 0.29f, tile * 0.46f);
        DrawCadet(g, player.X, player.Y, tile);
    }

    private static void DrawCadet(Graphics g, float x, float y, float size)
    {
        using var shadow = new SolidBrush(Color.FromArgb(160, 5, 13, 23));
        using var armor = new SolidBrush(Color.FromArgb(228, 235, 242));
        using var plates = new SolidBrush(Color.FromArgb(115, 142, 162));
        using var visor = new SolidBrush(Cyan);
        using var rim = new Pen(Color.FromArgb(30, 52, 72), Math.Max(1, size / 35));
        g.FillEllipse(shadow, x - size * 0.33f, y + size * 0.11f, size * 0.66f, size * 0.22f);
        var body = new RectangleF(x - size * 0.29f, y - size * 0.03f, size * 0.58f, size * 0.31f);
        g.FillEllipse(plates, body);
        g.DrawEllipse(rim, body);
        g.FillRectangle(armor, x - size * 0.12f, y + size * 0.05f, size * 0.24f, size * 0.23f);
        var helmet = new RectangleF(x - size * 0.18f, y - size * 0.31f, size * 0.36f, size * 0.38f);
        g.FillEllipse(armor, helmet);
        g.DrawEllipse(rim, helmet);
        g.FillEllipse(shadow, x - size * 0.16f, y - size * 0.20f, size * 0.32f, size * 0.15f);
        g.FillEllipse(visor, x - size * 0.12f, y - size * 0.18f, size * 0.24f, size * 0.08f);
    }

    private static void DrawOrb(Graphics g, float x, float y, float size)
    {
        using var halo = new SolidBrush(Color.FromArgb(54, Cyan));
        using var outer = new SolidBrush(Color.FromArgb(29, 67, 94));
        using var core = new SolidBrush(Cyan);
        using var highlight = new SolidBrush(Color.Azure);
        g.FillEllipse(halo, x - size / 2, y - size / 2, size, size);
        g.FillEllipse(outer, x - size * 0.32f, y - size * 0.32f, size * 0.64f, size * 0.64f);
        g.FillEllipse(core, x - size * 0.21f, y - size * 0.21f, size * 0.42f, size * 0.42f);
        g.FillEllipse(highlight, x - size * 0.13f, y - size * 0.12f, size * 0.15f, size * 0.15f);
    }

    private static void DrawRobot(Graphics g, float x, float y, float size)
    {
        using var legs = new Pen(Color.FromArgb(167, 44, 60), Math.Max(2, size / 13));
        using var shell = new SolidBrush(Color.FromArgb(127, 26, 42));
        using var core = new SolidBrush(Red);
        foreach (int side in new[] { -1, 1 })
        {
            for (int leg = -1; leg <= 1; leg++)
            {
                PointF[] points =
                [
                    new(x + side * size * 0.12f, y + leg * size * 0.11f),
                    new(x + side * size * 0.29f, y + leg * size * 0.18f - size * 0.03f),
                    new(x + side * size * 0.38f, y + leg * size * 0.23f + size * 0.04f)
                ];
                g.DrawLines(legs, points);
            }
        }
        g.FillEllipse(shell, x - size * 0.23f, y - size * 0.26f, size * 0.46f, size * 0.52f);
        g.FillEllipse(core, x - size * 0.105f, y - size * 0.105f, size * 0.21f, size * 0.21f);
    }

    private static void PaintShip(object? sender, PaintEventArgs e)
    {
        if (sender is not Control panel || panel.Width < 1 || panel.Height < 1) return;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = panel.ClientRectangle;
        using var space = new LinearGradientBrush(rect, Background, Color.FromArgb(10, 48, 71), 60);
        g.FillRectangle(space, rect);
        using var stars = new SolidBrush(Color.FromArgb(125, 189, 222));
        for (int i = 1; i < 74; i++)
            g.FillEllipse(stars, rect.Width * (i * 43 % 101) / 101f, rect.Height * (i * 67 % 103) / 103f, 2, 2);

        float w = rect.Width;
        float h = rect.Height;
        using var hull = new SolidBrush(Color.FromArgb(29, 59, 81));
        using var edge = new Pen(Cyan, 2);
        PointF[] ship =
        [
            new(w * .13f, h * .58f), new(w * .35f, h * .44f), new(w * .79f, h * .37f),
            new(w * .88f, h * .48f), new(w * .70f, h * .58f), new(w * .33f, h * .64f)
        ];
        g.FillPolygon(hull, ship);
        g.DrawPolygon(edge, ship);
        g.DrawLine(edge, w * .35f, h * .51f, w * .75f, h * .46f);
        DrawOrb(g, w * .79f, h * .52f, Math.Min(w, h) * .15f);
    }

    private static void PaintHolographicFrame(object? sender, PaintEventArgs e)
    {
        if (sender is not Control panel || panel.Width < 50 || panel.Height < 50) return;
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        int right = panel.Width - 7;
        int bottom = panel.Height - 7;
        Point[] outline = [new(6, 24), new(24, 6), new(right - 18, 6), new(right, 24),
            new(right, bottom - 18), new(right - 18, bottom), new(24, bottom), new(6, bottom - 18)];
        using var halo = new Pen(Color.FromArgb(58, Cyan), 8);
        using var edge = new Pen(Cyan, 2);
        graphics.DrawPolygon(halo, outline);
        graphics.DrawPolygon(edge, outline);
    }
}
