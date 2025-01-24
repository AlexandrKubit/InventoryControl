namespace Exchange.Commands.Directories.MeasureUnit.ChangeCondition;

using App.Base.Mediator;

public class Request : IRequest<Guid>
{
    public Guid Guid { get; set; }
}
