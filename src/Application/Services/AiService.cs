using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using API_RouteXFlow.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace API_RouteXFlow.Services;

public class AiService : IAiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AiService> _logger;

    public AiService(IHttpClientFactory httpClientFactory, ILogger<AiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> AnalyzeReportAsync(string reportText, CancellationToken cancellationToken = default)
    {
        var apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY")
            ?? throw new InvalidOperationException("Configure a variável de ambiente GROQ_API_KEY.");

        var model = Environment.GetEnvironmentVariable("GROQ_MODEL") ?? "llama-3.3-70b-versatile";

        var client = _httpClientFactory.CreateClient("Groq");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var request = new ChatRequest(
            Model: model,
            Messages:
            [
                new ChatMessage("system",
                    "Você é um analista financeiro. Responda sempre em português do Brasil, " +
                    "de forma clara e objetiva. Use apenas os números fornecidos no relatório, " +
                    "sem inventar dados."),
                new ChatMessage("user",
                    "Analise o relatório financeiro abaixo e escreva um texto corrido com resumo geral, " +
                    "principais tendências e pontos de atenção. Ao final, adicione uma seção que comece " +
                    "exatamente com a linha \"RECOMENDAÇÕES:\" seguida de 3 a 5 recomendações práticas do " +
                    "que pode melhorar, cada uma em uma linha iniciando com \"- \".\n\n" +
                    $"RELATÓRIO:\n{reportText}")
            ],
            Temperature: 0.4,
            MaxTokens: 1500);

        using var response = await client.PostAsJsonAsync("chat/completions", request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new HttpRequestException(
                "Limite de uso da Groq atingido. Tente novamente em alguns instantes.",
                null, response.StatusCode);
        }

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Erro na Groq ({StatusCode}): {Erro}", (int)response.StatusCode, erro);
            throw new HttpRequestException(
                $"Erro na Groq ({(int)response.StatusCode}): {erro}",
                null, response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: cancellationToken);

        return result?.Choices.FirstOrDefault()?.Message.Content?.Trim()
            ?? "A IA não retornou nenhuma análise.";
    }

    private record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private record ChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] ChatMessage[] Messages,
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("max_tokens")] int MaxTokens);

    private record ChatResponse(
        [property: JsonPropertyName("choices")] List<Choice> Choices);

    private record Choice(
        [property: JsonPropertyName("message")] ChatMessage Message);
}
