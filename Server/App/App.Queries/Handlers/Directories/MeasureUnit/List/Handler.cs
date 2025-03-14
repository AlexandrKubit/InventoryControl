﻿namespace App.Queries.Handlers.Directories.MeasureUnit.List;

using App.Base.Mediator;
using Exchange.Queries.Directories.MeasureUnit.List;
using Base;
using System.Threading.Tasks;
using System;
using Dapper;

[RequestRoute("/Directories/MeasureUnit/List", RequestRouteAttribute.Types.Query)]
public class Handler : IRequestHandler<Request, IEnumerable<Model>>
{
    public async Task<IEnumerable<Model>> HandleAsync(Request request, IServiceProvider provider)
    {
        var sql = @$"
            SELECT guid, name
            FROM measure_units
            WHERE condition = @Condition
        ";

        using var connection = Connection.Get();
        var model = await connection.QueryAsync<Model>(sql, new { request.Condition });

        return model;
    }
}
