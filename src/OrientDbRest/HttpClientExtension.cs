namespace System.Net.Http;

public static class HttpClientExtension
{
    public static HttpClient CreateOrientDbClient(this IHttpClientFactory factory)
    {
        return factory.CreateClient("orientdb");
    }
}
