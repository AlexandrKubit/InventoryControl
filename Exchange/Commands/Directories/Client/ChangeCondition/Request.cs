namespace Exchange.Commands.Directories.Client.ChangeCondition;

using App.Base.Mediator;

public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
}
