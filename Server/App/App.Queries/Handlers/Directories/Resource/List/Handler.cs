namespace App.Queries.Handlers.Directories.Resource.List;

using App.Base.Mediator;
using Exchange.Queries.Directories.Resource.List;
using Base;
using System.Threading.Tasks;
using System;
using Dapper;

[RequestRoute("/Directories/Resource/List", RequestRouteAttribute.Types.Query)]
public class Handler : IRequestHandler<Request, IEnumerable<Model>>
{
    public async Task<IEnumerable<Model>> HandleAsync(Request request, IServiceProvider provider)
    {
        var sql = @$"
            SELECT guid, name
            FROM resources
            WHERE condition = {request.Condition}
        ";

        using var connection = Connection.Get();
        var model = await connection.QueryAsync<Model>(sql);

        return model;
    }
}
