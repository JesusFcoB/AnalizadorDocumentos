using System.Text.RegularExpressions;

namespace AnalizadorDocumentos.Sintactico;

public class AnalizadorSintactico
{
    // Clasificar tipo de oración
    public string ClasificarOracion(string oracion)
    {
        oracion = oracion.Trim();
        if (oracion.EndsWith("?")) return "Interrogativa";
        if (oracion.EndsWith("!")) return "Exclamativa";
        if (oracion.StartsWith("No ") || oracion.StartsWith("no ")) return "Negativa";
        if (Regex.IsMatch(oracion, @"\b(por favor|debe|debería|hay que|es necesario)\b", RegexOptions.IgnoreCase)) return "Imperativa";
        return "Declarativa";
    }

    // Contar tipos de oraciones en el texto
    public Dictionary<string, int> AnalizarTiposOraciones(string texto)
    {
        var oraciones = Regex.Split(texto, @"(?<=[.!?])\s+")
                             .Where(o => o.Length > 5)
                             .ToList();

        var conteo = new Dictionary<string, int>
        {
            { "Declarativa", 0 },
            { "Interrogativa", 0 },
            { "Exclamativa", 0 },
            { "Negativa", 0 },
            { "Imperativa", 0 }
        };

        foreach (var oracion in oraciones)
        {
            string tipo = ClasificarOracion(oracion);
            conteo[tipo]++;
        }

        return conteo;
    }

    // Detectar oraciones simples vs compuestas
    public (int simples, int compuestas) ContarComplejidad(string texto)
    {
        var oraciones = Regex.Split(texto, @"(?<=[.!?])\s+")
                             .Where(o => o.Length > 5)
                             .ToList();

        int simples = 0, compuestas = 0;

        foreach (var oracion in oraciones)
        {
            // Una oración compuesta tiene conectores o múltiples verbos
            bool esCompuesta = Regex.IsMatch(oracion,
                @"\b(y|pero|sino|aunque|porque|cuando|donde|que|si|como|mientras|después|antes)\b",
                RegexOptions.IgnoreCase);

            if (esCompuesta) compuestas++;
            else simples++;
        }

        return (simples, compuestas);
    }

    // Detectar patrones gramaticales básicos
    public List<string> DetectarPatrones(string texto)
    {
        var patrones = new List<string>();

        if (Regex.IsMatch(texto, @"\b(primero|segundo|tercero|además|finalmente|por último)\b", RegexOptions.IgnoreCase))
            patrones.Add("Texto enumerativo");

        if (Regex.IsMatch(texto, @"\b(porque|debido|por lo tanto|en consecuencia|así que)\b", RegexOptions.IgnoreCase))
            patrones.Add("Causa-efecto");

        if (Regex.IsMatch(texto, @"\b(sin embargo|aunque|a pesar|pero|no obstante)\b", RegexOptions.IgnoreCase))
            patrones.Add("Contraste u oposición");

        if (Regex.IsMatch(texto, @"\b(por ejemplo|es decir|como|tal como|esto es)\b", RegexOptions.IgnoreCase))
            patrones.Add("Ejemplificación");

        if (Regex.IsMatch(texto, @"\b(en conclusión|en resumen|finalmente|para concluir)\b", RegexOptions.IgnoreCase))
            patrones.Add("Conclusión explícita");

        if (Regex.IsMatch(texto, @"\b(según|de acuerdo|afirma|señala|indica|menciona)\b", RegexOptions.IgnoreCase))
            patrones.Add("Citas o referencias");

        return patrones.Any() ? patrones : new List<string> { "Sin patrones detectados" };
    }

    // Longitud promedio de oraciones en palabras
    public double PromedioLongitudOraciones(string texto)
    {
        var oraciones = Regex.Split(texto, @"(?<=[.!?])\s+")
                             .Where(o => o.Length > 5)
                             .ToList();

        if (!oraciones.Any()) return 0;

        double promedio = oraciones
            .Select(o => o.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length)
            .Average();

        return Math.Round(promedio, 1);
    }
}