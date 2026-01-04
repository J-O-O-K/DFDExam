using MediatR;

namespace TaskService.Application.Features.Tasks.Commands.DeleteTask;

public class DeleteTaskCommand : IRequest<Unit>
{
    public int Id { get; set; }
}
