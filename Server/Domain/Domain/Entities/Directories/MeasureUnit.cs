namespace Domain.Entities.Directories;

using Common.Exceptions;
using Domain.Base;

/// <summary>
/// Единица измерения
/// </summary>
public sealed class MeasureUnit : BaseEntity
{
    public interface IRepository : IBaseRepository<MeasureUnit>
    {
        protected static MeasureUnit Restore(Guid id, string name, Conditions condition)
            => new MeasureUnit(id, name, condition);
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

    public static List<MeasureUnit> CreateRange(List<string> names, IData data)
    {
        if (data.MeasureUnit.List.Any(x => names.Contains(x.Name)))
            throw new DomainException("В системе уже зарегистрирована единица измерения с таким наименованием");

        List<MeasureUnit> units = new List<MeasureUnit>();

        foreach (var name in names)
        {
            var unit = new MeasureUnit(Guid.CreateVersion7(), name, Conditions.Work);
            unit.Append(data.MeasureUnit);
            units.Add(unit);
        }

        return units;
    }

    public record UpdateArg(MeasureUnit Unit)
    {
        public string Name = Unit.Name;
    };
    public static void UpdateRange(List<UpdateArg> args, IData data)
    {
        foreach (var arg in args)
        {
            arg.Unit.Name = arg.Name;
            arg.Unit.Update();
        }

        foreach (var arg in args)
        {
            if (data.MeasureUnit.List.Any(x => x.Name == arg.Name && x.Guid != arg.Unit.Guid))
                throw new DomainException("В системе уже зарегистрирована единица измерения с таким наименованием");
        }
    }

    public static void DeleteRange(List<MeasureUnit> units, IData data)
    {
        var unitGuids = units.Select(x => x.Guid).ToList();

        var receiptItems = data.ReceiptItem.List.Where(x => unitGuids.Contains(x.MeasureUnitGuid));
        var balances = data.Balance.List.Where(x => unitGuids.Contains(x.MeasureUnitGuid));
        var shipmentItems = data.ShipmentItem.List.Where(x => unitGuids.Contains(x.MeasureUnitGuid));

        if (receiptItems.Any() || balances.Any() || shipmentItems.Any())
            throw new Exception("Невозможно удалить единицу измерения, так как она используется");

        foreach (var unit in units)
            unit.Remove();
    }

    public void ToArchive()
    {
        if (Condition == Conditions.Work)
        {
            Condition = Conditions.Archive;
            Update();
        }
        else
            throw new DomainException("Невозможно перевести в архив, т.к. клиент уже находится в архиве");
    }

    public void ToWork()
    {
        if (Condition == Conditions.Archive)
        {
            Condition = Conditions.Work;
            Update();
        }
        else
            throw new DomainException("Невозможно перевести в работу, т.к. клиент уже находится в работе");
    }
}
