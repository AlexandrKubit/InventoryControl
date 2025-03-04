namespace App.Queries.Handlers.Directories.Resource.Form;

using App.Base.Mediator;
using Exchange.Queries.Directories.Resource.Form;
using Base;
using System.Threading.Tasks;
using System;
using Dapper;

[RequestRoute("/Directories/Resource/Form", RequestRouteAttribute.Types.Query)]
public class Handler : IRequestHandler<Request, Model>
{
    public async Task<Model> HandleAsync(Request request, IServiceProvider provider)
    {
        var sql = @$"
            SELECT guid, name, condition
            FROM resources
            WHERE guid = '{request.Guid}'
        ";

        using var connection = Connection.Get();
        var model = await connection.QueryFirstAsync<Model>(sql);

        return model;
    }
}
