using MediatR;
using MyApp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace OrientDbRest.Queries
{
    public class AbstractAction
    {
        public string[] ConcreteActionIds { get; set; } = Array.Empty<string>();
        public string In { get; set; } = default!;
        public string Out { get; set; } = default!;
        public string ActionId { get; set; } = default!;
    }

    public class AbstractActionRequest : IRequest<AbstractAction>
    {
        public AbstractActionId AbstractActionId { get; init; } = default!;
    }

    public class AbstractActionRequestHandler : IRequestHandler<AbstractActionRequest, AbstractAction>
    {
        private readonly HttpClient client;

        public AbstractActionRequestHandler(IHttpClientFactory clientFactory)
        {
            client = clientFactory.CreateOrientDbClient();
        }

        public async Task<AbstractAction> Handle(AbstractActionRequest request, CancellationToken cancellationToken)
        {
            var sql = $"SELECT FROM AbstractAction where actionId = '{request.AbstractActionId.Value}' ";

            var response = await client.GetAsync(sql);
            var content = await response.Content.ReadAsStringAsync();

            var result = JObject.Parse(content)["result"]?.ToString() ?? throw new Exception();
            var entities = JsonConvert.DeserializeObject<AbstractAction[]>(result);

            return entities?.Single() ?? throw new Exception("Something is wrong here");
        }
    }
}
