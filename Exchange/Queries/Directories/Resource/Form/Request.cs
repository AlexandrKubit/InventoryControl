namespace Exchange.Queries.Directories.Resource.Form;

using App.Base.Mediator;

public class Request : IRequest<Model>
{
    public Guid Guid { get; set; }
}
