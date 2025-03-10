namespace Exchange.Commands.Directories.Resource.Save;



public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
    public string Name { get; set; }
}
