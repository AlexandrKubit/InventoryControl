namespace Exchange.Commands.Directories.MeasureUnit.ChangeCondition;



public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
}
