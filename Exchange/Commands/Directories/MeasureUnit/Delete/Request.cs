namespace Exchange.Commands.Directories.MeasureUnit.Delete;



public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
}
