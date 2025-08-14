namespace App.Queries.Handlers.Warehouse.Shipment.Form;

using App.Base.Mediator;
using Exchange.Queries.Warehouse.Shipment.Form;
using Base;
using System.Threading.Tasks;
using System;
using Dapper;

[RequestRoute("/Warehouse/Shipment/Form", RequestRouteAttribute.Types.Query)]
public class Handler : IRequestHandler<Request, Model>
{
    public async Task<Model> HandleAsync(Request request, IServiceProvider provider)
    {
        var model = new Model();

        var sql = @$"
            CREATE TEMPORARY TABLE client_guids (guid UUID);
            CREATE TEMPORARY TABLE resource_guids (guid UUID);
            CREATE TEMPORARY TABLE measure_unit_guids (guid UUID);
            
			CREATE TEMPORARY TABLE shipment_condition AS 
			SELECT condition 
			FROM shipments 
			WHERE guid = @Guid;
            
            insert into client_guids
            SELECT client_guid
            FROM shipments
            where guid = @Guid;
            
            insert into resource_guids
            SELECT distinct resource_guid
            FROM shipment_items
            where shipment_guid = @Guid;
            
			INSERT INTO resource_guids
			SELECT DISTINCT resource_guid
			FROM balances
			WHERE quantity > 0
			  AND (SELECT condition FROM shipment_condition) = 1
			  AND resource_guid NOT IN (SELECT guid FROM resource_guids);
            
            insert into measure_unit_guids
            SELECT distinct measure_unit_guid
            FROM shipment_items
            where shipment_guid = @Guid;
            
			INSERT INTO measure_unit_guids
			SELECT DISTINCT measure_unit_guid
			FROM balances
			WHERE quantity > 0
			  AND (SELECT condition FROM shipment_condition) = 1
			  AND measure_unit_guid NOT IN (SELECT guid FROM measure_unit_guids);

            select guid, number, client_guid as ClientGuid, date::date, condition
            from shipments
            where guid = @Guid;

            SELECT guid, resource_guid as ResourceGuid, measure_unit_guid as MeasureUnitGuid, quantity
            FROM shipment_items
            where shipment_guid = @Guid;

            SELECT resource_guid as ResourceGuid, measure_unit_guid as MeasureUnitGuid, quantity
            FROM balances
            where quantity > 0;

            select guid, name 
            from clients
            where condition = 1
            or guid = ANY(select guid from client_guids);

            select guid, name 
            from resources
            where guid = ANY(select guid from resource_guids);

            select guid, name 
            from measure_units
            where guid = ANY(select guid from measure_unit_guids);

            drop table client_guids;
            drop table resource_guids;
            drop table measure_unit_guids;
			DROP TABLE shipment_condition;
        ";

        using var connection = Connection.Get();
        using var multi = await connection.QueryMultipleAsync(sql, new { request.Guid });

        model.Document = multi.ReadFirstOrDefault<Model.Shipment>();
        model.Items = multi.Read<Model.Item>().ToList();
        model.Balances = multi.Read<Model.Balance>().ToList();
        model.Clients = multi.Read<Model.Select>().ToList();
        model.Resources = multi.Read<Model.Select>().ToList();
        model.MeasureUnits = multi.Read<Model.Select>().ToList();

        return model;
    }
}
