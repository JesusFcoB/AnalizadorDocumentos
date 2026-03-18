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

        return reader.ReadToEnd();
    }
}