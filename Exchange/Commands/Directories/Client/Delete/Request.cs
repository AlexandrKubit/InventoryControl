namespace Exchange.Commands.Directories.Client.Delete;



public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
}
