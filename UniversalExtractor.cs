using GroupDocs.Parser;

namespace AnalizadorDocumentos.Extractor;

public class UniversalExtractor : IExtractor
{
    public string ExtraerTexto(string ruta)
    {
        using var parser = new Parser(ruta);
        using var reader = parser.GetText();

        if (reader == null)
            throw new Exception("Formato no soportado por GroupDocs.Parser");

        string texto = reader.ReadToEnd();

        // Limpiar guiones de separación silábica (soft hyphens)
        texto = texto.Replace("\u00AD", "");        // soft hyphen invisible
        texto = System.Text.RegularExpressions.Regex.Replace(
            texto, @"(\w+)\-\n(\w+)", "$1$2");      // guión al final de línea

        return texto;
    }
}