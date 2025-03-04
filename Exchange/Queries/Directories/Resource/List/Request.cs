namespace Exchange.Queries.Directories.Resource.List;

using App.Base.Mediator;

public class Request : IRequest<IEnumerable<Model>>
{
    public int Condition { get; set; }
}
