namespace App.Commands.Repositories;

using App.Commands.Base;
using Domain.Entities.Directories;

public interface IClientRepository : Client.IRepository, IRepository
{
    public Task FillByNames(List<string> names);
}
