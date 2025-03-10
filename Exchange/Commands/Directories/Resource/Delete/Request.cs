namespace Exchange.Commands.Directories.Resource.Delete;



public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
}
