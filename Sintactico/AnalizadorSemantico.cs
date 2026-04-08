using System.Text.RegularExpressions;

namespace AnalizadorDocumentos.Semantico;

public class AnalizadorSemantico
{
    // Diccionarios de sentimiento
    private static readonly HashSet<string> PalabrasPositivas = new(StringComparer.OrdinalIgnoreCase)
    {
        "bueno","excelente","positivo","éxito","logro","beneficio","mejora","avance",
        "óptimo","eficiente","efectivo","favorable","ventaja","oportunidad","progreso",
        "good","great","excellent","success","benefit","improvement","positive","advantage"
    };

    private static readonly HashSet<string> PalabrasNegativas = new(StringComparer.OrdinalIgnoreCase)
    {
        "malo","negativo","fracaso","problema","error","falla","riesgo","daño","pérdida",
        "deficiente","inadecuado","perjudicial","desventaja","obstáculo","crisis","conflicto",
        "bad","negative","failure","problem","error","risk","damage","loss","poor"
    };

    // Analizar sentimiento general del texto
    public (string sentimiento, double confianza) AnalizarSentimiento(List<string> tokens)
    {
        int positivos = tokens.Count(t => PalabrasPositivas.Contains(t));
        int negativos = tokens.Count(t => PalabrasNegativas.Contains(t));
        int total = positivos + negativos;

        if (total == 0) return ("Neutro", 100);

        if (positivos > negativos)
        {
            double confianza = Math.Round((double)positivos / total * 100, 1);
            return ("Positivo", confianza);
        }
        else if (negativos > positivos)
        {
            double confianza = Math.Round((double)negativos / total * 100, 1);
            return ("Negativo", confianza);
        }

        return ("Neutro", 50);
    }

    // Detectar entidades nombradas básicas
    public Dictionary<string, List<string>> DetectarEntidades(string texto)
    {
        var entidades = new Dictionary<string, List<string>>
        {
            { "Fechas", new List<string>() },
            { "Porcentajes", new List<string>() },
            { "Montos", new List<string>() },
            { "Emails", new List<string>() },
            { "URLs", new List<string>() }
        };

        // Fechas
        foreach (Match m in Regex.Matches(texto, @"\b\d{1,2}[\/\-]\d{1,2}[\/\-]\d{2,4}\b"))
            entidades["Fechas"].Add(m.Value);

        foreach (Match m in Regex.Matches(texto,
            @"\b(enero|febrero|marzo|abril|mayo|junio|julio|agosto|septiembre|octubre|noviembre|diciembre)\s+\d{4}\b",
            RegexOptions.IgnoreCase))
            entidades["Fechas"].Add(m.Value);

        // Porcentajes
        foreach (Match m in Regex.Matches(texto, @"\b\d+([.,]\d+)?%"))
            entidades["Porcentajes"].Add(m.Value);

        // Montos monetarios
        foreach (Match m in Regex.Matches(texto, @"(\$|€|£|MXN|USD|EUR)\s?\d+([.,]\d+)?"))
            entidades["Montos"].Add(m.Value);

        // Emails
        foreach (Match m in Regex.Matches(texto, @"\b[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}\b"))
            entidades["Emails"].Add(m.Value);

        // URLs
        foreach (Match m in Regex.Matches(texto, @"https?://[^\s]+"))
            entidades["URLs"].Add(m.Value);

        return entidades;
    }

    // Detectar campo semántico dominante
    public string DetectarCampoSemantico(List<string> palabrasClave)
    {
        var campos = new Dictionary<string, HashSet<string>>
        {
            { "Tecnología",   new HashSet<string> { "sistema","software","datos","red","digital","código","programa","tecnología","computadora","servidor","base","aplicación" } },
            { "Medicina",     new HashSet<string> { "paciente","médico","salud","tratamiento","diagnóstico","enfermedad","clínica","hospital","síntoma","terapia" } },
            { "Legal",        new HashSet<string> { "contrato","ley","artículo","derecho","obligación","cláusula","parte","firma","acuerdo","tribunal","legal","jurídico" } },
            { "Finanzas",     new HashSet<string> { "dinero","costo","presupuesto","ingreso","gasto","inversión","capital","financiero","banco","precio","valor" } },
            { "Educación",    new HashSet<string> { "estudiante","aprendizaje","escuela","universidad","curso","materia","conocimiento","enseñanza","alumno","docente" } },
            { "Ciencia",      new HashSet<string> { "investigación","análisis","método","resultado","experimento","hipótesis","estudio","muestra","variable","dato" } }
        };

        string campoDominante = "General";
        int maxCoincidencias = 0;

        foreach (var campo in campos)
        {
            int coincidencias = palabrasClave.Count(p => campo.Value.Contains(p.ToLower()));
            if (coincidencias > maxCoincidencias)
            {
                maxCoincidencias = coincidencias;
                campoDominante = campo.Key;
            }
        }

        return campoDominante;
    }

    // Calcular coherencia general del documento
    public (string nivel, string descripcion, int puntaje) AnalizarCoherencia(
        List<string> tokens,
        Dictionary<string, int> frecuencias,
        string campoSemantico,
        int oracionesSimples,
        int oracionesCompuestas,
        List<string> patrones)
    {
        int puntaje = 0;

        // 1. Diversidad léxica (vocabulario variado pero no disperso)
        double diversidad = (double)frecuencias.Count / tokens.Count * 100;
        if (diversidad >= 30 && diversidad <= 80) puntaje += 25;
        else if (diversidad > 80) puntaje += 10; // muy disperso
        else puntaje += 5; // muy repetitivo

        // 2. Campo semántico definido
        if (campoSemantico != "General") puntaje += 25;

        // 3. Proporción de oraciones compuestas (texto elaborado)
        int totalOraciones = oracionesSimples + oracionesCompuestas;
        if (totalOraciones > 0)
        {
            double proporcion = (double)oracionesCompuestas / totalOraciones * 100;
            if (proporcion >= 40 && proporcion <= 80) puntaje += 25;
            else if (proporcion > 80) puntaje += 10; // demasiado complejo
            else puntaje += 15; // muy simple
        }

        // 4. Patrones textuales detectados
        if (patrones.Any() && !patrones.Contains("Sin patrones detectados")) puntaje += 25;

        // Determinar nivel
        string nivel, descripcion;
        if (puntaje >= 80)
        {
            nivel = "Alta";
            descripcion = "El documento es coherente, bien estructurado y temáticamente consistente.";
        }
        else if (puntaje >= 50)
        {
            nivel = "Media";
            descripcion = "El documento tiene coherencia aceptable con algunas áreas de mejora.";
        }
        else
        {
            nivel = "Baja";
            descripcion = "El documento puede tener problemas de estructura o consistencia temática.";
        }

        return (nivel, descripcion, puntaje);
    }
}