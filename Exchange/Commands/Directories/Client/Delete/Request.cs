namespace Exchange.Commands.Directories.Client.Delete;

using App.Base.Mediator;

public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
}
