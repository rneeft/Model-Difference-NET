using MediatR;
using MyApp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrientDbRest.Queries
{
    public class ConcreteStateEntityRequest : IRequest<ConcreteStateEntity?>
    {
        public ConcreteStateId ConcreteStateId { get; init; }
    }

    public class ConcreteStateEntityRequestHandler : IRequestHandler<ConcreteStateEntityRequest, ConcreteStateEntity?>
    {
        private readonly HttpClient client;

        public ConcreteStateEntityRequestHandler(IHttpClientFactory clientFactory)
        {
            client = clientFactory.CreateOrientDbClient();
        }

        public async Task<ConcreteStateEntity?> Handle(ConcreteStateEntityRequest request, CancellationToken cancellationToken)
        {
            var sql = $"SELECT FROM ConcreteState WHERE ConcreteIDCustom = '{request.ConcreteStateId.Value}' LIMIT 1";

            var response = await client.GetAsync(sql);
            var content = await response.Content.ReadAsStringAsync();

            var result = JObject.Parse(content)["result"]?.ToString() ?? throw new Exception();
            var entities = JsonConvert.DeserializeObject<ConcreteStateEntity[]>(result);

            return entities.SingleOrDefault();
        }
    }

    public class ConcreteStateEntity
    {
        public string ConcreteIDCustom { get; set; }
        public string Screenshot { get; set; }
    }
}
