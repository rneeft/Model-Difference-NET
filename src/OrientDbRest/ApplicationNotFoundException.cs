namespace MyApp;

public class ApplicationNotFoundException : Exception
{
    public ApplicationNotFoundException(string name, string version)
    {
        Name = name;
        Version = version;
    }

    public string Name { get; }
    public string Version { get; }
}
