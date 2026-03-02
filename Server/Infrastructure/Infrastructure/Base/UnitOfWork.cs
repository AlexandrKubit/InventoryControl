namespace Infrastructure.Base;

using App.Base.Mediator;
using Domain.Base;
using Domain.Entities.Directories;
using Domain.Entities.Warehouse;
using Infrastructure.Repositories;
using Infrastructure.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;

public sealed class UnitOfWork : IData, IUnitOfWork
{
    internal Context Context { get; set; }

    private IServiceProvider provider;
    private IDbContextTransaction transaction;
    private string connectionString;
    private Dictionary<Type, BaseRepository> repositories;

    public UnitOfWork(IServiceProvider provider, string connectionString)
    {
        this.provider = provider;
        this.connectionString = connectionString;
    }

    // Универсальный метод для получения репозитория
    private T Get<T>() where T : BaseRepository
    {
        var type = typeof(T);
        if (!repositories.TryGetValue(type, out var repository))
        {
            repository = ActivatorUtilities.CreateInstance<T>(provider)
                ?? throw new Exception($"Не удалось создать экземпляр репозитория {type.Name}");

            repositories[type] = repository;
        }

        return (T)repository;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<Context>()
            .UseNpgsql(connectionString)
            .Options;

        Context = new Context(options);
        await Context.Database.OpenConnectionAsync();
        transaction = await Context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        repositories = new();
    }

    public async Task CommitAsync()
    {
        foreach (var item in repositories)
            item.Value.Commit();

        await Context.SaveChangesAsync();
        await transaction.CommitAsync();
        await transaction.DisposeAsync(); // освобождаем транзакцию
        transaction = null;

        // тут можно вызвать обещания инфраструктурных сервисов например отправить email
    }

    public async Task RollbackAsync()
    {
        if (transaction != null)
        {
            await transaction.RollbackAsync();
            await transaction.DisposeAsync();
            transaction = null;
        }
        if (Context != null)
        {
            await Context.DisposeAsync();
            Context = null;
        }
    }

    public bool IsDeadlockException(Exception exception)
    {
        if (exception == null)
            return false;

        if (exception is PostgresException postgresEx && postgresEx.SqlState == "40P01")
            return true;

        if (exception is DbUpdateException dbUpdateEx)
            return IsDeadlockException(dbUpdateEx.InnerException);

        if (exception.InnerException != null)
            return IsDeadlockException(exception.InnerException);

        return false;
    }

    Client.IRepository IData.Client => Get<ClientRepository>();
    MeasureUnit.IRepository IData.MeasureUnit => Get<MeasureUnitRepository>();
    Resource.IRepository IData.Resource => Get<ResourceRepository>();
    Balance.IRepository IData.Balance => Get<BalanceRepository>();
    Domain.Entities.Warehouse.Receipt.Document.IRepository IData.Receipt => Get<ReceiptRepository>();
    Domain.Entities.Warehouse.Receipt.Item.IRepository IData.ReceiptItem => Get<ReceiptItemRepository>();
    Domain.Entities.Warehouse.Shipment.Document.IRepository IData.Shipment => Get<ShipmentRepository>();
    Domain.Entities.Warehouse.Shipment.Item.IRepository IData.ShipmentItem => Get<ShipmentItemRepository>();
}
