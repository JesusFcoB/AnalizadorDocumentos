using AnalizadorDocumentos.Analizador;
using AnalizadorDocumentos.Extractor;
using AnalizadorDocumentos.Lexer;
using OxmlWordprocessing = DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using System.Windows.Forms;
using Button = System.Windows.Forms.Button;
using Label = System.Windows.Forms.Label;
using TextBox = System.Windows.Forms.TextBox;

namespace AnalizadorDocumentos;

public class MainForm : Form
{
    // Colores de la paleta "Modern Dark"
    private readonly Color _fondoOscuro = Color.FromArgb(28, 28, 28);
    private readonly Color _fondoPanel = Color.FromArgb(40, 40, 40);
    private readonly Color _azulAcento = Color.FromArgb(0, 122, 204);
    private readonly Color _textoPrincipal = Color.FromArgb(224, 224, 224);
    private readonly Color _verdeExito = Color.FromArgb(15, 110, 86);

    private DataGridView dgvSintactico;
    private DataGridView dgvSemantico;
    private TextBox txtRuta;
    private Button btnExaminar;
    private Button btnAnalizar;
    private Button btnLimpiar;
    private ProgressBar progressBar;
    private Label lblEstado;
    private FlowLayoutPanel flpTokens;
    private DataGridView dgvEstadisticas;
    private RichTextBox rtbResumen;
    private DataGridView dgvIncoherencias;
    private RichTextBox rtbCorrecciones;
    public MainForm()
    {
        InicializarComponentes();
    }

    // Método para configurar la interfaz gráfica
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
        this.Size = new Size(850, 700); // Tamaño de la ventana visible
        this.MinimumSize = new Size(850, 600);
        this.BackColor = _fondoOscuro;
        this.ForeColor = _textoPrincipal;
        this.Font = new Font("Segoe UI", 10);
        this.StartPosition = FormStartPosition.CenterScreen;

