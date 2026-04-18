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


    // Detectar oraciones con baja coherencia local
    public List<(string oracion, string razon)> DetectarOracionesIncoherentes(string texto, string campoSemantico)
    {
        var resultado = new List<(string, string)>();
        var oraciones = System.Text.RegularExpressions.Regex
            .Split(texto, @"(?<=[.!?])\s+")
            .Where(o => o.Trim().Length > 15)
            .ToList();

        var camposPalabras = new Dictionary<string, HashSet<string>>
    {
        { "Tecnología",  new HashSet<string> { "sistema","software","datos","red","digital","código","programa","tecnología","computadora","servidor","base","aplicación" } },
        { "Medicina",    new HashSet<string> { "paciente","médico","salud","tratamiento","diagnóstico","enfermedad","clínica","hospital","síntoma","terapia" } },
        { "Legal",       new HashSet<string> { "contrato","ley","artículo","derecho","obligación","cláusula","parte","firma","acuerdo","tribunal","legal","jurídico" } },
        { "Finanzas",    new HashSet<string> { "dinero","costo","presupuesto","ingreso","gasto","inversión","capital","financiero","banco","precio","valor" } },
        { "Educación",   new HashSet<string> { "estudiante","aprendizaje","escuela","universidad","curso","materia","conocimiento","enseñanza","alumno","docente" } },
        { "Ciencia",     new HashSet<string> { "investigación","análisis","método","resultado","experimento","hipótesis","estudio","muestra","variable","dato" } }
    };

        HashSet<string>? palabrasDelCampo = campoSemantico != "General" && camposPalabras.ContainsKey(campoSemantico)
            ? camposPalabras[campoSemantico]
            : null;

        // Calcular longitud promedio de oraciones para detectar outliers
        var longitudes = oraciones
            .Select(o => o.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length)
            .ToList();
        double promedioLong = longitudes.Any() ? longitudes.Average() : 10;

        // Al inicio del método, antes del for loop, agrega este filtro:
        var patronesIgnorar = new[]
{
    @"^copyright\s",
    @"^©",
    @"all rights reserved",
    @"todos los derechos",
    @"[{}();=<>]",
    @"^https?://",
    @"^\d+$",
    @"^www\."
};

        for (int i = 0; i < oraciones.Count; i++)
        {
            string oracion = oraciones[i];

            // Ignorar fragmentos que no son oraciones reales
            if (patronesIgnorar.Any(p => Regex.IsMatch(oracion.Trim(), p, RegexOptions.IgnoreCase)))
                continue;

            string lower = oracion.ToLower();
            var palabras = Regex.Matches(lower, @"\b[a-záéíóúüñ]{3,}\b")
                .Select(m => m.Value)
                .ToList();

            if (palabras.Count < 3) continue;

            // ... resto del código igual

            var razones = new List<string>();

            // 1. Oración fuera del campo semántico (umbral bajado: solo 6+ palabras)
            if (palabrasDelCampo != null && palabras.Count >= 6)
            {
                int hits = palabras.Count(p => palabrasDelCampo.Contains(p));
                if (hits == 0)
                    razones.Add($"Sin términos del campo '{campoSemantico}'");
            }

            // 2. Mezcla de sentimientos (bajado a 1+1)
            int pos = palabras.Count(p => PalabrasPositivas.Contains(p));
            int neg = palabras.Count(p => PalabrasNegativas.Contains(p));
            if (pos >= 1 && neg >= 2)
                razones.Add("Mezcla de términos positivos y negativos");

            // 3. Oración muy corta (idea posiblemente incompleta)
            if (palabras.Count <= 4 && oracion.Trim().Length > 10)
                razones.Add("Oración muy corta, posible idea incompleta");

            // 4. Oración excesivamente larga (difícil de seguir)
            int numPalabras = oracion.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            if (numPalabras > promedioLong * 2.5 && numPalabras > 40)
                razones.Add($"Oración muy larga ({numPalabras} palabras), puede perder coherencia");

            // 5. Repetición excesiva (bajado a 2 repeticiones)
            var repetidas = palabras
                .Where(p => p.Length > 4)
                .GroupBy(p => p)
                .Where(g => g.Count() >= 2)
                .Select(g => g.Key)
                .ToList();
            if (repetidas.Count >= 2)
                razones.Add($"Palabras repetidas: '{string.Join(", ", repetidas.Take(3))}'");

            // 6. Contradicción directa con oración anterior
            if (i > 0)
            {
                string anteriorLower = oraciones[i - 1].ToLower();
                bool anteriorPositiva = PalabrasPositivas.Any(p => anteriorLower.Contains(p));
                bool anteriorNegativa = PalabrasNegativas.Any(p => anteriorLower.Contains(p));
                bool actualPositiva = PalabrasPositivas.Any(p => lower.Contains(p));
                bool actualNegativa = PalabrasNegativas.Any(p => lower.Contains(p));

                if ((anteriorPositiva && !anteriorNegativa) && (actualNegativa && !actualPositiva))
                    razones.Add("Contraste abrupto con la oración anterior (positivo → negativo)");
                else if ((anteriorNegativa && !anteriorPositiva) && (actualPositiva && !actualNegativa))
                    razones.Add("Contraste abrupto con la oración anterior (negativo → positivo)");
            }

            // 7. Conectores usados incorrectamente (conclusión sin desarrollo previo)
            bool tieneConclusion = System.Text.RegularExpressions.Regex.IsMatch(lower,
                @"\b(en conclusión|en resumen|por lo tanto|en consecuencia|finalmente)\b");
            bool esSegundaOracion = i <= 1;
            if (tieneConclusion && esSegundaOracion)
                razones.Add("Conector de conclusión usado demasiado pronto en el texto");

            if (razones.Any())
                resultado.Add((oracion.Trim(), string.Join("; ", razones)));
        }

        return resultado;
    }
}