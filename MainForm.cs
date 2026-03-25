using AnalizadorDocumentos.Analizador;
using AnalizadorDocumentos.Extractor;
using AnalizadorDocumentos.Lexer;
using System.Windows.Forms;
using TextBox = System.Windows.Forms.TextBox;
using Label = System.Windows.Forms.Label;
using Button = System.Windows.Forms.Button;

namespace AnalizadorDocumentos;

public class MainForm : Form
{
    // Colores de la paleta "Modern Dark"
    private readonly Color _fondoOscuro = Color.FromArgb(28, 28, 28);
    private readonly Color _fondoPanel = Color.FromArgb(40, 40, 40);
    private readonly Color _azulAcento = Color.FromArgb(0, 122, 204);
    private readonly Color _textoPrincipal = Color.FromArgb(224, 224, 224);
    private readonly Color _verdeExito = Color.FromArgb(15, 110, 86);


    private TextBox txtRuta;
    private Button btnExaminar;
    private Button btnAnalizar;
    private Button btnLimpiar;
    private ProgressBar progressBar;
    private Label lblEstado;
    private FlowLayoutPanel flpTokens;
    private DataGridView dgvEstadisticas;
    private RichTextBox rtbResumen;

    public MainForm()
    {
        InicializarComponentes();
    }

