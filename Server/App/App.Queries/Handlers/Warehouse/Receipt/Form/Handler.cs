namespace App.Queries.Handlers.Warehouse.Receipt.Form;

using App.Base.Mediator;
using Exchange.Queries.Warehouse.Receipt.Form;
using Base;
using System.Threading.Tasks;
using System;
using Dapper;

[RequestRoute("/Warehouse/Receipt/Form", RequestRouteAttribute.Types.Query)]
public class Handler : IRequestHandler<Request, Model>
{
    public async Task<Model> HandleAsync(Request request, IServiceProvider provider)
    {
        var model = new Model();

        var sql = @$"
            CREATE TEMPORARY TABLE resource_guids (guid UUID);
            CREATE TEMPORARY TABLE measure_unit_guids (guid UUID);

            insert into resource_guids
            SELECT distinct resource_guid
            FROM receipt_items
            where receipt_guid = @Guid;

            insert into measure_unit_guids
            SELECT distinct measure_unit_guid
            FROM receipt_items
            where receipt_guid = @Guid;

            select guid, number, date::date
            from receipts
            where guid = @Guid;

            SELECT guid, resource_guid as ResourceGuid, measure_unit_guid as MeasureUnitGuid, quantity
            FROM receipt_items
            where receipt_guid = @Guid;

            select guid, name 
            from resources
            where condition = 1
            or guid = ANY(select guid from resource_guids);

            select guid, name 
            from measure_units
            where condition = 1
            or guid = ANY(select guid from measure_unit_guids);

            drop table resource_guids;
            drop table measure_unit_guids;
        ";

        using var connection = Connection.Get();
        using var multi = await connection.QueryMultipleAsync(sql, new { request.Guid });

        model.Document = multi.ReadFirstOrDefault<Model.Receipt>();
        model.Items = multi.Read<Model.Item>().ToList();
        model.Resources = multi.Read<Model.Select>().ToList();
        model.MeasureUnits = multi.Read<Model.Select>().ToList();

        return model;
    }
}
