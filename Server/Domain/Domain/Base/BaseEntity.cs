namespace Domain.Base;

/// <summary>
/// Базовый класс для всех доменных сущностей, которые работают с репозиториями BaseRepository<T> where T : BaseEntity 
/// </summary>
public class BaseEntity
{
    /// <summary>
    /// Идентификатор, позволяющий создать идентичность без необходимости обращаться к БД
    /// </summary>
    public Guid Guid { get; protected init; }

    /// <summary>
    /// Состояние, в котором находится сущность ("не изменен","создан", "изменен", "удален")
    /// </summary>
    public ModificationTypes ModificationType { get; private set; } = ModificationTypes.None;

    public enum ModificationTypes
    {
        /// <summary>
        /// Сущность восстановленная из репозитория изначально имеет состояние "не изменен"
        /// </summary>
        None = 0,

        /// <summary>
        /// После того как мы создали новую сущность через ПУБЛИЧНЫЙ конструктор или статический метод, мы обязаны пометить сущность как созданная. 
        /// Это не относится к действию восстановления сущности из БД в коллекцию репозитория, так как там используется приватный конструктор.
        /// </summary>
        Created = 1,

        /// <summary>
        /// После любого изменения свойств сущности, мы должны пометить ее как измененная 
        /// </summary>
        Updated = 2,

        /// <summary>
        /// Если мы хотим, чтобы в конце сценария сущность была удалена из БД, 
        /// нам необходимо пометить эту сущность как удаленная
        /// </summary>
        Removed = 3
    }

    /// <summary>
    /// Добавляет сущность в коллекцию репозитория и помечает ее состояние как "созданная".
    /// Метод должен быть protected, у сущности должен быть бизнесовый метод создания, 
    /// например публичный конструктор или статический метод с дополнительными проверками
    /// и в этом методе после создания сущности она обязана быть добавлена в репозиторий
    /// </summary>
    protected void Append<TEntity>(IBaseRepository<TEntity> repository) where TEntity : BaseEntity
    {
        if (ModificationType == ModificationTypes.None)
        {
            ModificationType = ModificationTypes.Created;
            repository.AddEntity((TEntity)this);
        }
        else
            throw new Exception("Неправильное использование метода Append");
    }

    /// <summary>
    /// Метод который помечает сущность как "удаленная".
    /// Тоже должен быть protected т.к. если бы он был публичный,
    /// то в любом месте сценария мы могли бы вызвать его, обойдя все бизнесправила.
    /// У сущности скорее всего должен быть собственный метод Delete, 
    /// в котором проходт все проверки а уже потом вызывается метод Remove()
    /// </summary>
    protected void Remove()
    {
        ModificationType = ModificationTypes.Removed;
    }

    /// <summary>
    /// Метод помечает сущность как "измененная"
    /// Он мог бы быть публичным, т.к. ничего бы особо не сломал, 
    /// лишь увеличил бы нагрузку на БД, если его не правильно использовать.
    /// Но по смысловой нагрузке он должен вызываться внутри методов сущности,
    /// которые изменяют состояние этой сущности
    /// </summary>
    protected void Update()
    {
        if (ModificationType == ModificationTypes.None || ModificationType == ModificationTypes.Updated)
            ModificationType = ModificationTypes.Updated;
        else
            throw new Exception("Неправильное использование метода Update");
    }
}
