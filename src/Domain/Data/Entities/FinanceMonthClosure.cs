using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("finance_month_closure")]
public class FinanceMonthClosure : BaseEntity
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("month")]
    public int Month { get; set; }

    [Column("year")]
    public int Year { get; set; }

    [Column("closed_at")]
    public DateTime ClosedAt { get; set; }

    [Column("total_earnings")]
    public decimal TotalEarnings { get; set; }

    [Column("total_expenses")]
    public decimal TotalExpenses { get; set; }

    [Column("balance")]
    public decimal Balance { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
