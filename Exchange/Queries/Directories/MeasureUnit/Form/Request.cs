namespace Exchange.Queries.Directories.MeasureUnit.Form;

using App.Base.Mediator;

public class Request : IRequest<Model>
{
    public Guid Guid { get; set; }
}
