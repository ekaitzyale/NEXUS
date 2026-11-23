namespace WindowsForms;

public sealed partial class IrisGameForm
{
    private sealed record InventoryEntry(string Name, string Description, int? Quantity = null, TerminalDocument? Document = null)
    {
        public override string ToString() => Quantity is int count ? $"{Name}  ×{count}" : Name;
    }

    private static readonly (int Room, string Objective, InventoryEntry Entry)[] MissionItems =
    [
        (7, "chip", new("CHIP DE INFORMACIÓN",
            "Contiene un código de acceso que permitirá desbloquear una puerta de seguridad en la Fase 2 (Protocolo IRIS).", 1))
    ];

    private Button InventoryButton()
    {
        inventoryAvailable = true;
        return Button("INVENTARIO", OpenInventory, 148);
    }

    private IEnumerable<InventoryEntry> InventoryEntries()
    {
        int kits = combat?.Kits ?? Session.Kits;
        yield return new InventoryEntry("BOTIQUINES",
            $"Recupera {CombatTurn.KitHealing} HP. En combate, USAR KIT cuesta 1 PA. Capacidad máxima: {GameSession.MaxKits}.", kits);

        foreach (var (room, objective, entry) in MissionItems)
            if (Session.HasObjective(room, objective))
                yield return entry;

        foreach (var (room, progress) in Session.Rooms.OrderBy(entry => entry.Key))
        {
            foreach (var document in catalog.Documents(room))
                if (progress.ReadDocuments.Contains(document.Id))
                    yield return new InventoryEntry(document.Title,
                        $"{catalog.Room(room).Name}   /   {document.Origin}   /   {document.Date}", Document: document);
        }
    }

    private void OpenInventory()
    {
        using var dialog = new InventoryDialog
        {
            Text = "INVENTARIO", ClientSize = new Size(780, 490), MinimumSize = new Size(670, 400),
            StartPosition = FormStartPosition.CenterParent, ShowInTaskbar = false, MinimizeBox = false,
            BackColor = Background, ForeColor = Color.White, Padding = new Padding(16), AutoScaleMode = AutoScaleMode.Dpi
        };
        var title = Caption("INVENTARIO", 23, Cyan);
        title.Dock = DockStyle.Top;
        title.Height = 60;
        var body = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, Dock = DockStyle.Fill };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        var entries = new ListBox
        {
            Dock = DockStyle.Fill, BackColor = Surface, ForeColor = Color.White, Font = new Font("Segoe UI", 11),
            AccessibleName = "OBJETOS Y DOCUMENTOS", HorizontalScrollbar = true
        };
        foreach (var entry in InventoryEntries()) entries.Items.Add(entry);

        var details = new ArtPanel { Dock = DockStyle.Fill, BackColor = Surface, Margin = new Padding(8, 3, 3, 3) };
        details.Paint += PaintHolographicFrame;
        var name = Caption("", 15, Cyan);
        name.Dock = DockStyle.Top;
        name.Height = 78;
        var description = new TextBox
        {
            Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, BorderStyle = BorderStyle.None,
            BackColor = Surface, ForeColor = Color.White, Font = new Font("Segoe UI", 12), ScrollBars = ScrollBars.Vertical
        };
        var detailBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 18, 20, 12) };
        detailBody.Controls.Add(description);
        detailBody.Controls.Add(name);
        var detailBar = Bar(detailBody);
        var read = Button("LEER DOCUMENTO", () =>
        {
            if (entries.SelectedItem is InventoryEntry { Document: { } document }) OpenDocument(document, dialog);
        }, 175);
        detailBar.Controls.Add(read);
        detailBar.SendToBack();
        details.Padding = new Padding(10);
        details.Controls.Add(detailBody);
        body.Controls.Add(entries, 0, 0);
        body.Controls.Add(details, 1, 0);

        entries.SelectedIndexChanged += (_, _) =>
        {
            if (entries.SelectedItem is not InventoryEntry selected) return;
            name.Text = selected.Name;
            description.Text = selected.Quantity is int quantity
                ? $"CANTIDAD: {quantity}{Environment.NewLine}{Environment.NewLine}{selected.Description}"
                : selected.Description;
            read.Visible = selected.Document is not null;
        };
        entries.DoubleClick += (_, _) => read.PerformClick();
        entries.SelectedIndex = 0;

        var bar = Bar(dialog);
        var close = Button("CERRAR (I)", () => dialog.DialogResult = DialogResult.Cancel, 155);
        bar.Controls.Add(close);
        dialog.CancelButton = close;
        dialog.Controls.Add(body);
        dialog.Controls.Add(title);
        bar.SendToBack();
        dialog.ActiveControl = entries;
        dialog.ShowDialog(this);
        board?.Focus();
    }

    private void OpenDocument(TerminalDocument document, IWin32Window? owner = null)
    {
        using var dialog = Hologram(document.Title, document.Text, "CERRAR", $"{document.Origin}   /   {document.Date}");
        dialog.ShowDialog(owner ?? this);
    }

    private sealed class InventoryDialog : Form
    {
        protected override bool ProcessCmdKey(ref Message message, Keys key)
        {
            if (key != Keys.I) return base.ProcessCmdKey(ref message, key);
            if ((message.LParam.ToInt64() & (1L << 30)) == 0) DialogResult = DialogResult.Cancel;
            return true;
        }
    }
}
