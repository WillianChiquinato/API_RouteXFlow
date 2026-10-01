using System.Net;
using System.Reflection;

namespace API_RouteXFlow.Emails;

public static class EmailTemplate
{
    /// <summary>
    /// Carrega o template embutido (ex: "ResetPassword") e troca cada {{Chave}} pelo valor, já com HTML encode.
    /// </summary>
    public static string Render(string templateName, IReadOnlyDictionary<string, string> values)
    {
        var assembly = typeof(EmailTemplate).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith($".{templateName}.html", StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException($"Template de e-mail '{templateName}' não encontrado.");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();

        foreach (var (key, value) in values)
        {
            html = html.Replace($"{{{{{key}}}}}", WebUtility.HtmlEncode(value));
        }

        return html;
    }

    /// <summary>123.456.789-09 -> ***.456.789-**</summary>
    public static string MaskCpf(string cpf)
    {
        var digits = new string(cpf.Where(char.IsDigit).ToArray());
        return digits.Length == 11
            ? $"***.{digits.Substring(3, 3)}.{digits.Substring(6, 3)}-**"
            : "***";
    }
}
