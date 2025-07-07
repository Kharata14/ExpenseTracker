namespace ExpenseTrackerApi.Common.Entities;

public class Category
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string? Icon { get; set; }
    public string? ColorHex { get; set; }
    public decimal? MonthlyBudget { get; set; }
    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}