        // Panel con scroll que contiene todo
        var panelScroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = _fondoOscuro
        };
        this.Controls.Add(panelScroll);

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
        // AHORA:
        rtbResumen.ReadOnly = true;
        rtbResumen.Cursor = Cursors.IBeam;      // cursor de texto para seleccionar
        rtbResumen.ShortcutsEnabled = true;     // habilita Ctrl+C, Ctrl+A
        rtbResumen.Enter -= (s, e) => { btnLimpiar.Focus(); }; // quita el bloqueo de foco

        var btnCopiar = CrearBotonModerno("Copiar", new Point(695, 398), new Size(120, 24), Color.FromArgb(60, 60, 60));
        btnCopiar.Font = new Font("Segoe UI", 8);
        btnCopiar.Click += (s, e) =>
        {
            if (!string.IsNullOrEmpty(rtbResumen.Text))
            {
                Clipboard.SetText(rtbResumen.Text);
                btnCopiar.Text = "¡Copiado!";
                Task.Delay(1500).ContinueWith(_ =>
                    btnCopiar.Invoke(() => btnCopiar.Text = "Copiar"));
            }
        };

        var btnGuardar = CrearBotonModerno("Guardar", new Point(570, 398), new Size(120, 24), Color.FromArgb(60, 60, 60));
        btnGuardar.Font = new Font("Segoe UI", 8);
        btnGuardar.Click += (s, e) =>
        {
            if (string.IsNullOrEmpty(rtbResumen.Text)) return;

            using var dialogo = new SaveFileDialog
            {
                Title = "Guardar resumen",
                Filter = "Archivo de texto|*.txt|Word (.docx)|*.docx|PDF|*.pdf",
                FileName = "resumen_documento"
            };

            if (dialogo.ShowDialog() != DialogResult.OK) return;

            string textoLimpio = rtbResumen.Text
                .Replace("**", "")
                .Replace("*", "")
                .Trim();

            string extension = Path.GetExtension(dialogo.FileName).ToLower();

            try
            {
                switch (extension)
                {
                    case ".txt":
                        GuardarTxt(dialogo.FileName, textoLimpio);
                        break;
                    case ".docx":
                        GuardarDocx(dialogo.FileName, textoLimpio);
                        break;
                    case ".pdf":
                        GuardarPdf(dialogo.FileName, textoLimpio);
                        break;
                }

                btnGuardar.Text = "¡Guardado!";
                Task.Delay(1500).ContinueWith(_ =>
                    btnGuardar.Invoke(() => btnGuardar.Text = "Guardar"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        // ANÁLISIS SINTÁCTICO
        var lblSintactico = new Label { Text = "ANÁLISIS SINTÁCTICO", Location = new Point(20, 615), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = _textoPrincipal };
        dgvSintactico = new DataGridView { Location = new Point(20, 638), Size = new Size(380, 130), BackgroundColor = _fondoPanel, GridColor = Color.FromArgb(60, 60, 60), BorderStyle = BorderStyle.None, RowHeadersVisible = false, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };
        dgvSintactico.Columns.Add("Aspecto", "Aspecto");
        dgvSintactico.Columns.Add("Resultado", "Resultado");
        EstilarDataGrid(dgvSintactico);

        // ANÁLISIS SEMÁNTICO
        var lblSemantico = new Label { Text = "ANÁLISIS SEMÁNTICO", Location = new Point(420, 615), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = _textoPrincipal };
        dgvSemantico = new DataGridView { Location = new Point(420, 638), Size = new Size(395, 130), BackgroundColor = _fondoPanel, GridColor = Color.FromArgb(60, 60, 60), BorderStyle = BorderStyle.None, RowHeadersVisible = false, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };
        dgvSemantico.Columns.Add("Aspecto", "Aspecto");
        dgvSemantico.Columns.Add("Resultado", "Resultado");
        EstilarDataGrid(dgvSemantico);

        dgvSemantico.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        dgvSemantico.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;


        // PANEL DE INCOHERENCIAS
        var lblIncoherencias = new Label
        {
            Text = "ANÁLISIS DE COHERENCIA Y SUGERENCIAS",
            Location = new Point(20, 790),
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = _textoPrincipal
        };

        dgvIncoherencias = new DataGridView
        {
            Location = new Point(20, 815),
            Size = new Size(795, 130),
            BackgroundColor = _fondoPanel,
            GridColor = Color.FromArgb(60, 60, 60),
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ReadOnly = true
        };
        dgvIncoherencias.Columns.Add("Oracion", "Oración problemática");
        dgvIncoherencias.Columns.Add("Razon", "Razón");
        dgvIncoherencias.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        dgvIncoherencias.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        EstilarDataGrid(dgvIncoherencias);

        // Highlight rojo suave para filas de incoherencias
        dgvIncoherencias.DefaultCellStyle.BackColor = Color.FromArgb(55, 30, 30);
        dgvIncoherencias.DefaultCellStyle.ForeColor = Color.FromArgb(255, 180, 180);

        var lblCorrecciones = new Label
        {
            Text = "CORRECCIONES Y SUGERENCIAS DE MEJORA POR IA",
            Location = new Point(20, 960),
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = _textoPrincipal
        };

        rtbCorrecciones = new RichTextBox
        {
            Location = new Point(20, 985),
            Size = new Size(795, 180),
            BackColor = _fondoPanel,
            ForeColor = _textoPrincipal,
            BorderStyle = BorderStyle.None,
            Font = new Font("Consolas", 10),
            ReadOnly = true,
            ScrollBars = RichTextBoxScrollBars.Vertical
        };


        dgvSintactico.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        dgvSintactico.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

        // 7. BOTONES ACCIÓN
        btnLimpiar = CrearBotonModerno("Limpiar", new Point(530, 790), new Size(110, 35), Color.Transparent);
        btnLimpiar.Click += BtnLimpiar_Click;

        btnAnalizar = CrearBotonModerno("Analizar documento", new Point(650, 790), new Size(165, 35), _azulAcento);
        btnAnalizar.Enabled = false;
        btnAnalizar.Click += BtnAnalizar_Click;

        this.Size = new Size(850, 1300); // era 850, 900

        btnLimpiar.Location = new Point(530, 1185);
        btnAnalizar.Location = new Point(650, 1185);

        panelScroll.Controls.AddRange(new Control[]
        {
            lblRuta, txtRuta, btnExaminar,
            progressBar, lblEstado,
            lblStats, dgvEstadisticas,
            lblTokens, flpTokens,
            lblResumen, rtbResumen, btnCopiar, btnGuardar, 
            lblSintactico, dgvSintactico,
            lblSemantico, dgvSemantico,
            lblIncoherencias, dgvIncoherencias,
            lblCorrecciones, rtbCorrecciones,
            btnLimpiar, btnAnalizar
        });
    }
    
    

    private void EstilarDataGrid(DataGridView dgv) // Aplica un estilo moderno y oscuro a las tablas
    {
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.DefaultCellStyle.BackColor = _fondoPanel;
        dgv.DefaultCellStyle.ForeColor = Color.White;
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(55, 55, 55);
        dgv.DefaultCellStyle.SelectionForeColor = Color.White;
        dgv.EnableHeadersVisualStyles = false;
    }

    // Método para crear botones con estilo moderno
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
        dgvIncoherencias.Rows.Clear();
        rtbCorrecciones.Clear();

        string texto = string.Empty;
        string campo = string.Empty;
        string idioma = "es";
        AnalizadorDocumentos.Semantico.AnalizadorSemantico? semantico = null;
        AnalizadorTema? analizador = null;

        try
        {
            string ruta = txtRuta.Text;

            // 1. Extracción
            lblEstado.Text = "Extrayendo texto...";
            lblEstado.ForeColor = Color.Gray;
            IExtractor extractor = new UniversalExtractor();
            texto = await Task.Run(() => extractor.ExtraerTexto(ruta));

            // 2. Análisis léxico
            lblEstado.Text = "Analizando léxico...";
            var lexer = new AnalizadorLexico();
            idioma = lexer.DetectarIdioma(texto);
            var tokens = lexer.Tokenizar(texto, idioma);
            var frecuencias = lexer.ObtenerFrecuencias(tokens);
            var palabrasClave = lexer.ObtenerPalabrasClave(frecuencias);
            var bigramas = lexer.ObtenerBigramas(tokens);
            var numeros = lexer.ExtraerNumeros(texto);
            double diversidad = lexer.DiversidadLexica(tokens, frecuencias);
            double densidad = lexer.DensidadPalabrasClave(tokens, palabrasClave);
            int oraciones = lexer.ContarOraciones(texto);
            double promedio = lexer.PromedioWordsPorOracion(tokens, oraciones);

            // 3. Análisis sintáctico
            lblEstado.Text = "Analizando sintaxis...";
            var sintactico = new AnalizadorDocumentos.Sintactico.AnalizadorSintactico();
            var tiposOraciones = sintactico.AnalizarTiposOraciones(texto);
            var (simples, compuestas) = sintactico.ContarComplejidad(texto);
            var patrones = sintactico.DetectarPatrones(texto);
            double promedioLongitud = sintactico.PromedioLongitudOraciones(texto);

            dgvSintactico.Rows.Clear();
            foreach (var tipo in tiposOraciones.Where(t => t.Value > 0))
                dgvSintactico.Rows.Add(tipo.Key, tipo.Value);
            dgvSintactico.Rows.Add("Simples", simples);
            dgvSintactico.Rows.Add("Compuestas", compuestas);
            dgvSintactico.Rows.Add("Patrones", string.Join(", ", patrones));

            // 4. Análisis semántico
            lblEstado.Text = "Analizando semántica...";
            semantico = new AnalizadorDocumentos.Semantico.AnalizadorSemantico();
            var (sentimiento, confianza) = semantico.AnalizarSentimiento(tokens);
            var entidades = semantico.DetectarEntidades(texto);
            campo = semantico.DetectarCampoSemantico(palabrasClave);

            dgvSemantico.Rows.Clear();
            dgvSemantico.Rows.Add("Sentimiento", $"{sentimiento} ({confianza}%)");
            dgvSemantico.Rows.Add("Campo semántico", campo);
            foreach (var ent in entidades.Where(e => e.Value.Any()))
                dgvSemantico.Rows.Add(ent.Key, string.Join(", ", ent.Value.Take(3)));

            // Coherencia
            var (nivelCoherencia, descCoherencia, puntajeCoherencia) =
                semantico.AnalizarCoherencia(tokens, frecuencias, campo, simples, compuestas, patrones);

            dgvSemantico.Rows.Add("Coherencia", $"{nivelCoherencia} ({puntajeCoherencia}/100)");
            dgvSemantico.Rows.Add("", descCoherencia);

            // 5. Estadísticas léxicas
            dgvEstadisticas.Rows.Add("Idioma", idioma == "es" ? "Español" : "Inglés");
            dgvEstadisticas.Rows.Add("Palabras", tokens.Count);
            dgvEstadisticas.Rows.Add("Únicas", frecuencias.Count);
            dgvEstadisticas.Rows.Add("Diversidad", $"{diversidad}%");
            dgvEstadisticas.Rows.Add("Densidad", $"{densidad}%");
            dgvEstadisticas.Rows.Add("Oraciones", oraciones);
            dgvEstadisticas.Rows.Add("Prom. palabras", promedio);

            // 6. Tokens chips
            int count = 0;
            foreach (var kv in frecuencias.Take(25))
            {
                AgregarTokenChip(kv.Key, kv.Value, count < 5);
                count++;
            }

            // 7. Resumen IA
            lblEstado.Text = "Generando resumen con IA...";
            analizador = new AnalizadorTema();
            string resultado = await analizador.GenerarResumen(
                texto, palabrasClave, bigramas, idioma, diversidad, numeros,
                sentimiento, campo, patrones, simples, compuestas,
                nivelCoherencia, puntajeCoherencia
            );
            rtbResumen.Text = resultado;

            // 8. Detección y corrección de incoherencias
            lblEstado.Text = "Detectando incoherencias...";
            var incoherencias = semantico.DetectarOracionesIncoherentes(texto, campo);

            // Si el campo es General o no se detectaron problemas locales,
            // pedir a la IA que haga el análisis completo
            if (!incoherencias.Any())
            {
                lblEstado.Text = "Analizando coherencia con IA...";
                string correcciones = await analizador.CorregirOraciones(
                    ObtenerOracionesParaIA(texto), idioma);
                dgvIncoherencias.Rows.Add("Análisis delegado a IA", "Ver sugerencias abajo");
                rtbCorrecciones.Text = correcciones; // ← muestra lo que dijo la IA, no texto fijo
            }
            else
            {
                foreach (var (oracion, razon) in incoherencias)
                {
                    string fragmento = oracion.Length > 80 ? oracion[..80] + "..." : oracion;
                    dgvIncoherencias.Rows.Add(fragmento, razon);
                }
                lblEstado.Text = "Corrigiendo oraciones con IA...";
                string correcciones = await analizador.CorregirOraciones(incoherencias, idioma);
                rtbCorrecciones.Text = correcciones;
            }

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
        dgvSintactico.Rows.Clear();  // agrega esto
        dgvSemantico.Rows.Clear();   // agrega esto
        dgvIncoherencias.Rows.Clear();
        rtbCorrecciones.Clear();

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

    // Nota: Simple y universal, pero sin formato.
    // Ideal para resúmenes puros o si el usuario quiere editar después en cualquier editor de texto.
    private void GuardarTxt(string ruta, string texto) 
    {
        File.WriteAllText(ruta, texto, System.Text.Encoding.UTF8);
    }

    // Nota: OpenXML es un poco más complejo pero permite formato
    // Básico sin depender de Word instalado
    private void GuardarDocx(string ruta, string texto)
    {
        using var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument
            .Create(ruta, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);

        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new OxmlWordprocessing.Document();
        var body = mainPart.Document.AppendChild(
            new OxmlWordprocessing.Body());

        foreach (var linea in texto.Split('\n'))
        {
            var para = body.AppendChild(new OxmlWordprocessing.Paragraph());
            var run = para.AppendChild(new OxmlWordprocessing.Run());
            run.AppendChild(new OxmlWordprocessing.Text(linea.Trim()));
        }

        mainPart.Document.Save();
    }

    // Nota: QuestPDF es muy rápido y sencillo para generar PDFs con formato básico
    private void GuardarPdf(string ruta, string texto) 
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(QuestPDF.Helpers.PageSizes.A4);
                page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Content().Column(col =>
                {
                    col.Item().Text("Resumen del documento")
                        .FontSize(16)
                        .Bold();

                    col.Item().PaddingTop(10).Text("");

                    foreach (var linea in texto.Split('\n'))
                    {
                        if (!string.IsNullOrWhiteSpace(linea))
                            col.Item().Text(linea.Trim());
                        else
                            col.Item().PaddingTop(5).Text("");
                    }
                });
            });
        }).GeneratePdf(ruta);
    }

    private List<(string oracion, string razon)> ObtenerOracionesParaIA(string texto)
    {
        // Patrones a ignorar: copyright, fechas sueltas, títulos muy cortos, código
        var patronesIgnorar = new[]
        {
        @"^copyright\s",
        @"^\d{4}\s",
        @"^www\.",
        @"^https?://",
        @"@",                          // emails
        @"^\s*[A-Z][a-z]+\s*$",        // título de una sola palabra
        @"[{}();=<>]",                 // código de programación
        @"^(página|page)\s*\d",        // pies de página
        @"^\d+$",                      // solo números
        @"all rights reserved",
        @"todos los derechos"
    };

        return System.Text.RegularExpressions.Regex
            .Split(texto, @"(?<=[.!?])\s+")
            .Where(o => o.Trim().Length > 25)
            .Where(o => !patronesIgnorar.Any(p =>
                System.Text.RegularExpressions.Regex.IsMatch(
                    o.Trim(), p, System.Text.RegularExpressions.RegexOptions.IgnoreCase)))
            .Take(8)
            .Select(o => (o.Trim(), "Revisar coherencia y estilo"))
            .ToList();
    }
}