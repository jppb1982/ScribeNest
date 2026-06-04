using Microsoft.AspNetCore.Mvc;
using ScribeNest.Web.Api.Dtos;

namespace ScribeNest.Web.Api;

[ApiController]
[Route("api/ai-assistant")]
public class AiAssistantController : ControllerBase
{
    private const int MinimumContentCharacters = 80;

    [HttpPost("suggestions")]
    public ActionResult<AiSuggestionResponse> GenerateSuggestions([FromBody] AiSuggestionRequest request)
    {
        var title = Normalize(request.Title ?? string.Empty);
        var content = Normalize(request.Content ?? string.Empty);
        var category = Normalize(request.Category ?? string.Empty);
        var currentTags = ParseTags(request.Tags ?? string.Empty);

        if (string.IsNullOrWhiteSpace(content))
            return BadRequest("El contenido es requerido para generar sugerencias IA mock.");

        if (content.Length < MinimumContentCharacters)
            return BadRequest($"El contenido debe tener al menos {MinimumContentCharacters} caracteres para generar sugerencias IA mock.");

        var titleContext = AnalyzeContext(title);
        var contentContext = AnalyzeContext(content);
        var categoryContext = AnalyzeCategory(category);
        var context = contentContext;

        var hasMeaningfulContent = HasMeaningfulContent(content, contentContext);
        var hasTechnicalContent = IsTechnical(contentContext);

        var summary = BuildSummary(content, hasMeaningfulContent);
        var excerpt = BuildExcerpt(content, hasMeaningfulContent);
        var suggestedTags = BuildTags(content, contentContext, hasMeaningfulContent);
        var warnings = BuildWarnings(title, content, titleContext, contentContext, categoryContext, category, currentTags);
        var alternativeTitle = BuildAlternativeTitle(content, context, hasMeaningfulContent);
        var editorialConsistency = BuildEditorialConsistency(warnings);
        var editorialNotes = BuildEditorialNotes(warnings);
        var explainForJuniors = BuildJuniorExplanation(content, context, hasTechnicalContent, hasMeaningfulContent);

        return Ok(new AiSuggestionResponse(
            alternativeTitle,
            summary,
            excerpt,
            suggestedTags,
            explainForJuniors,
            warnings,
            editorialConsistency,
            editorialNotes));
    }

    private static string BuildAlternativeTitle(string content, AiContext context, bool hasMeaningfulContent)
    {
        if (!hasMeaningfulContent)
            return "No hay suficiente contexto para sugerir un titulo alternativo.";

        return context switch
        {
            AiContext.Architecture => "Arquitectura simple y mantenible para aplicaciones modernas",
            AiContext.Angular => "Angular como opcion solida para aplicaciones web escalables",
            AiContext.ArtificialIntelligence => "Inteligencia artificial aplicada con criterio al desarrollo de software",
            AiContext.Api => "Buenas practicas para disenar APIs REST mantenibles",
            AiContext.Data => "Metricas y analisis de datos en proyectos full stack",
            AiContext.Career => "Crecimiento profesional en IT: practica, criterio y constancia",
            AiContext.Entertainment => BuildEntertainmentTitle(content),
            AiContext.Music => "Contenido musical o lirico para revision editorial",
            _ => "No hay suficiente contexto claro para sugerir un titulo alternativo."
        };
    }

    private static string BuildEditorialConsistency(IReadOnlyList<string> warnings)
    {
        return warnings.Count switch
        {
            0 => "Alta",
            1 => "Media",
            _ => "Baja"
        };
    }

    private static IReadOnlyList<string> BuildEditorialNotes(IReadOnlyList<string> warnings)
    {
        return warnings.Count == 0 ? [] : warnings;
    }

    private static string BuildEntertainmentTitle(string content)
    {
        var value = content.ToLowerInvariant();

        if (value.Contains("thundercat"))
            return "Thundercats y la cultura animada de los 80";

        if (ContainsAny(value, "dibujos", "cartoon", "animado", "animacion"))
            return "Dibujos animados clasicos y cultura pop";

        return "Contenido de entretenimiento para revision editorial";
    }

    private static string BuildSummary(string content, bool hasMeaningfulContent)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "Resumen no disponible: agrega contenido para generar una sugerencia.";

        if (!hasMeaningfulContent)
            return "Resumen no disponible: el contenido no tiene suficiente contexto claro para resumirlo con confianza.";

