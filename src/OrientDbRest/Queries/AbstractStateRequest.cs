using MediatR;
using MyApp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrientDbRest.Queries;

public class AbstractStateEntity
{
    public string[] ConcreteStateIds { get; set; }
    public string ModelIdentifier { get; set; }
    public bool IsInitial { get; set; }
    public int Counter { get; set; }

    [JsonProperty("in_AbstractAction")]
    public string[] InAbstractActions { get; set; }

    [JsonProperty("out_AbstractAction")]
    public string[] OutAbstractActions { get; set; }
}

public class AbstractStateRequest : IRequest<AbstractStateEntity[]>
{
    public ModelIdentifier ModelIdentifier { get; init; }
}

public class AbstractStateRequestHandler : IRequestHandler<AbstractStateRequest, AbstractStateEntity[]>
{
    private readonly HttpClient client;

    public AbstractStateRequestHandler(IHttpClientFactory httpClientFactory)
    {
        client = httpClientFactory.CreateOrientDbClient();
    }

    public async Task<AbstractStateEntity[]> Handle(AbstractStateRequest request, CancellationToken cancellationToken)
    {
        var sql = $"SELECT FROM AbstractState WHERE modelIdentifier = '{request.ModelIdentifier.Value}'";

        var response = await client.GetAsync(sql);
        var content = await response.Content.ReadAsStringAsync();

        var result = JObject.Parse(content)["result"]?.ToString() ?? "";
        var model = JsonConvert.DeserializeObject<AbstractStateEntity[]>(result.ToString());

        return model ?? Array.Empty<AbstractStateEntity>();
    }
}

