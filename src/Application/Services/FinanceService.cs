using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.Extensions.Logging;

namespace API_RouteXFlow.Services;

public class FinanceService : IFinanceService
{
    private readonly IFinanceRepository _financeRepository;
    private readonly IAiService _aiService;
    private readonly ILogger<FinanceService> _logger;

    public FinanceService(IFinanceRepository financeRepository, IAiService aiService, ILogger<FinanceService> logger)
    {
        _financeRepository = financeRepository;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<CustomResponse<List<FinanceEntry>>> GetEntries(FinanceFilterRequest filter, int userId)
    {
        try
        {
            var entries = await _financeRepository.GetEntriesAsync(userId, filter);

            if (!entries.Any())
            {
                _logger.LogError("Nenhuma movimentação encontrada no GetEntries para o usuário {UserId}", userId);
                return new CustomResponse<List<FinanceEntry>>(false, new List<string> { "Nenhuma movimentação encontrada." }, new List<FinanceEntry>());
            }

            return new CustomResponse<List<FinanceEntry>>(true, new List<string>(), entries);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao buscar movimentações financeiras");
            throw;
        }
    }

    public async Task<CustomResponse<FinanceSummaryResponse>> GetSummary(FinanceFilterRequest filter, int userId)
    {
        try
        {
            var entries = await _financeRepository.GetEntriesAsync(userId, filter);

            var summary = new FinanceSummaryResponse
            {
                TotalEarnings = entries.Where(e => e.Type == FinanceEntryType.Earning).Sum(e => e.Amount),
                TotalExpenses = entries.Where(e => e.Type == FinanceEntryType.Expense).Sum(e => e.Amount),
                TotalResgates = entries.Where(e => e.Source == FinanceEntrySource.Resgate).Sum(e => e.Amount),
                TotalManual = entries.Where(e => e.Source == FinanceEntrySource.Manual).Sum(e => e.Amount)
            };
            summary.Balance = summary.TotalEarnings - summary.TotalExpenses;

            return new CustomResponse<FinanceSummaryResponse>(true, new List<string>(), summary);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao calcular resumo financeiro");
            throw;
        }
    }

    public async Task<CustomResponse<FinanceEntry>> RegisterEntry(FinanceEntryRegisterRequest request, int userId)
    {
        try
        {
            var entry = new FinanceEntry
            {
                UserId = userId,
                Type = request.Type,
                Source = FinanceEntrySource.Manual,
                Category = request.Category,
                Description = request.Description,
                Amount = request.Amount,
                Date = request.Date
            };

            var registered = await _financeRepository.RegisterEntryAsync(entry);

            return new CustomResponse<FinanceEntry>(true, new List<string>(), registered);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao registrar movimentação financeira");
            throw;
        }
    }

    public async Task<CustomResponse<FinanceEntry>> UpdateEntry(FinanceEntryUpdateRequest request, int userId)
    {
        try
        {
            var entry = await _financeRepository.GetEntryByIdAsync(request.Id);

            if (entry is null)
            {
                _logger.LogError("Movimentação {EntryId} não encontrada para atualização", request.Id);
                return new CustomResponse<FinanceEntry>(false, new List<string> { "Movimentação não encontrada." }, null);
            }

            if (entry.UserId != userId)
            {
                _logger.LogWarning("Usuário {UserId} tentou atualizar movimentação de outro usuário", userId);
                return new CustomResponse<FinanceEntry>(false, new List<string> { "Movimentação não pertence ao usuário autenticado." }, null);
            }

            var updated = await _financeRepository.UpdateEntryAsync(request);

            if (!updated)
            {
                return new CustomResponse<FinanceEntry>(false, new List<string> { "Erro ao atualizar movimentação." }, null);
            }

            var updatedEntry = await _financeRepository.GetEntryByIdAsync(request.Id);

            return new CustomResponse<FinanceEntry>(true, new List<string>(), updatedEntry);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao atualizar movimentação financeira");
            throw;
        }
    }

    public async Task<CustomResponse<bool>> DeleteEntry(int id, int userId)
    {
        try
        {
            var entry = await _financeRepository.GetEntryByIdAsync(id);

            if (entry is null)
            {
                _logger.LogError("Movimentação {EntryId} não encontrada para exclusão", id);
                return new CustomResponse<bool>(false, new List<string> { "Movimentação não encontrada." }, false);
            }

            if (entry.UserId != userId)
            {
                _logger.LogWarning("Usuário {UserId} tentou excluir movimentação de outro usuário", userId);
                return new CustomResponse<bool>(false, new List<string> { "Movimentação não pertence ao usuário autenticado." }, false);
            }

            var deleted = await _financeRepository.DeleteEntryAsync(id);

            return new CustomResponse<bool>(deleted, new List<string>(), deleted);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao excluir movimentação financeira");
            throw;
        }
    }

    public async Task<CustomResponse<FinanceMonthClosure?>> GetMonthClosure(FinanceMonthPeriodRequest period, int userId)
    {
        try
        {
            var closure = await _financeRepository.GetMonthClosureAsync(userId, period.Month, period.Year);

            return new CustomResponse<FinanceMonthClosure?>(true, new List<string>(), closure);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao buscar fechamento do mês");
            throw;
        }
    }

    public async Task<CustomResponse<FinanceMonthClosure>> CloseMonth(FinanceMonthPeriodRequest period, int userId)
    {
        try
        {
            var existingClosure = await _financeRepository.GetMonthClosureAsync(userId, period.Month, period.Year);

            if (existingClosure is not null)
            {
                _logger.LogWarning("Usuário {UserId} tentou fechar o mês {Month}/{Year} novamente", userId, period.Month, period.Year);
                return new CustomResponse<FinanceMonthClosure>(false, new List<string> { "Este mês já foi fechado." }, null);
            }

            var filter = new FinanceFilterRequest
            {
                StartDate = new DateTime(period.Year, period.Month, 1),
                EndDate = new DateTime(period.Year, period.Month, 1).AddMonths(1).AddTicks(-1)
            };

            var entries = await _financeRepository.GetEntriesAsync(userId, filter);

            var totalEarnings = entries.Where(e => e.Type == FinanceEntryType.Earning).Sum(e => e.Amount);
            var totalExpenses = entries.Where(e => e.Type == FinanceEntryType.Expense).Sum(e => e.Amount);

            var closure = new FinanceMonthClosure
            {
                UserId = userId,
                Month = period.Month,
                Year = period.Year,
                ClosedAt = DateTime.UtcNow,
                TotalEarnings = totalEarnings,
                TotalExpenses = totalExpenses,
                Balance = totalEarnings - totalExpenses
            };

            var registered = await _financeRepository.RegisterMonthClosureAsync(closure);

            return new CustomResponse<FinanceMonthClosure>(true, new List<string>(), registered);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao fechar o mês");
            throw;
        }
    }

    public async Task<CustomResponse<FinanceAiReportResponse>> GenerateAiReport(FinanceMonthPeriodRequest period, int userId)
    {
        try
        {
            var filter = new FinanceFilterRequest
            {
                StartDate = new DateTime(period.Year, period.Month, 1),
                EndDate = new DateTime(period.Year, period.Month, 1).AddMonths(1).AddTicks(-1)
            };

            var entries = await _financeRepository.GetEntriesAsync(userId, filter);

            if (!entries.Any())
            {
                return new CustomResponse<FinanceAiReportResponse>(false, new List<string> { "Nenhuma movimentação encontrada para o período informado." }, null);
            }

            var reportText = BuildReportText(period, entries);

            var analysis = await _aiService.AnalyzeReportAsync(reportText);

            var report = ParseAiReport(analysis);

            return new CustomResponse<FinanceAiReportResponse>(true, new List<string>(), report);
        }
        catch (Exception e) when (e is InvalidOperationException or HttpRequestException)
        {
            _logger.LogError(e, "Erro ao consultar a IA para gerar o relatório financeiro");
            return new CustomResponse<FinanceAiReportResponse>(false, new List<string> { "Não foi possível gerar a análise de IA no momento." }, null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao gerar relatório de IA");
            throw;
        }
    }

    private static string BuildReportText(FinanceMonthPeriodRequest period, List<FinanceEntry> entries)
    {
        var totalEarnings = entries.Where(e => e.Type == FinanceEntryType.Earning).Sum(e => e.Amount);
        var totalExpenses = entries.Where(e => e.Type == FinanceEntryType.Expense).Sum(e => e.Amount);
        var balance = totalEarnings - totalExpenses;

        var earningsByCategory = entries
            .Where(e => e.Type == FinanceEntryType.Earning)
            .GroupBy(e => e.Category)
            .Select(g => $"- {g.Key}: R$ {g.Sum(e => e.Amount):N2}");

        var expensesByCategory = entries
            .Where(e => e.Type == FinanceEntryType.Expense)
            .GroupBy(e => e.Category)
            .Select(g => $"- {g.Key}: R$ {g.Sum(e => e.Amount):N2}");

        return
            $"""
             Período: {period.Month:00}/{period.Year}
             Total de ganhos: R$ {totalEarnings:N2}
             Total de gastos: R$ {totalExpenses:N2}
             Saldo: R$ {balance:N2}
             Quantidade de movimentações: {entries.Count}

             Ganhos por categoria:
             {string.Join('\n', earningsByCategory.DefaultIfEmpty("- nenhum"))}

             Gastos por categoria:
             {string.Join('\n', expensesByCategory.DefaultIfEmpty("- nenhum"))}
             """;
    }

    private static FinanceAiReportResponse ParseAiReport(string analysis)
    {
        const string marker = "RECOMENDAÇÕES:";
        var markerIndex = analysis.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

        if (markerIndex < 0)
        {
            return new FinanceAiReportResponse { Summary = analysis, Recommendations = new List<string>() };
        }

        var summary = analysis[..markerIndex].Trim();
        var recommendationsBlock = analysis[(markerIndex + marker.Length)..];

        var recommendations = recommendationsBlock
            .Split('\n')
            .Select(line => line.Trim().TrimStart('-').Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        return new FinanceAiReportResponse { Summary = summary, Recommendations = recommendations };
    }
}
