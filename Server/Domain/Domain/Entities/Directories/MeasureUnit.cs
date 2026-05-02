namespace Domain.Entities.Directories;

using Common.Exceptions;
using Domain.Base;
using System;
using System.Threading.Tasks;

/// <summary>
/// Единица измерения
/// </summary>
public sealed class MeasureUnit : BaseEntity
{
    public interface IRepository : IBaseRepository<MeasureUnit>
    {
        protected static MeasureUnit Restore(Guid id, string name, Conditions condition)
            => new MeasureUnit(id, name, condition);

        public Task EnsureByNames(HashSet<string> names);
    }


    public string Name { get; private set; }
    public Conditions Condition { get; private set; }

    public enum Conditions
    {
        Work = 1,
        Archive = 2
    }

    private MeasureUnit(Guid guid, string name, Conditions condition)
    {
        Guid = guid;
        Name = name;
        Condition = condition;
    }

    public static async Task<List<MeasureUnit>> CreateRange(HashSet<string> names, IData data)
    {
        await data.MeasureUnits.EnsureByNames(names);

        if (data.MeasureUnits.List.Any(x => names.Contains(x.Name)))
            throw new DomainException("В системе уже зарегистрирована единица измерения с таким наименованием");

        List<MeasureUnit> units = new List<MeasureUnit>();

        foreach (var name in names)
        {
            var unit = new MeasureUnit(Guid.CreateVersion7(), name, Conditions.Work);
            unit.Create();
            data.MeasureUnits.Add(unit);
			units.Add(unit);
        }

        return units;
    }

    public record UpdateArg(Guid Guid, string Name);
    public static async Task UpdateRange(List<UpdateArg> args, IData data)
    {
        var guids = args.Select(x => x.Guid).ToHashSet();
        await data.MeasureUnits.EnsureByGuids(guids);

        var names = args.Select(x => x.Name).ToHashSet();
        await data.MeasureUnits.EnsureByNames(names);

        var units = data.MeasureUnits.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var unit in units)
        {
            var arg = args.First(x => x.Guid == unit.Guid);

            unit.Name = arg.Name;
            unit.Update();
        }

        foreach (var arg in args)
        {
            if (data.MeasureUnits.List.Any(x => x.Name == arg.Name && x.Guid != arg.Guid))
                throw new DomainException("В системе уже зарегистрирована единица измерения с таким наименованием");
        }
    }

    public static async Task DeleteRange(HashSet<Guid> guids, IData data)
    {
        await data.ReceiptItems.EnsureByMeasureUnitGuids(guids);
        await data.Balances.EnsureByMeasureUnitGuids(guids);
        await data.ShipmentItems.EnsureByMeasureUnitGuids(guids);

        var receiptItems = data.ReceiptItems.List.Where(x => guids.Contains(x.MeasureUnitGuid));
        var balances = data.Balances.List.Where(x => guids.Contains(x.MeasureUnitGuid));
        var shipmentItems = data.ShipmentItems.List.Where(x => guids.Contains(x.MeasureUnitGuid));

        if (receiptItems.Any() || balances.Any() || shipmentItems.Any())
            throw new DomainException("Невозможно удалить единицу измерения, так как она используется");

        await data.MeasureUnits.EnsureByGuids(guids);
        var units = data.MeasureUnits.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var unit in units)
            unit.Remove();
    }

    public static async Task ToArchiveRange(HashSet<Guid> guids, IData data)
    {
        await data.MeasureUnits.EnsureByGuids(guids);
        var units = data.MeasureUnits.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var unit in units)
        {
            if (unit.Condition == Conditions.Work)
            {
                unit.Condition = Conditions.Archive;
                unit.Update();
            }
            else
                throw new DomainException("Невозможно перевести в архив, т.к. единица измерения уже находится в архиве");
        }
    }

    public static async Task ToWorkRange(HashSet<Guid> guids, IData data)
    {
        await data.MeasureUnits.EnsureByGuids(guids);
        var units = data.MeasureUnits.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var unit in units)
        {
            if (unit.Condition == Conditions.Archive)
            {
                unit.Condition = Conditions.Work;
                unit.Update();
            }
            else
                throw new DomainException("Невозможно перевести в работу, т.к. единица измерения уже находится в работе");
        }
    }
}
