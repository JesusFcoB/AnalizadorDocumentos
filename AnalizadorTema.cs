using System.Net.Http.Json;
using System.Text.Json;

namespace AnalizadorDocumentos.Analizador;

public class AnalizadorTema
{
    private readonly HttpClient _http;
    private const string Url = "https://api.groq.com/openai/v1/chat/completions";
    private const string ApiKey = ""; 
    private const string Modelo = "llama-3.1-8b-instant";
    private const int TamanoFragmento = 3000; // Groq aguanta más texto

    public AnalizadorTema()
    {
        _http = new HttpClient();
        _http.Timeout = TimeSpan.FromMinutes(2);
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
    }

    public async Task<string> GenerarResumen(
    string textoCompleto,
    List<string> palabrasClave,
    Dictionary<string, int> bigramas,
    string idioma,
    double diversidad,
    List<string> numeros,
    // Nuevos parámetros
    string sentimiento,
    string campoSemantico,
    List<string> patrones,
    int oracionesSimples,
    int oracionesCompuestas,
    string nivelCoherencia,
    int puntajeCoherencia)
    {
        var fragmentos = DividirEnFragmentos(textoCompleto, TamanoFragmento);
        string textoParaResumen;

        if (fragmentos.Count == 1)
        {
            textoParaResumen = fragmentos[0];
        }
        else
        {
            Console.WriteLine($"   📑 Documento largo ({fragmentos.Count} fragmentos). Procesando...\n");
            var resumenesParciales = new List<string>();

            for (int i = 0; i < fragmentos.Count; i++)
            {
                Console.WriteLine($"   ⏳ Procesando fragmento {i + 1} de {fragmentos.Count}...");
                string resumenParcial = await LlamarGroq(
                    $"Resume brevemente este fragmento en 3-5 oraciones, " +
                    $"conservando las ideas principales:\n\n{fragmentos[i]}"
                );
                resumenesParciales.Add(resumenParcial);
            }

            textoParaResumen = string.Join("\n\n", resumenesParciales);
            Console.WriteLine("\n   ✅ Fragmentos listos. Generando resumen final...\n");
        }

        string promptFinal = $"""
    Analiza el siguiente documento y genera un resumen completo en {(idioma == "es" ? "español" : "inglés")}.

    Datos del análisis léxico:
    - Palabras clave más frecuentes: {string.Join(", ", palabrasClave)}
    - Frases frecuentes: {string.Join(", ", bigramas.Take(5).Select(b => b.Key))}
    - Números relevantes: {(numeros.Any() ? string.Join(", ", numeros) : "ninguno")}
    - Diversidad léxica: {diversidad}%

    Datos del análisis sintáctico:
    - Oraciones simples: {oracionesSimples}
    - Oraciones compuestas: {oracionesCompuestas}
    - Patrones detectados: {string.Join(", ", patrones)}

    Datos del análisis semántico:
    - Sentimiento general: {sentimiento}
    - Campo semántico: {campoSemantico}
    - Coherencia del documento: {nivelCoherencia} ({puntajeCoherencia}/100)

    Texto:
    {textoParaResumen}

    El resumen debe incluir:
    - Tema principal: De qué trata el documento en una oración.
    - Tipo de documento: (académico, técnico, legal, literario, etc.)
    - Tono del documento: basado en el sentimiento detectado.
    - Resumen general: Un párrafo de 5-8 oraciones explicando el contenido.
    - Puntos clave: Lista de 4-6 ideas más importantes.
    - Conclusión: Para qué sirve o a qué conclusión llega.
    """;

        return await LlamarGroq(promptFinal);
    }

    private async Task<string> LlamarGroq(string prompt)
    {
        var body = new
        {
            model = Modelo,
            max_tokens = 1000,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var response = await _http.PostAsJsonAsync(Url, body);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error Groq: {response.StatusCode} - {error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement
                  .GetProperty("choices")[0]
                  .GetProperty("message")
                  .GetProperty("content")
                  .GetString() ?? "Sin respuesta.";
    }

    private static List<string> DividirEnFragmentos(string texto, int tamano)
    {
        var fragmentos = new List<string>();
        int inicio = 0;

        while (inicio < texto.Length)
        {
            int longitud = Math.Min(tamano, texto.Length - inicio);

            if (inicio + longitud < texto.Length)
            {
                int corte = texto.LastIndexOfAny(new[] { '.', '\n' }, inicio + longitud, longitud / 2);
                if (corte > inicio) longitud = corte - inicio + 1;
            }

            fragmentos.Add(texto.Substring(inicio, longitud).Trim());
            inicio += longitud;
        }

        return fragmentos;
    }
}