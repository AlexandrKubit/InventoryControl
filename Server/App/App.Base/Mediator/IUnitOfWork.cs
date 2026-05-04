namespace App.Base.Mediator;

public interface IUnitOfWork
{
    public Task InitializeAsync(System.Data.IsolationLevel isolationLevel);
    public Task CommitAsync();
    public Task RollbackAsync();
    public bool IsTransientConcurrencyException(Exception exception);

    // для получения эксклюзивной рекомендательной блокировки
    // действует до конца текущей транзакции
    public Task AcquireLock(Type entityType, string key);
}
