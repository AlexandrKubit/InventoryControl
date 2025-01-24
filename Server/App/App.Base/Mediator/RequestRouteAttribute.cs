namespace App.Base.Mediator;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class RequestRouteAttribute : Attribute
{
    public enum Types
    {
        Command = 1,
        Query = 2
    }
    public string Path { get; }
    public Types Type { get; }

    public RequestRouteAttribute(string path, Types type)
    {
        Path = path;
        Type = type;
    }
}
