using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrientDbRest.Queries;

public class AbstractStateModel
{
    public string[] AbstractionAttributes { get; set; }
    public string ApplicationName { get; set; }
    public string ApplicationVersion { get; set; }
    public string ModelIdentifier { get; set; }
}

public class AbstractStateModelRequest : IRequest<AbstractStateModel?>
{
    public string ApplicationName { get; init; }
    public string ApplicationVersion { get; init; }

}

public class AbstractStateModelRequestHandler : IRequestHandler<AbstractStateModelRequest, AbstractStateModel?>
{
    private readonly HttpClient client;

    public AbstractStateModelRequestHandler(IHttpClientFactory httpClientFactory)
    {
        client = httpClientFactory.CreateOrientDbClient();
    }

    public async Task<AbstractStateModel?> Handle(AbstractStateModelRequest request, CancellationToken cancellationToken)
    {
        var sql = "SELECT FROM AbstractStateModel WHERE " +
         $"applicationName = '{request.ApplicationName}' AND " +
         $"applicationVersion = '{request.ApplicationVersion}'";

        var response = await client.GetAsync(sql);
        var content = await response.Content.ReadAsStringAsync();

        var result = JObject.Parse(content)["result"] ?? throw new Exception();
        var model = JsonConvert.DeserializeObject<AbstractStateModel[]>(result.ToString());

        return model?.SingleOrDefault();
    }
}
