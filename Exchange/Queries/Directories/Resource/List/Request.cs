namespace Exchange.Queries.Directories.Resource.List;



public class Request : IRequest<IEnumerable<Model>>
{
    public int Condition { get; set; }
}
