namespace Exchange.Queries.Directories.Client.List;

using App.Base.Mediator;

public class Request : IRequest<IEnumerable<Model>>
{
    public int Condition { get; set; }
}
