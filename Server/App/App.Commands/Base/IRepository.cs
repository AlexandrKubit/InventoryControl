namespace App.Commands.Base;

public interface IRepository
{
    public Task FillByGuids(List<Guid> guids);
}
