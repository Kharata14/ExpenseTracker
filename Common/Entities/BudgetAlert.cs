namespace ExpenseTrackerApi.Common.Entities;

public class BudgetAlert
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime Month { get; set; }
    public decimal PercentageUsed { get; set; }
    public DateTime AlertSentAt { get; set; }
}