    private void InicializarComponentes()
    {
        flpTokens = new FlowLayoutPanel
        {
            Location = new Point(390, 175),
            Size = new Size(425, 200),
            BackColor = _fondoPanel,
            Padding = new Padding(10, 10, 25, 10), // Aumentamos padding derecho (25)
            AutoScroll = true,
            WrapContents = true
        }; 

        this.Text = "Analizador Léxico de Documentos";
        this.Size = new Size(850, 700);
        this.BackColor = _fondoOscuro;
        this.ForeColor = _textoPrincipal;
        this.Font = new Font("Segoe UI", 10);
        this.StartPosition = FormStartPosition.CenterScreen;

        // 1. INSTANCIAR PRIMERO (Evita errores de Null)
        txtRuta = new TextBox();
        btnExaminar = new Button();
        progressBar = new ProgressBar();
        lblEstado = new Label();
        dgvEstadisticas = new DataGridView();
        flpTokens = new FlowLayoutPanel();
        rtbResumen = new RichTextBox();
        btnLimpiar = new Button();
        btnAnalizar = new Button();

        // 2. CONFIGURAR RUTA
        var lblRuta = new Label { Text = "Documento seleccionado", Location = new Point(20, 20), AutoSize = true, ForeColor = Color.Gray };
        txtRuta.Location = new Point(20, 45);
        txtRuta.Size = new Size(650, 30);
        txtRuta.BackColor = _fondoPanel;
        txtRuta.ForeColor = Color.White;
        txtRuta.BorderStyle = BorderStyle.FixedSingle;
        txtRuta.ReadOnly = true;

        btnExaminar = CrearBotonModerno("Examinar...", new Point(685, 44), new Size(130, 32), Color.FromArgb(60, 60, 60));
        btnExaminar.Click += BtnExaminar_Click;

        // 3. CONFIGURAR PROGRESO
        progressBar.Location = new Point(20, 95);
        progressBar.Size = new Size(795, 6);
        progressBar.Style = ProgressBarStyle.Marquee;
        progressBar.MarqueeAnimationSpeed = 0;
        progressBar.BackColor = _fondoOscuro;

        lblEstado.Text = "Esperando documento...";
        lblEstado.Location = new Point(20, 110);
        lblEstado.AutoSize = true;
        lblEstado.Font = new Font("Segoe UI", 9, FontStyle.Italic);

        // 4. CONFIGURAR TABLA (ESTADÍSTICAS)
        var lblStats = new Label { Text = "ESTADÍSTICAS LÉXICAS", Location = new Point(20, 150), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        dgvEstadisticas.Location = new Point(20, 175);
        dgvEstadisticas.Size = new Size(350, 200);
        dgvEstadisticas.BackgroundColor = _fondoPanel;
        dgvEstadisticas.GridColor = Color.FromArgb(60, 60, 60);
        dgvEstadisticas.BorderStyle = BorderStyle.None;
        dgvEstadisticas.RowHeadersVisible = false;
        dgvEstadisticas.AllowUserToAddRows = false;
        dgvEstadisticas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvEstadisticas.ReadOnly = true;
        dgvEstadisticas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvEstadisticas.EnableHeadersVisualStyles = false;

        dgvEstadisticas.Columns.Add("Metrica", "Métrica");
        dgvEstadisticas.Columns.Add("Valor", "Valor");

        // Estilos de la tabla para mantener el BLANCO
        dgvEstadisticas.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
        dgvEstadisticas.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvEstadisticas.DefaultCellStyle.BackColor = _fondoPanel;
        dgvEstadisticas.DefaultCellStyle.ForeColor = Color.White;
        dgvEstadisticas.DefaultCellStyle.SelectionBackColor = Color.FromArgb(55, 55, 55);
        dgvEstadisticas.DefaultCellStyle.SelectionForeColor = Color.White;

        // 5. CONFIGURAR TOKENS (CHIPS)
        var lblTokens = new Label { Text = "TOKENS PRINCIPALES", Location = new Point(390, 150), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        flpTokens.Location = new Point(390, 175);
        flpTokens.Size = new Size(425, 200);
        flpTokens.BackColor = _fondoPanel;
        flpTokens.Padding = new Padding(10);
        flpTokens.AutoScroll = true;
        flpTokens.WrapContents = true;

        // 6. CONFIGURAR RESUMEN
        var lblResumen = new Label { Text = "Resumen generado por IA", Location = new Point(20, 400), AutoSize = true, ForeColor = Color.Gray };
        rtbResumen.Location = new Point(20, 425);
        rtbResumen.Size = new Size(795, 180);
        rtbResumen.BackColor = _fondoPanel;
        rtbResumen.ForeColor = _textoPrincipal;
        rtbResumen.BorderStyle = BorderStyle.None;
        rtbResumen.Font = new Font("Consolas", 11);
        rtbResumen.ReadOnly = true;
        rtbResumen.Cursor = Cursors.Arrow;
        rtbResumen.Enter += (s, e) => { btnLimpiar.Focus(); }; // Bloqueo de foco

        // 7. BOTONES ACCIÓN
        btnLimpiar = CrearBotonModerno("Limpiar", new Point(530, 620), new Size(110, 35), Color.Transparent);
        btnLimpiar.Click += BtnLimpiar_Click;

        btnAnalizar = CrearBotonModerno("Analizar documento", new Point(650, 620), new Size(165, 35), _azulAcento);
        btnAnalizar.Enabled = false;
        btnAnalizar.Click += BtnAnalizar_Click;

        this.Controls.AddRange(new Control[] { lblRuta, txtRuta, btnExaminar, progressBar, lblEstado, lblStats, dgvEstadisticas, lblTokens, flpTokens, lblResumen, rtbResumen, btnLimpiar, btnAnalizar });
    }

    private Button CrearBotonModerno(string texto, Point loc, Size tam, Color backColor)
    {
        return new Button
        {
            Text = texto,
            Location = loc,
            Size = tam,
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(80, 80, 80) }
        };
    }

    private void AgregarTokenChip(string texto, int conteo, bool esImportante)
    {
        // Contenedor principal
        var chipContainer = new FlowLayoutPanel
        {
            AutoSize = true,
            BackColor = esImportante ? _azulAcento : Color.FromArgb(60, 60, 60),
            Padding = new Padding(10, 5, 10, 5), // Un poco más de aire interno
            Margin = new Padding(5),
            WrapContents = false, // Obliga a que el número esté al lado de la palabra
            Cursor = Cursors.Default
        };

        // Palabra
        var lblTexto = new Label
        {
            Text = texto.ToLower(),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft
        };

        // Número (Badge)
        var lblConteo = new Label
        {
            Text = conteo.ToString(),
            AutoSize = true,
            BackColor = Color.FromArgb(45, 0, 0, 0),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 7, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(5, 2, 5, 2), // Padding interno para que el número no toque los bordes
            Margin = new Padding(8, 0, 0, 0)   // Separación clara de la palabra
        };

        // Evitar que el click robe el foco visual
        lblTexto.Click += (s, e) => { btnLimpiar.Focus(); };
        lblConteo.Click += (s, e) => { btnLimpiar.Focus(); };
        chipContainer.Click += (s, e) => { btnLimpiar.Focus(); };

        chipContainer.Controls.Add(lblTexto);
        chipContainer.Controls.Add(lblConteo);

        flpTokens.Controls.Add(chipContainer);
    }

    private void BtnExaminar_Click(object sender, EventArgs e)
    {
        using var dialogo = new OpenFileDialog
        {
            Title = "Selecciona un documento",
            Filter = "Documentos|*.pdf;*.docx;*.txt;*.xlsx;*.pptx;*.html;*.csv|Todos|*.*"
        };

        if (dialogo.ShowDialog() == DialogResult.OK)
        {
            txtRuta.Text = dialogo.FileName;
            btnAnalizar.Enabled = true;
            lblEstado.Text = "Archivo listo. Presiona Analizar documento.";
            lblEstado.ForeColor = _verdeExito;
        }
    }

    private async void BtnAnalizar_Click(object sender, EventArgs e)
    {
        // Bloquear UI y limpiar
        btnAnalizar.Enabled = false;
        btnExaminar.Enabled = false;
        btnLimpiar.Enabled = false;
        progressBar.MarqueeAnimationSpeed = 30;
        dgvEstadisticas.Rows.Clear();
        flpTokens.Controls.Clear();
        rtbResumen.Clear();

        try
        {
            string ruta = txtRuta.Text;

            // 1. Extracción
            lblEstado.Text = "Extrayendo texto...";
            lblEstado.ForeColor = Color.Gray;
            IExtractor extractor = new UniversalExtractor();
            string texto = await Task.Run(() => extractor.ExtraerTexto(ruta));

            // 2. Análisis léxico
            lblEstado.Text = "Analizando léxico...";
            var lexer = new AnalizadorLexico();
            string idioma = lexer.DetectarIdioma(texto);
            var tokens = lexer.Tokenizar(texto, idioma);
            var frecuencias = lexer.ObtenerFrecuencias(tokens);
            var palabrasClave = lexer.ObtenerPalabrasClave(frecuencias);
            var bigramas = lexer.ObtenerBigramas(tokens);
            var numeros = lexer.ExtraerNumeros(texto);
            double diversidad = lexer.DiversidadLexica(tokens, frecuencias);
            double densidad = lexer.DensidadPalabrasClave(tokens, palabrasClave);
            int oraciones = lexer.ContarOraciones(texto);
            double promedio = lexer.PromedioWordsPorOracion(tokens, oraciones);

            // 3. Mostrar estadísticas
            dgvEstadisticas.Rows.Add("Idioma", idioma == "es" ? "Español" : "Inglés");
            dgvEstadisticas.Rows.Add("Palabras", tokens.Count);
            dgvEstadisticas.Rows.Add("Únicas", frecuencias.Count);
            dgvEstadisticas.Rows.Add("Diversidad", $"{diversidad}%");
            dgvEstadisticas.Rows.Add("Densidad", $"{densidad}%");
            dgvEstadisticas.Rows.Add("Oraciones", oraciones);
            dgvEstadisticas.Rows.Add("Prom. palabras", promedio);

            // 4. Mostrar tokens como Chips con sus frecuencias
            int count = 0;
            foreach (var kv in frecuencias.Take(25)) // Subí a 25 para llenar mejor el panel
            {
                // kv.Key es la palabra, kv.Value es la frecuencia
                AgregarTokenChip(kv.Key, kv.Value, count < 5);
                count++;
            }

            // 5. Groq IA
            lblEstado.Text = "Generando resumen con IA...";
            var analizador = new AnalizadorTema();
            string resultado = await analizador.GenerarResumen(texto, palabrasClave, bigramas, idioma, diversidad, numeros);

            rtbResumen.Text = resultado;
            lblEstado.Text = "Análisis completado.";
            lblEstado.ForeColor = _verdeExito;
        }
        catch (Exception ex)
        {
            lblEstado.Text = $"Error: {ex.Message}";
            lblEstado.ForeColor = Color.Red;
        }
        finally
        {
            progressBar.MarqueeAnimationSpeed = 0;
            btnAnalizar.Enabled = true;
            btnExaminar.Enabled = true;
            btnLimpiar.Enabled = true;
        }
    }

    private void BtnLimpiar_Click(object sender, EventArgs e)
    {
        // Limpiar entradas y resultados
        txtRuta.Clear();
        dgvEstadisticas.Rows.Clear();
        flpTokens.Controls.Clear();
        rtbResumen.Clear();

        // Resetear indicadores visuales
        lblEstado.Text = "Esperando documento...";
        lblEstado.ForeColor = Color.Gray;
        progressBar.Value = 0;
        progressBar.MarqueeAnimationSpeed = 0;

        // Resetear botones
        btnAnalizar.Enabled = false;
        btnExaminar.Enabled = true;

        // Quitar foco de cualquier lado
        this.ActiveControl = null;
    }
}