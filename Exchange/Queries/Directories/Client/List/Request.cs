namespace Exchange.Queries.Directories.Client.List;



public class Request : IRequest<IEnumerable<Model>>
{
    public int Condition { get; set; }
}
