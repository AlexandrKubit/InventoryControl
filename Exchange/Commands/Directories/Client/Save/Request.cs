namespace Exchange.Commands.Directories.Client.Save;



public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
}
