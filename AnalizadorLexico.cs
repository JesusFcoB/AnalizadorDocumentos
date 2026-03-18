using System.Text.RegularExpressions;

namespace AnalizadorDocumentos.Lexer;

public class AnalizadorLexico
{
    private static readonly HashSet<string> StopWordsEspanol = new(StringComparer.OrdinalIgnoreCase)
    {
        "el","la","los","las","de","del","en","y","a","un","una","por","para",
        "con","que","se","su","es","son","como","más","al","lo","le","este",
        "esta","pero","no","si","ese","esa","esos","esas","ante","bajo","cabe",
        "cada","cual","cuyo","donde","desde","entre","hacia","hasta","mimo",
        "otro","para","poco","sido","todo","tuvo","unos","unas","sobre","tras"
    };

    private static readonly HashSet<string> StopWordsIngles = new(StringComparer.OrdinalIgnoreCase)
    {
        "the","of","and","to","a","in","is","it","that","was","for","on","are",
        "with","as","at","be","by","from","or","an","this","which","but","not",
        "have","had","they","you","we","he","she","his","her","its","our","their",
        "been","has","were","will","would","could","should","than","then","them"
    };

    // Tokenizar según idioma detectado
    public List<string> Tokenizar(string texto, string idioma = "es")
    {
        var stopwords = idioma == "en" ? StopWordsIngles : StopWordsEspanol;

        return Regex.Matches(texto.ToLower(), @"\b[a-záéíóúüñ]{4,}\b")
                    .Select(m => m.Value)
                    .Where(w => !stopwords.Contains(w))
                    .ToList();
    }

    // Frecuencia de palabras
    public Dictionary<string, int> ObtenerFrecuencias(List<string> tokens)
    {
        return tokens
            .GroupBy(t => t)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // Top N palabras clave
    public List<string> ObtenerPalabrasClave(Dictionary<string, int> frecuencias, int top = 15)
    {
        return frecuencias.Take(top).Select(kv => kv.Key).ToList();
    }

    // Bigramas: pares de palabras frecuentes
    public Dictionary<string, int> ObtenerBigramas(List<string> tokens, int top = 10)
    {
        return tokens
            .Zip(tokens.Skip(1), (a, b) => $"{a} {b}")
            .GroupBy(bg => bg)
            .OrderByDescending(g => g.Count())
            .Take(top)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // Diversidad léxica: % de palabras únicas sobre el total
    public double DiversidadLexica(List<string> tokens, Dictionary<string, int> frecuencias)
    {
        if (tokens.Count == 0) return 0;
        return Math.Round((double)frecuencias.Count / tokens.Count * 100, 2);
    }

    // Detectar idioma por frecuencia de stopwords características
    public string DetectarIdioma(string texto)
    {
        string textoLower = texto.ToLower();

        int esp = new[] { "que", "del", "los", "las", "una", "por", "como", "pero", "este", "esta" }
                    .Count(w => Regex.IsMatch(textoLower, $@"\b{w}\b"));

        int eng = new[] { "the", "and", "for", "that", "with", "this", "from", "have", "they", "will" }
                    .Count(w => Regex.IsMatch(textoLower, $@"\b{w}\b"));

        return esp >= eng ? "es" : "en";
    }

    // Densidad de las top palabras: qué tanto dominan el texto
    public double DensidadPalabrasClave(List<string> tokens, List<string> palabrasClave)
    {
        if (tokens.Count == 0) return 0;
        int apariciones = tokens.Count(t => palabrasClave.Contains(t));
        return Math.Round((double)apariciones / tokens.Count * 100, 2);
    }

    // Oraciones del texto (útil para estadísticas)
    public int ContarOraciones(string texto)
    {
        return Regex.Matches(texto, @"[.!?]+").Count;
    }

    // Promedio de palabras por oración
    public double PromedioWordsPorOracion(List<string> tokens, int oraciones)
    {
        if (oraciones == 0) return 0;
        return Math.Round((double)tokens.Count / oraciones, 1);
    }
}