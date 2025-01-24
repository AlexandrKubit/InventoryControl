namespace Exchange.Commands.Directories.Resource.Delete;

using App.Base.Mediator;

public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
}
