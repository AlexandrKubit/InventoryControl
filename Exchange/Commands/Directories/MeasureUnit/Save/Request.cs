namespace Exchange.Commands.Directories.MeasureUnit.Save;



public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
    public string Name { get; set; }
}
