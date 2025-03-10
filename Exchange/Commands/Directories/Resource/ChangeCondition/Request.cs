namespace Exchange.Commands.Directories.Resource.ChangeCondition;



public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
}
