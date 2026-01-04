namespace AnalyticsService.Domain.Entities;

public class TaskMetric
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public decimal AverageCompletionTimeHours { get; set; }
    public decimal CompletionRate { get; set; }
}