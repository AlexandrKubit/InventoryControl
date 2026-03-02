namespace Domain.Base;

public class DomainEvent<TArg>
{
    /// <summary>
    /// если порядок не важен (или один подписчик)
    /// </summary>
    public DomainEvent()
    {
        order = null;
    }

    /// <summary>
    /// если порядок важен
    /// </summary>
    public DomainEvent(Type[] order)
    {
        this.order = order;
    }

    private readonly Dictionary<Type, Func<TArg, Task>> dictionary = new();
    private readonly Type[] order;

    /// <summary>
    /// Важное ограничение: на одно событие дожен быть один обработчик от каждого типа
    /// но это и правильно, ведь на событие тип должен реагировать однозначно
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public async Task Invoke(TArg arg)
    {
        // в теории если порядок не важен, то пофиг
        if (order != null)
        {
            foreach (var handlerType in order)
            {
                if (dictionary.TryGetValue(handlerType, out var handler))
                    await handler(arg);
            }
        }
        else
        {
            foreach (var handler in dictionary)
            {
                await handler.Value(arg);
            }
        }
    }

    /// <summary>
    /// Важное ограничение: для нормальной работы мы можем подписать только статические методы типа
    /// </summary>
    /// <param name="handler"></param>
    public void Subscribe(Func<TArg, Task> handler)
    {
        // рефлексия при старте приложения нам не мешает
        Type declaringType = handler.Method.DeclaringType;
        dictionary[declaringType] = handler;
    }
}
