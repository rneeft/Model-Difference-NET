using MediatR;
using MyApp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;

namespace OrientDbRest.Queries
{
    public class WidgetEntity
    {
        [JsonProperty("in_isChildOf")]
        public string[] InIsChilderenOf { get; set; } = Array.Empty<string>();

        [JsonProperty("out_isChildOf")]
        public string[] OutIsChilderenOf { get; set; } = Array.Empty<string>();
        public string Role { get; set; }
        public string Title { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new();
    }

    public  class WidgetTreeRequest : IRequest<WidgetEntity[]>
    {
        public ConcreteIDCustom ConcreteIDCustom { get; set; }
    }

    public class WidgetTreeRequestHandler : IRequestHandler<WidgetTreeRequest, WidgetEntity[]>
    {
        private readonly HttpClient client;

        public WidgetTreeRequestHandler(IHttpClientFactory httpClientFactory)
        {
            client = httpClientFactory.CreateOrientDbClient();
        }

        public async Task<WidgetEntity[]> Handle(WidgetTreeRequest request, CancellationToken cancellationToken)
        {
            var sql = $"SELECT FROM (TRAVERSE IN('isChildOf') FROM (SELECT FROM Widget WHERE ConcreteIDCustom = '{request.ConcreteIDCustom.Value}'))";
            var urlencoded = HttpUtility.UrlEncode(sql);
            var response = await client.GetAsync(urlencoded);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var result = JObject.Parse(content)["result"];

            if (result is null) return Array.Empty<WidgetEntity>();

            var entities = result
                .Select(ToWidgetEntity)
                .ToArray();

            return entities;
        }

        private WidgetEntity ToWidgetEntity(JToken token, int arg2)
        {
            return new WidgetEntity
            {
                Title = token["Title"]?.ToString() ?? string.Empty,
                Role = token["Role"]?.ToString() ?? string.Empty,
                InIsChilderenOf = token["in_isChildOf"]?.Select(x => x.ToString())?.ToArray() ?? Array.Empty<string>(),
                OutIsChilderenOf = token["out_isChildOf"]?.Select(x => x.ToString())?.ToArray() ?? Array.Empty<string>(),
                Properties = token.Children<JProperty>().ToDictionary(x => x.Name, x => x.Value.ToString())
            };
        }
    }
}