        var sentences = SplitSentences(content).Take(2).ToList();
        if (sentences.Count == 0)
            return content.Length <= 220 ? content : content[..220].TrimEnd() + "...";

        return string.Join(" ", sentences);
    }

    private static string BuildExcerpt(string content, bool hasMeaningfulContent)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "Excerpt no disponible.";

        if (!hasMeaningfulContent)
            return "Excerpt no disponible: se necesita contenido mas claro o contextualizado.";

        var firstSentence = SplitSentences(content).FirstOrDefault();
        var source = string.IsNullOrWhiteSpace(firstSentence) ? content : firstSentence;

        return source.Length <= 150
            ? source
            : source[..150].TrimEnd() + "...";
    }

    private static string BuildJuniorExplanation(string content, AiContext context, bool hasTechnicalContent, bool hasMeaningfulContent)
    {
        if (!hasMeaningfulContent)
            return "En simple: todavia no hay suficiente contexto para generar una explicacion util. Agrega problema, contexto, solucion y trade-offs.";

        if (context == AiContext.Career)
            return "En simple: crecer profesionalmente en IT no depende solo de aprender herramientas; tambien importa practicar, construir proyectos, comunicar decisiones y sostener constancia.";

        if (!hasTechnicalContent)
            return "En simple: el contenido tiene un tema reconocible. La explicacion debe resumir esa idea sin depender del titulo, la categoria ni los tags cargados.";

        var normalizedContent = NormalizeForCompare(content);

        return context switch
        {
            AiContext.Architecture => "En simple: una buena arquitectura separa responsabilidades para que el sistema sea mas facil de mantener, probar y extender sin generar acoplamiento innecesario.",
            AiContext.Angular => "En simple: Angular ayuda a organizar una aplicacion frontend en componentes, rutas y servicios, facilitando el mantenimiento cuando el proyecto crece.",
            AiContext.ArtificialIntelligence => "En simple: la IA puede asistir tareas del software, pero hay que integrarla con criterio, considerando datos, costos, limites y calidad de las respuestas.",
            AiContext.Api when IsDotNetContent(normalizedContent) => "En simple: .NET es una tecnologia para crear aplicaciones robustas y mantenibles. Su ecosistema permite construir APIs, servicios y soluciones backend con herramientas maduras para rendimiento, seguridad, testing y despliegue.",
            AiContext.Api => "En simple: una API permite que distintas partes de un sistema se comuniquen mediante reglas claras, endpoints y datos bien definidos.",
            AiContext.Data => "En simple: agregar metricas permite entender mejor el comportamiento del sistema y tomar decisiones basadas en informacion, no solo intuicion.",
            _ => "En simple: identifica el problema, la solucion propuesta y los trade-offs principales del contenido."
        };
    }

    private static IReadOnlyList<string> BuildTags(string content, AiContext contentContext, bool hasMeaningfulContent)
    {
        if (!hasMeaningfulContent)
            return ["Needs Review"];

        var value = $" {content} ".ToLowerInvariant();
        var tags = new List<string>();

        AddIf(value, tags, "dotnet", ".NET");
        AddIf(value, tags, ".net", ".NET");
        AddIf(value, tags, "asp.net", "ASP.NET Core");
        AddIf(value, tags, "angular", "Angular");
        AddIf(value, tags, "api", "REST API");
        AddIf(value, tags, "rest", "REST API");
        AddIf(value, tags, "entity framework", "EF Core");
        AddIf(value, tags, "ef core", "EF Core");
        AddIf(value, tags, "sqlite", "SQLite");
        AddIf(value, tags, "architecture", "Architecture");
        AddIf(value, tags, "arquitectura", "Architecture");
        AddIf(value, tags, "repository", "Repository Pattern");
        AddIf(value, tags, "data", "Data");
        AddIf(value, tags, "datos", "Data");
        AddIf(value, tags, "dashboard", "Dashboard");
        AddIf(value, tags, "ai", "AI");
        AddIf(value, tags, "ia", "AI");
        AddIf(value, tags, "thundercat", "Thundercats");
        AddIf(value, tags, "cartoon", "Cartoons");
        AddIf(value, tags, "dibujos", "Cartoons");
        AddIf(value, tags, "animado", "Animation");
        AddIf(value, tags, "80", "80s");
        AddIf(value, tags, "arroz con leche", "Traditional Song");
        AddIf(value, tags, "cancion", "Music");
        AddIf(value, tags, "canción", "Music");
        AddIf(value, tags, "lyrics", "Lyrics");
        AddIf(value, tags, "career", "Career");
        AddIf(value, tags, "carrera", "Career");
        AddIf(value, tags, "progreso", "Career");
        AddIf(value, tags, "crecer", "Career");
        AddIf(value, tags, "crecimiento", "Career");
        AddIf(value, tags, "aprendizaje", "Learning");
        AddIf(value, tags, "aprender", "Learning");
        AddIf(value, tags, "practica", "Practice");
        AddIf(value, tags, "práctica", "Practice");
        AddIf(value, tags, "constancia", "Consistency");
        AddIf(value, tags, "entrevista", "Interviews");
        AddIf(value, tags, "portfolio", "Portfolio");

        if (tags.Count == 0)
        {
            tags.Add(contentContext switch
            {
                AiContext.Architecture => "Architecture",
                AiContext.Angular => "Angular",
                AiContext.ArtificialIntelligence => "AI",
                AiContext.Api => "REST API",
                AiContext.Data => "Data",
                AiContext.Career => "Career",
                AiContext.Entertainment => "Entertainment",
                AiContext.Music => "Music",
                _ => "Needs Review"
            });
        }

        if (IsSoftwareContext(contentContext) && !tags.Contains("Software Development"))
            tags.Add("Software Development");

        if (contentContext == AiContext.Career && !tags.Contains("Career"))
            tags.Insert(0, "Career");

        if (contentContext == AiContext.Entertainment && !tags.Contains("Entertainment"))
            tags.Add("Entertainment");

        return tags.Distinct(StringComparer.OrdinalIgnoreCase).Take(6).ToList();
    }

    private static IReadOnlyList<string> BuildWarnings(
        string title,
        string content,
        AiContext titleContext,
        AiContext contentContext,
        AiContext categoryContext,
        string category,
        IReadOnlyList<string> currentTags)
    {
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(category))
        {
            warnings.Add("No seleccionaste categoria. Revisala antes de publicar.");
        }
        else if (categoryContext != AiContext.General
            && contentContext != AiContext.General
            && !CategoryMatchesContent(categoryContext, contentContext))
        {
            warnings.Add($"La categoria seleccionada ({category}) no parece coincidir con el contenido.");
        }

        if (!TitleMatchesContent(title, content, titleContext, contentContext))
        {
            warnings.Add("El titulo no parece coincidir con el contenido. Revisa la coherencia editorial antes de publicar.");
        }

        if (currentTags.Count == 0)
        {
            warnings.Add("No cargaste tags. La IA sugiere agregar tags relacionados con el contenido.");
        }
        else if (contentContext != AiContext.General)
        {
            var hasAnyRelatedTag = currentTags.Any(tag => TagMatchesContent(tag, contentContext));

            if (!hasAnyRelatedTag)
                warnings.Add("Los tags actuales podrian no representar correctamente el contenido. La IA sugiere revisarlos, pero no los modifica automaticamente.");
        }

        return warnings.Distinct().ToList();
    }

    private static AiContext AnalyzeContext(string text)
    {
        var value = $" {text} ".ToLowerInvariant();

        var apiScore = CountMatches(value, "asp.net", ".net", "dotnet", " api", " apis", "rest", "endpoint", "http", "json", "controller", "dto", "backend", "microservicios", "middleware");
        var angularScore = CountMatches(value, "angular", "component", "components", "componentes", "routing", "frontend", "typescript", "angular cli");
        var architectureScore = CountMatches(value, "arquitectura", "architecture", "acoplamiento", "coupling", "dependencias", "capas", "responsabilidades", "modular", "mantenibilidad");
        var aiScore = CountMatches(value, "inteligencia artificial", " ia ", " ai ", "chatbot", "modelo", "mock", "openai", "prompt");
        var dataScore = CountMatches(value, "data", "datos", "dashboard", "metrica", "metricas", "analytics", "power bi", "python", "pandas");
        var careerScore = CountMatches(value, "portfolio", "entrevista", "interview", "recruiter", "reclutador", "cv", "career", "carrera", "progreso", "aprendizaje", "aprender", "practicar", "equivocarse", "mejorar", "consistencia", "desarrollador", "desarrolladores", "frustracion", "frustración", "experiencia practica", "experiencia práctica");
        var entertainmentScore = CountMatches(value, "thundercat", "dibujos", "cartoon", "animado", "animacion", "80");
        var musicScore = CountMatches(value, "arroz con leche", "cancion", "song", "lyrics", "zamba");
        var foodScore = CountMatches(value, "helado", "vainilla", "chocolate", "frutilla", "receta", "cocina");

        var scores = new Dictionary<AiContext, int>
        {
            [AiContext.Api] = apiScore,
            [AiContext.Angular] = angularScore,
            [AiContext.Architecture] = architectureScore,
            [AiContext.ArtificialIntelligence] = aiScore,
            [AiContext.Data] = dataScore,
            [AiContext.Career] = careerScore,
            [AiContext.Entertainment] = entertainmentScore,
            [AiContext.Music] = musicScore,
            [AiContext.Food] = foodScore
        };

        var best = scores
            .Where(score => score.Value > 0)
            .OrderByDescending(score => score.Value)
            .ThenBy(score => ContextPriority(score.Key))
            .FirstOrDefault();

        return best.Value > 0 ? best.Key : AiContext.General;
    }

    private static AiContext AnalyzeCategory(string category)
    {
        var value = category.ToLowerInvariant();

        if (ContainsAny(value, ".net", "dotnet"))
            return AiContext.Api;
        if (value.Contains("angular"))
            return AiContext.Angular;
        if (ContainsAny(value, "architecture", "arquitectura"))
            return AiContext.Architecture;
        if (ContainsAny(value, "ai", "ia"))
            return AiContext.ArtificialIntelligence;
        if (ContainsAny(value, "data", "datos"))
            return AiContext.Data;
        if (ContainsAny(value, "career", "carrera"))
            return AiContext.Career;

        return AiContext.General;
    }

    private static bool HasMeaningfulContent(string content, AiContext contentContext)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length < 30)
            return false;

        if (contentContext != AiContext.General)
            return true;

        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length < 8)
            return false;

        var averageWordLength = words.Average(word => word.Length);
        var readableWords = words.Count(word => word.Any("aeiouAEIOU".Contains));

        return averageWordLength < 14 && readableWords >= words.Length * 0.6;
    }

    private static bool CategoryMatchesContent(AiContext categoryContext, AiContext contentContext)
    {
        if (categoryContext == contentContext)
            return true;

        // In this project, the .NET category is mapped to Api because most backend/.NET
        // articles are about ASP.NET Core, REST APIs and backend implementation details.
        // It should not silently accept unrelated contexts such as Data, Career or Entertainment.
        if (categoryContext == AiContext.Api)
            return contentContext is AiContext.Api or AiContext.Architecture;

        if (categoryContext == AiContext.Angular)
            return contentContext is AiContext.Angular or AiContext.Architecture;

        if (categoryContext == AiContext.Architecture)
            return contentContext is AiContext.Architecture or AiContext.Api or AiContext.Angular;

        return false;
    }

    private static bool TitleMatchesContent(string title, string content, AiContext titleContext, AiContext contentContext)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;

        if (contentContext != AiContext.General && titleContext != AiContext.General)
            return ContextsAreCompatible(titleContext, contentContext);

        var normalizedTitle = NormalizeForCompare(title);
        var normalizedContent = NormalizeForCompare(content);

        var meaningfulTitleWords = normalizedTitle
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(word => word.Length >= 4)
            .Where(word => !IsStopWord(word))
            .ToList();

        if (meaningfulTitleWords.Count == 0)
            return false;

        return meaningfulTitleWords.Any(normalizedContent.Contains);
    }

    private static string CategoryLabel(AiContext context)
    {
        return context switch
        {
            AiContext.Architecture => "Architecture",
            AiContext.Angular => "Angular",
            AiContext.ArtificialIntelligence => "AI",
            AiContext.Api => ".NET",
            AiContext.Data => "Data",
            AiContext.Career => "Career",
            AiContext.Entertainment => "Entertainment",
            AiContext.Music => "Music",
            AiContext.Food => "Food",
            _ => "Needs Review"
        };
    }

    private static bool IsStopWord(string word)
    {
        return word is "como" or "hacer" or "para" or "sobre" or "desde" or "hasta" or "cuando" or "porque"
            or "with" or "from" or "this" or "that" or "what" or "when" or "where" or "your"
            or "una" or "uno" or "unos" or "unas" or "con" or "del" or "los" or "las" or "the" or "and";
    }

    private static bool ContextsAreCompatible(AiContext titleContext, AiContext contentContext)
    {
        if (titleContext == contentContext)
            return true;

        if (titleContext == AiContext.Angular)
            return contentContext is AiContext.Angular or AiContext.Architecture;

        if (titleContext == AiContext.Architecture)
            return contentContext is AiContext.Architecture or AiContext.Api or AiContext.Angular;

        if (titleContext == AiContext.Api)
            return contentContext is AiContext.Api or AiContext.Architecture;

        if (titleContext == AiContext.Data)
            return contentContext == AiContext.Data;

        if (titleContext == AiContext.Career)
            return contentContext == AiContext.Career;

        if (titleContext == AiContext.Entertainment)
            return contentContext == AiContext.Entertainment;

        if (titleContext == AiContext.Music)
            return contentContext == AiContext.Music;

        if (titleContext == AiContext.Food)
            return contentContext == AiContext.Food;

        return false;
    }

    private static bool IsTechnical(AiContext context) =>
        context is AiContext.Architecture or AiContext.Angular or AiContext.ArtificialIntelligence or AiContext.Api or AiContext.Data;

    private static bool IsSoftwareContext(AiContext context) =>
        IsTechnical(context) || context == AiContext.Career;

    private static IEnumerable<string> SplitSentences(string value) =>
        value.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(sentence => !string.IsNullOrWhiteSpace(sentence))
            .Select(sentence => sentence.EndsWith('.') ? sentence : sentence + ".");

    private static string Normalize(string value) =>
        string.Join(' ', value.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

    private static IReadOnlyList<string> ParseTags(string value) =>
        value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static string NormalizeForCompare(string value) =>
        value.Trim().ToLowerInvariant();

    private static bool TagMatchesContent(string tag, AiContext contentContext)
    {
        var normalizedTag = NormalizeForCompare(tag);

        var tagContext = AnalyzeContext(tag);
        if (tagContext != AiContext.General && ContextsAreCompatible(tagContext, contentContext))
            return true;

        return contentContext switch
        {
            AiContext.Career => ContainsAny(normalizedTag, "career", "carrera", "portfolio", "entrevista", "interviews", "learning", "aprendizaje", "growth", "progreso", "practice", "practica", "práctica", "consistency", "constancia", "desarrollo profesional", "crecimiento", "it"),
            AiContext.Architecture => ContainsAny(normalizedTag, "architecture", "arquitectura", "software development", "backend"),
            AiContext.Angular => ContainsAny(normalizedTag, "angular", "frontend", "front-end", "typescript", "spa", "componentes", "components"),
            AiContext.ArtificialIntelligence => ContainsAny(normalizedTag, "ai", "ia", "artificial intelligence", "inteligencia artificial"),
            AiContext.Api => ContainsAny(normalizedTag, "api", "rest", ".net", "dotnet", "backend", "back-end", "backend", "asp.net"),
            AiContext.Data => ContainsAny(normalizedTag, "data", "datos", "analytics", "dashboard", "power bi", "python", "bi", "dax"),
            AiContext.Entertainment => ContainsAny(normalizedTag, "entertainment", "cartoons", "animation", "thundercats", "80s"),
            AiContext.Music => ContainsAny(normalizedTag, "music", "song", "lyrics", "cancion", "canción"),
            AiContext.Food => ContainsAny(normalizedTag, "food", "receta", "cocina", "helado", "vainilla", "chocolate", "frutilla"),
            _ => false
        };
    }


    private static bool IsDotNetContent(string normalizedContent) =>
        ContainsAny(normalizedContent, ".net", "dotnet", "asp.net", "c#", "microsoft");

    private static int CountMatches(string text, params string[] keywords) =>
        keywords.Count(text.Contains);

    private static int ContextPriority(AiContext context) =>
        context switch
        {
            AiContext.Api => 0,
            AiContext.Angular => 1,
            AiContext.Architecture => 2,
            AiContext.ArtificialIntelligence => 3,
            AiContext.Data => 4,
            AiContext.Career => 5,
            AiContext.Entertainment => 6,
            AiContext.Music => 7,
            AiContext.Food => 8,
            _ => 9
        };

    private static bool ContainsAny(string text, params string[] keywords) =>
        keywords.Any(text.Contains);

    private static void AddIf(string text, ICollection<string> tags, string keyword, string tag)
    {
        if (text.Contains(keyword) && !tags.Contains(tag))
            tags.Add(tag);
    }

    private enum AiContext
    {
        General,
        Architecture,
        Angular,
        ArtificialIntelligence,
        Api,
        Data,
        Career,
        Entertainment,
        Music,
        Food
    }
}
