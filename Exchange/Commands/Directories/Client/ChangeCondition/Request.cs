namespace Exchange.Commands.Directories.Client.ChangeCondition;

public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
}
