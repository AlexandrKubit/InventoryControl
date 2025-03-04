namespace Exchange.Queries.Directories.Client.Form;

using App.Base.Mediator;

public class Request : IRequest<Model>
{
    public Guid Guid { get; set; }
}
