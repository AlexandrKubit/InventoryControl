namespace App.Commands.Repositories;

using App.Commands.Base;
using Domain.Entities.Warehouse.Shipment;

public interface IShipmentRepository : Document.IRepository, IRepository
{
    public abstract Task FillByNumbers(List<string> numbers);
    public abstract Task FillByClients(List<Guid> clientGuids);
}
