using AnalizadorDocumentos.Analizador;
using AnalizadorDocumentos.Extractor;
using AnalizadorDocumentos.Lexer;

Console.OutputEncoding = System.Text.Encoding.UTF8;

bool continuar = true;

while (continuar)
{
    Console.Clear();
    Console.WriteLine("=== Analizador Léxico de Documentos ===\n");

    Console.Write("Ingresa la ruta del documento: ");
    string ruta = Console.ReadLine()?.Trim().Trim('"') ?? "";

    if (!File.Exists(ruta))
    {
        Console.WriteLine("❌ Archivo no encontrado.");
        Console.WriteLine("\nPresiona cualquier tecla para intentar de nuevo...");
        Console.ReadKey();
        continue;
    }

    try
    {
        IExtractor extractor = new UniversalExtractor();

        Console.WriteLine($"\n📄 Procesando archivo: {Path.GetFileName(ruta)}");
        string texto = extractor.ExtraerTexto(ruta);

        var lexer = new AnalizadorLexico();
        string idioma = lexer.DetectarIdioma(texto);
        var tokens = lexer.Tokenizar(texto, idioma);
        var frecuencias = lexer.ObtenerFrecuencias(tokens);
        var palabrasClave = lexer.ObtenerPalabrasClave(frecuencias);
        var bigramas = lexer.ObtenerBigramas(tokens);
        double diversidad = lexer.DiversidadLexica(tokens, frecuencias);
        double densidad = lexer.DensidadPalabrasClave(tokens, palabrasClave);
        int oraciones = lexer.ContarOraciones(texto);
        double promedioPalabras = lexer.PromedioWordsPorOracion(tokens, oraciones);

        Console.WriteLine($"\n📊 Estadísticas léxicas:");
        Console.WriteLine($"   Idioma detectado:               {(idioma == "es" ? "Español" : "Inglés")}");
        Console.WriteLine($"   Total palabras (sin stopwords): {tokens.Count}");
        Console.WriteLine($"   Palabras únicas:                {frecuencias.Count}");
        Console.WriteLine($"   Diversidad léxica:              {diversidad}%");
        Console.WriteLine($"   Densidad de palabras clave:     {densidad}%");
        Console.WriteLine($"   Total de oraciones:             {oraciones}");
        Console.WriteLine($"   Promedio palabras por oración:  {promedioPalabras}");

        Console.WriteLine($"\n🔑 Palabras clave más frecuentes:");
        foreach (var kv in frecuencias.Take(10))
            Console.WriteLine($"   {kv.Key,-22} {kv.Value} veces");

        Console.WriteLine($"\n🔗 Frases frecuentes (bigramas):");
        foreach (var kv in bigramas.Take(5))
            Console.WriteLine($"   {kv.Key,-30} {kv.Value} veces");

        Console.Write("\n🤖 Generando resumen");
        var cts = new CancellationTokenSource();
        var animacion = Task.Run(async () => {
            while (!cts.Token.IsCancellationRequested)
            {
                Console.Write(".");
                await Task.Delay(800).ContinueWith(_ => { });
            }
        });

        var analizador = new AnalizadorTema();
        string resultado = await analizador.GenerarResumen(texto, palabrasClave, bigramas, idioma, diversidad);

        cts.Cancel();
        await animacion;
        Console.WriteLine(" ✅\n");

        Console.WriteLine("=== RESULTADO DEL ANÁLISIS ===");
        Console.WriteLine(resultado);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error al procesar el archivo: {ex.Message}");
    }

    // Preguntar si quiere analizar otro documento
    Console.WriteLine("\n¿Qué deseas hacer?");
    Console.WriteLine("   [1] Analizar otro documento");
    Console.WriteLine("   [2] Salir");
    Console.Write("\nOpción: ");

    string opcion = Console.ReadLine()?.Trim() ?? "2";
    continuar = opcion == "1";
}

Console.WriteLine("\n👋 ¡Hasta luego!");
