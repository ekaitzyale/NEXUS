using System.Drawing;
using System.Windows.Forms;

namespace WindowsForms
{
    // Primer paso de la nueva interfaz. La lógica del juego se conectará por capas.
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ConfigurarMenu();
        }

        private void ConfigurarMenu()
        {
            Text = "NEXUS — Protocolo IRIS";
            ClientSize = new Size(960, 600);
            MinimumSize = new Size(800, 500);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(12, 19, 34);
            ForeColor = Color.White;
            Controls.Clear();

            var titulo = new Label
            {
                Text = "N E X U S",
                Font = new Font("Segoe UI", 38, FontStyle.Bold),
                ForeColor = Color.FromArgb(106, 220, 245),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 135
            };
            Controls.Add(titulo);

            var subtitulo = new Label
            {
                Text = "PROTOCOLO IRIS",
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                ForeColor = Color.FromArgb(185, 200, 220),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 42
            };
            Controls.Add(subtitulo);
            subtitulo.BringToFront();

            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Size = new Size(340, 300),
                Anchor = AnchorStyles.None,
                BackColor = Color.Transparent,
                Padding = new Padding(10)
            };
            Controls.Add(panel);

            AgregarBoton(panel, "NUEVA PARTIDA", () => MostrarPendiente("Nueva partida"));
            AgregarBoton(panel, "CONTINUAR", () => MostrarPendiente("Cargar partida"));
            AgregarBoton(panel, "CONFIGURACIÓN", () => MostrarPendiente("Configuración"));
            AgregarBoton(panel, "SALIR", Close);

            void CentrarPanel()
            {
                panel.Location = new Point((ClientSize.Width - panel.Width) / 2,
                    Math.Max(190, (ClientSize.Height - panel.Height) / 2 + 55));
            }
            Resize += (_, _) => CentrarPanel();
            CentrarPanel();
        }

        private static void AgregarBoton(FlowLayoutPanel panel, string texto, Action accion)
        {
            var boton = new Button
            {
                Text = texto,
                Width = 300,
                Height = 54,
                Margin = new Padding(8, 5, 8, 5),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(28, 52, 78),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            boton.FlatAppearance.BorderColor = Color.FromArgb(75, 158, 190);
            boton.Click += (_, _) => accion();
            panel.Controls.Add(boton);
        }

        private void MostrarPendiente(string funcionalidad)
        {
            MessageBox.Show(
                $"{funcionalidad}: se implementará en la siguiente etapa.",
                "NEXUS — Desarrollo v2",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
