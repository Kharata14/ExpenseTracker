namespace ExpenseTrackerApi.Common.Entities;

public class ReportJob
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ReportType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}