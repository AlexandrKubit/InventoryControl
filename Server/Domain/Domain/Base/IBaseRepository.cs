﻿namespace Domain.Base;

public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    // Репозиторий - это прежде всего коллекция.
    // Не способ доступа к данным, не абстракция над конкретной ORM или БД, не кэширование - это все детали реализации.
    // Очень грубо, репозиторий можно представить, как "личную" копию данных из конкретной таблицы БД, представленную в виде доменных объектов.
    // В ходе сценария эта коллекция наполняется данными, эти данные как то изменяются, а в конце сценария нужно обновить данные в самой БД 

    public IEnumerable<TEntity> List { get; }
    public void AddEntity(TEntity entity);
}
