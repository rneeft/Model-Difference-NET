using MoreLinq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrientDbRest.Queries;
using System.Diagnostics;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Org.XmlUnit.Builder;

namespace MyApp;

public record ModelIdentifier(string Value);
public record ConcreteStateId(string Value);
public record ConcreteIDCustom(string Value);
public record OrientDbId(string Value);
public class Application
{
    public string[] AbstractionAttributes { get; init; }
    public string ApplicationName { get; init; }
    public string ApplicationVersion { get; init; }
    public ModelIdentifier ModelIdentifier { get; init; }
    public AbstractState[] AbstractStates { get; init; }
}

public class AbstractState
{
    public ConcreteStateId[] ConcreteStateIds { get; set; }
    public ModelIdentifier ModelIdentifier { get; set; }
    public bool IsInitial { get; set; }
    public int Counter { get; set; }
    public string[] InAbstractActions { get; set; }
    public string[] OutAbstractActions { get; set; }
}

internal class Program
{
    static async Task Main(string[] args)
    {
        var host = new AppHostBuilder()
            .BuildHost(args);

        var applicationBuilder = host.Services.GetRequiredService<IApplicationBuilder>();
        var mediator = host.Services.GetRequiredService<IMediator>();

        var application = await applicationBuilder.GetApplicationAsync("wpfApp", "2");

        var initialAbstractState = application.AbstractStates.First(x => x.IsInitial);

        foreach (var id in initialAbstractState.ConcreteStateIds)
        {
            var request = new ConcreteStateEntityRequest
            {
                ConcreteStateId = id
            };

            var concreteState = (await mediator.Send(request))
                ?? throw new Exception("We should have this");

            var widgetTreeRequest = new WidgetTreeRequest
            {
                ConcreteIDCustom = new ConcreteIDCustom(concreteState.ConcreteIDCustom)
            };

            var widgetTreeEntities = await mediator.Send(widgetTreeRequest);

            var rootWidget = widgetTreeEntities
                .Where(x => x.OutIsChilderenOf?.Length == 0)
                .Single();

            var widgetTree = new WidgetTree
            {
                ConcreteStateId = concreteState.ConcreteIDCustom,
                Root = GetChilderen(rootWidget, widgetTreeEntities)
            };

            using var textWriter = new StreamWriter(widgetTree.ConcreteStateId + ".xml");
            {
                var xml = new XmlSerializer(typeof(WidgetTree));
                xml.Serialize(textWriter, widgetTree);
                
                textWriter.Flush();
                textWriter.Close();
            }

            var source1 = Input.FromFile(widgetTree.ConcreteStateId + ".xml")
                .Build();

            var source2 = Input.FromFile("SCC1s6ib7kc62944251090_old")
                .Build();

            var diffb = DiffBuilder.Compare(source1).WithTest(source2).Build();
            var differences = diffb.Differences;
            

        }
    }

    private static Widget GetChilderen(WidgetEntity root,  WidgetEntity[] widgetEntities)
    {
        var properties = root.Properties
                .Select(x => new WidgetProperty { Name = x.Key, Value = x.Value })
                .ToArray();

        var childeren = root.InIsChilderenOf
                .SelectMany(x => widgetEntities.Where(y => y.OutIsChilderenOf.Contains(x)))
                .Select(x => GetChilderen(x, widgetEntities))
                .ToArray();

        return new Widget
        {
            Role = root.Role,
            Title = root.Title,
            Properties = properties,
            Childeren = childeren,
        };
    }
}

public class WidgetTree
{
    public Widget Root { get; set; }
    
    [XmlAttribute]
    public string ConcreteStateId { get; set; }
}

[DebuggerDisplay("{Role}-{Title}")]
public class Widget
{
    [XmlAttribute]
    public string Role { get; set; }

    [XmlAttribute]
    public string Title { get; set; }

    public Widget[] Childeren { get; set; }

    public WidgetProperty[] Properties { get; set; }
}

public class WidgetProperty
{
    [XmlAttribute]
    public string Name { get; set; }

    [XmlAttribute]
    public string Value { get; set; }
}
