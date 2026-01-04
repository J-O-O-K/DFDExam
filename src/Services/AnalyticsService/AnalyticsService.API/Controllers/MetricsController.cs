using AnalyticsService.Application.DTOs;
using AnalyticsService.Application.Features.Metrics.Commands.AggregateMetrics;
using AnalyticsService.Application.Features.Metrics.Commands.RefreshMetrics;
using AnalyticsService.Application.Features.Metrics.Queries.GetDashboardMetrics;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AnalyticsService.API.Controllers;

[ApiController]
[Route("api/analytics/metrics")]
[Produces("application/json")]
public class MetricsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(IMediator mediator, ILogger<MetricsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(IEnumerable<DashboardMetricsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DashboardMetricsDto>>> GetDashboardMetrics()
    {
        _logger.LogInformation("Getting dashboard metrics from materialized view");
        var metrics = await _mediator.Send(new GetDashboardMetricsQuery());
        return Ok(metrics);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RefreshMetrics()
    {
        _logger.LogInformation("Manually triggering materialized view refresh");
        await _mediator.Send(new RefreshMetricsCommand());
        return Ok(new { Message = "Materialized view refreshed successfully" });
    }

    [HttpPost("aggregate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> AggregateMetrics()
    {
        _logger.LogInformation("Manually triggering metrics aggregation");
        
        // Aggregate metrics for today
        await _mediator.Send(new AggregateMetricsCommand { ForDate = DateTime.UtcNow.Date });
        
        // Refresh  materialized view
        await _mediator.Send(new RefreshMetricsCommand());
        
        return Ok(new { Message = "Metrics aggregated and materialized view refreshed successfully" });
    }
}