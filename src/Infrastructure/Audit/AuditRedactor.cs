using System.Text.Json;
using System.Text.Json.Nodes;

namespace Infrastructure.Audit;

public static class AuditRedactor
{
    private const string Mask = "***";

    private static readonly string[] SensitiveFragments =
    {
        "password", "senha", "token", "secret", "apikey", "api_key", "authorization", "cvv", "salt", "hash"
    };

    private static readonly string[] SensitiveExact = { "code", "pin" };

    public static bool IsSensitive(string name)
    {
        var n = name.ToLowerInvariant();
        return SensitiveExact.Contains(n) || SensitiveFragments.Any(n.Contains);
    }

    /// <summary>Mascara campos sensíveis de um JSON e trunca. Não-JSON é devolvido truncado.</summary>
    public static string? SanitizeJson(string? raw, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        string result;
        try
        {
            var node = JsonNode.Parse(raw);
            Redact(node);
            result = node?.ToJsonString() ?? raw;
        }
        catch (JsonException)
        {
            result = raw;
        }

        return result.Length <= maxLength ? result : result[..maxLength] + "...[truncated]";
    }

    private static void Redact(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var key in obj.Select(p => p.Key).ToList())
                {
                    if (IsSensitive(key)) obj[key] = Mask;
                    else Redact(obj[key]);
                }
                break;
            case JsonArray arr:
                foreach (var item in arr) Redact(item);
                break;
        }
    }
}
