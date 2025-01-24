namespace App.Commands.Repositories;

using App.Commands.Base;
using Domain.Entities.Warehouse;

public interface IBalanceRepository : Balance.IRepository, IRepository
{
    public Task FillByResourceMeasureUnit(IEnumerable<(Guid ResourceGuid, Guid MeasureUnitGuid)> args);
    public Task FillByMeasureUnitGuids(List<Guid> unitGuids);
    public Task FillByResourceGuids(List<Guid> resourceGuids);
}
