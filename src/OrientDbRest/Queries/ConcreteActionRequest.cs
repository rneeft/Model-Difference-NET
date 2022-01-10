using MediatR;
using MyApp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrientDbRest.Queries;

public class ConcreteActionRequest : IRequest<ConcreteAction[]>
{
    public ConcreteActionId ConcreteActionId { get; set; }
}

public class ConcreteActionRequestHandler : IRequestHandler<ConcreteActionRequest, ConcreteAction[]>
{
    private readonly HttpClient client;

    public ConcreteActionRequestHandler(IHttpClientFactory clientFactory)
    {
        client = clientFactory.CreateOrientDbClient();
    }

    public async Task<ConcreteAction[]> Handle(ConcreteActionRequest request, CancellationToken cancellationToken)
    {
        var sql = $"SELECT `Desc`, actionId FROM ConcreteAction WHERE actionId = '{request.ConcreteActionId.Value}'";

        var response = await client.GetAsync(sql);
        var content = await response.Content.ReadAsStringAsync();

        var result = JObject.Parse(content)["result"]?.ToString() ?? throw new Exception();
        var entities = JsonConvert.DeserializeObject<ConcreteAction[]>(result);

        return entities ?? Array.Empty<ConcreteAction>();
    }
}

public class ConcreteAction
{
    [JsonProperty("Desc")]
    public string Description { get; set; }

    [JsonProperty("actionId")]
    public string ConcreateActionId { get; set; }
}

