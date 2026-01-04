using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskService.Application.DTOs;
using TaskService.Application.Features.Tasks.Commands.AssignTask;
using TaskService.Application.Features.Tasks.Commands.CompleteTask;
using TaskService.Application.Features.Tasks.Commands.CreateTask;
using TaskService.Application.Features.Tasks.Commands.DeleteTask;
using TaskService.Application.Features.Tasks.Commands.UpdateTask;
using TaskService.Application.Features.Tasks.Queries.GetAllTasks;
using TaskService.Application.Features.Tasks.Queries.GetHighPriorityTasks;
using TaskService.Application.Features.Tasks.Queries.GetOverdueTasks;
using TaskService.Application.Features.Tasks.Queries.GetTaskById;

namespace TaskService.API.Controllers;

[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks()
    {
        _logger.LogInformation("Getting all tasks");
        var tasks = await _mediator.Send(new GetAllTasksQuery());
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> GetTaskById(int id)
    {
        _logger.LogInformation("Getting task with ID: {TaskId}", id);
        var task = await _mediator.Send(new GetTaskByIdQuery { Id = id });

        if (task == null)
        {
            _logger.LogWarning("Task with ID {TaskId} not found", id);
            return NotFound(new { Message = $"Task with ID {id} not found" });
        }

        return Ok(task);
    }

    [HttpGet("high-priority")]
    [ProducesResponseType(typeof(IEnumerable<TaskWithPriorityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskWithPriorityDto>>> GetHighPriorityTasks()
    {
        _logger.LogInformation("Getting high priority tasks via stored procedure");
        var tasks = await _mediator.Send(new GetHighPriorityTasksQuery());
        return Ok(tasks);
    }

    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IEnumerable<TaskOverdueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskOverdueDto>>> GetOverdueTasks()
    {
        _logger.LogInformation("Getting overdue tasks via stored procedure");
        var tasks = await _mediator.Send(new GetOverdueTasksQuery());
        return Ok(tasks);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto createTaskDto)
    {
        _logger.LogInformation("Creating new task: {Title}", createTaskDto.Title);

        var command = new CreateTaskCommand
        {
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            Priority = createTaskDto.Priority,
            DueDate = createTaskDto.DueDate
        };

        var createdTask = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
    {
        _logger.LogInformation("Updating task with ID: {TaskId}", id);

        try
        {
            var command = new UpdateTaskCommand
            {
                Id = id,
                Title = updateTaskDto.Title,
                Description = updateTaskDto.Description,
                Status = updateTaskDto.Status,
                Priority = updateTaskDto.Priority,
                DueDate = updateTaskDto.DueDate
            };

            var updatedTask = await _mediator.Send(command);
            return Ok(updatedTask);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Task with ID {TaskId} not found", id);
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTask(int id)
    {
        _logger.LogInformation("Deleting task with ID: {TaskId}", id);

        try
        {
            await _mediator.Send(new DeleteTaskCommand { Id = id });
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Task with ID {TaskId} not found", id);
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost("{id}/assign")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> AssignTask(int id, [FromBody] AssignTaskDto assignTaskDto)
    {
        _logger.LogInformation("Assigning task {TaskId} to {AssignedTo}", id, assignTaskDto.AssignedTo);

        try
        {
            var command = new AssignTaskCommand
            {
                Id = id,
                AssignedTo = assignTaskDto.AssignedTo
            };

            var updatedTask = await _mediator.Send(command);
            return Ok(updatedTask);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Task with ID {TaskId} not found", id);
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> CompleteTask(int id)
    {
        _logger.LogInformation("Marking task {TaskId} as completed", id);

        try
        {
            var command = new CompleteTaskCommand { Id = id };
            var updatedTask = await _mediator.Send(command);
            return Ok(updatedTask);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Task with ID {TaskId} not found", id);
            return NotFound(new { Message = ex.Message });
        }
    }
}
