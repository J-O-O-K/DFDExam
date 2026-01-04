using AnalyticsService.Application.Contracts.Persistence;
using AnalyticsService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnalyticsService.Application.Features.Metrics.Commands.AggregateMetrics;

public class AggregateMetricsCommandHandler : IRequestHandler<AggregateMetricsCommand, bool>
{
    private readonly ITaskEventRepository _eventRepository;
    private readonly IMetricsRepository _metricsRepository;
    private readonly ILogger<AggregateMetricsCommandHandler> _logger;

    public AggregateMetricsCommandHandler(
        ITaskEventRepository eventRepository,
        IMetricsRepository metricsRepository,
        ILogger<AggregateMetricsCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _metricsRepository = metricsRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(AggregateMetricsCommand request, CancellationToken cancellationToken)
    {
        var targetDate = request.ForDate ?? DateTime.UtcNow.Date;
        
        _logger.LogInformation("Aggregating metrics for date: {Date}", targetDate);

        var endDate = targetDate.AddDays(1);
        var allEvents = await _eventRepository.GetEventsByDateRangeAsync(DateTime.MinValue, endDate);

        if (!allEvents.Any())
        {
            _logger.LogInformation("No events found up to date: {Date}", targetDate);
            
            var emptyMetric = new TaskMetric
            {
                Date = targetDate,
                TotalTasks = 0,
                CompletedTasks = 0,
                OverdueTasks = 0,
                AverageCompletionTimeHours = 0m,
                CompletionRate = 0m
            };
            
            await _metricsRepository.UpsertMetricAsync(emptyMetric);
            return true;
        }

        var taskStates = new Dictionary<int, TaskState>();

        foreach (var evt in allEvents.OrderBy(e => e.Timestamp))
        {
            if (!taskStates.ContainsKey(evt.TaskId))
            {
                taskStates[evt.TaskId] = new TaskState
                {
                    TaskId = evt.TaskId,
                    CreatedAt = evt.Timestamp,
                    Status = "Pending"
                };
            }

            var state = taskStates[evt.TaskId];

            switch (evt.EventType)
            {
                case "TaskCreated":
                    state.Status = "Pending";
                    state.CreatedAt = evt.Timestamp;
                    break;

                case "TaskUpdated":
                    if (evt.Metadata != null && evt.Metadata.Contains("status"))
                    {
                        var status = evt.Metadata["status"].AsString;
                        if (!string.IsNullOrEmpty(status))
                        {
                            state.Status = status;
                            
                            if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                            {
                                state.CompletedAt ??= evt.Timestamp;
                            }
                        }
                    }
                    break;

                case "TaskCompleted":
                    state.Status = "Completed";
                    state.CompletedAt = evt.Timestamp;
                    break;

                case "TaskDeleted":
                    state.IsDeleted = true;
                    break;
            }
        }

        var activeTasks = taskStates.Values.Where(t => !t.IsDeleted).ToList();
        
        _logger.LogInformation("Found {Count} active tasks (non-deleted)", activeTasks.Count);

        var totalTasks = activeTasks.Count;
        var completedTasks = activeTasks.Count(t => 
            t.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));

        _logger.LogInformation("Total: {Total}, Completed: {Completed}", totalTasks, completedTasks);

        var overdueTasks = 0;

        var completionTimes = activeTasks
            .Where(t => t.CompletedAt.HasValue)
            .Select(t => (t.CompletedAt!.Value - t.CreatedAt).TotalHours)
            .ToList();

        var avgCompletionTime = completionTimes.Any()
            ? (decimal)completionTimes.Average()
            : 0m;

        var completionRate = totalTasks > 0
            ? Math.Round((decimal)completedTasks / totalTasks * 100, 2)
            : 0m;

        var metric = new TaskMetric
        {
            Date = targetDate,
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks,
            OverdueTasks = overdueTasks,
            AverageCompletionTimeHours = avgCompletionTime,
            CompletionRate = completionRate
        };

        await _metricsRepository.UpsertMetricAsync(metric);

        _logger.LogInformation(
            "Metrics aggregated successfully for {Date}: Total={Total}, Completed={Completed}, Rate={Rate}%",
            targetDate, totalTasks, completedTasks, completionRate);

        return true;
    }

    private class TaskState
    {
        public int TaskId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}