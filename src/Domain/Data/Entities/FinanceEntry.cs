using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_RouteXFlow.Domain.Data.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FinanceEntryType
{
    [JsonStringEnumMemberName("earning")]
    Earning,

    [JsonStringEnumMemberName("expense")]
    Expense
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FinanceEntrySource
{
    [JsonStringEnumMemberName("manual")]
    Manual,

    [JsonStringEnumMemberName("resgate")]
    Resgate
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FinanceCategory
{
    [JsonStringEnumMemberName("resgate_delivery")]
    ResgateDelivery,

    [JsonStringEnumMemberName("resgate_entregas_marketplace")]
    ResgateEntregasMarketplace,

    [JsonStringEnumMemberName("salario_clt")]
    SalarioClt,

    [JsonStringEnumMemberName("freela")]
    Freela,

    [JsonStringEnumMemberName("outro_ganho")]
    OutroGanho,

    [JsonStringEnumMemberName("combustivel")]
    Combustivel,

    [JsonStringEnumMemberName("manutencao")]
    Manutencao,

    [JsonStringEnumMemberName("alimentacao")]
    Alimentacao,

    [JsonStringEnumMemberName("aluguel")]
    Aluguel,

    [JsonStringEnumMemberName("imposto")]
    Imposto,

    [JsonStringEnumMemberName("outro_gasto")]
    OutroGasto
}

[Table("finance_entry")]
public class FinanceEntry : BaseEntity
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("type")]
    public FinanceEntryType Type { get; set; }

    [Column("source")]
    public FinanceEntrySource Source { get; set; }

    [Column("category")]
    public FinanceCategory Category { get; set; }

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
