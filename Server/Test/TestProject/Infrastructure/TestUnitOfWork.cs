using Domain.Base;
using Domain.Entities.Directories;
using Domain.Entities.Warehouse;
using System.Reflection;
using TestProject.Infrastructure;

namespace Tests.Infrastructure
{
    internal class TestUnitOfWork : IData
    {
        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        public TestUnitOfWork()
        {
            // Автоматически находим и создаем все тестовые репозитории через рефлексию
            var testRepositoryTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(TestBaseRepository<>))
                .ToList();

            foreach (var repoType in testRepositoryTypes)
            {
                var repositoryInstance = Activator.CreateInstance(repoType) 
                    ?? throw new Exception($"Не удалось найти репозиторий для типа {repoType}");
                repositories[repoType] = repositoryInstance;
            }
        }

        private T Get<T>() where T : class
        {
            var type = typeof(T);
            if (repositories.TryGetValue(type, out var repository))
            {
                return (T)repository;
            }
            throw new Exception($"Не удалось найти репозиторий для типа {type.Name}");
        }

        public void LoadData()
        {
            foreach (var repo in repositories.Values)
            {
                var initDataMethod = repo.GetType().GetMethod("InitData");
                initDataMethod?.Invoke(repo, null);
            }
        }

        // Явная реализация интерфейса IData
        public Client.IRepository Client => Get<TestClientRepository>();
        public MeasureUnit.IRepository MeasureUnit { get; }
        public Resource.IRepository Resource { get; }
        public Balance.IRepository Balance { get; }
        public Domain.Entities.Warehouse.Receipt.Document.IRepository Receipt { get; }
        public Domain.Entities.Warehouse.Receipt.Item.IRepository ReceiptItem { get; }
        public Domain.Entities.Warehouse.Shipment.Document.IRepository Shipment { get; }
        public Domain.Entities.Warehouse.Shipment.Item.IRepository ShipmentItem { get; }
    }
}