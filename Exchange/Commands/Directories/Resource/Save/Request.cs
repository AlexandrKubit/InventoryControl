namespace Exchange.Commands.Directories.Resource.Save;

using App.Base.Mediator;

public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
    public string Name { get; set; }
}
