namespace App.Queries.Handlers.Directories.Client.Form;

using App.Base.Mediator;
using Base;
using Dapper;
using Exchange.Queries.Directories.Client.Form;
using System.Threading.Tasks;

[RequestRoute("/Directories/Client/Form", RequestRouteAttribute.Types.Query)]
public class Handler : IRequestHandler<Request, Model>
{
    public async Task<Model> HandleAsync(Request request)
    {
        var sql = @$"
            SELECT guid, name, address, condition
            FROM clients
            WHERE guid = @Guid
        ";

        using var connection = Connection.Get();
        var model = await connection.QueryFirstAsync<Model>(sql, new { request.Guid });

        return model;
    }
}
