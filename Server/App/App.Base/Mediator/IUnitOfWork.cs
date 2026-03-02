namespace App.Base.Mediator;

public interface IUnitOfWork
{
    public Task InitializeAsync();
    public Task CommitAsync();
    public Task RollbackAsync();
    public bool IsDeadlockException(Exception exception);
}
