namespace Infrastructure.Base;

using App.Commands.Base;
using App.Commands.Repositories;
using Domain.Base;
using Domain.Entities.Directories;
using Domain.Entities.Warehouse;
using Infrastructure.Services.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

public sealed class UnitOfWork : IUnitOfWork
{
    public UnitOfWork(IServiceProvider provider)
    {
        this.provider = provider;
    }

    private IServiceProvider provider;
    private readonly Dictionary<Type, object> repositories = new();

    // Универсальный метод для получения репозитория
    private T Get<T>()
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

    public async Task Commit()
    {
        foreach (var item in repositories)
        {
            var baseType = item.Key.BaseType;
            if (baseType == null)
                continue;

            bool isBaseRepository = baseType.IsGenericType
               && baseType.GetGenericTypeDefinition() == typeof(BaseRepository<>);

            if (isBaseRepository)
            {
                var dynMethod = item.Key.GetMethod("Commit", BindingFlags.NonPublic | BindingFlags.Instance);
                await (Task)dynMethod.Invoke(item.Value, null);
            }
        }
    }


    IClientRepository IUnitOfWork.Client => Get<ClientRepository>();
    Client.IRepository IData.Client => Get<ClientRepository>();

    public IMeasureUnitRepository MeasureUnit => Get<MeasureUnitRepository>();
    MeasureUnit.IRepository IData.MeasureUnit => Get<MeasureUnitRepository>();

    public IResourceRepository Resource => Get<ResourceRepository>();
    Resource.IRepository IData.Resource => Get<ResourceRepository>();

    public IBalanceRepository Balance => Get<BalanceRepository>();
    Balance.IRepository IData.Balance => Get<BalanceRepository>();

    public IReceiptRepository Receipt => Get<ReceiptRepository>();
    Domain.Entities.Warehouse.Receipt.Document.IRepository IData.Receipt => Get<ReceiptRepository>();

    public IReceiptItemRepository ReceiptItem => Get<ReceiptItemRepository>();
    Domain.Entities.Warehouse.Receipt.Item.IRepository IData.ReceiptItem => Get<ReceiptItemRepository>();

    public IShipmentRepository Shipment => Get<ShipmentRepository>();
    Domain.Entities.Warehouse.Shipment.Document.IRepository IData.Shipment => Get<ShipmentRepository>();

    public IShipmentItemRepository ShipmentItem => Get<ShipmentItemRepository>();
    Domain.Entities.Warehouse.Shipment.Item.IRepository IData.ShipmentItem => Get<ShipmentItemRepository>();
}
