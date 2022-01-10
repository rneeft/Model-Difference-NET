namespace MyApp;

public interface IDifferenceProvider
{
    Task<ApplicationDifferences> FindDifferences(Application application1, Application application2)
}
