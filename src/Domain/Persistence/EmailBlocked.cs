namespace Domain.Persistence;

public static class EmailBlocklist
{
    private const string ResourceName = "Domain.blocked-domains.txt";

    // Carrega só na primeira chamada e é thread-safe
    private static readonly Lazy<HashSet<string>> BlockedDomains = new(LoadDomains);

    private static HashSet<string> LoadDomains()
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var stream = typeof(EmailBlocklist).Assembly.GetManifestResourceStream(ResourceName)
                           ?? throw new InvalidOperationException($"Recurso '{ResourceName}' não encontrado.");
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            line = line.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            set.Add(line.TrimStart('@'));
        }

        return set;
    }

    public static bool IsBlocked(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        email = email.Trim();

        var atIndex = email.LastIndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1)
            return false;

        var domain = email[(atIndex + 1)..].TrimEnd('.');
        var blocked = BlockedDomains.Value;

        while (true)
        {
            if (blocked.Contains(domain))
                return true;

            var dot = domain.IndexOf('.');
            if (dot == -1)
                return false;

            domain = domain[(dot + 1)..];
        }
    }
}