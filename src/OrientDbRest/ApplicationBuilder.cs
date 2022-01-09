using MediatR;
using OrientDbRest.Queries;

namespace MyApp;

public interface IApplicationBuilder
{
    Task<Application> GetApplicationAsync(string name, string version);
}

public class ApplicationBuilder : IApplicationBuilder
{
    private readonly IMediator mediator;

    public ApplicationBuilder(IMediator mediator)
    {
        this.mediator = mediator;
    }

    public async Task<Application> GetApplicationAsync(string name, string version)
    {
        var stateModel = (await GetAbstractStateModelAsync(name, version))
            ??  throw new ApplicationNotFoundException(name, version);

        var abstractStates = await GetAbstractStates(new ModelIdentifier(stateModel.ModelIdentifier));

        return new Application
        {
            AbstractionAttributes = stateModel.AbstractionAttributes,
            ApplicationName = stateModel.ApplicationName,
            ApplicationVersion = stateModel.ApplicationVersion,
            ModelIdentifier = new ModelIdentifier(stateModel.ModelIdentifier),
            AbstractStates = abstractStates.Select(x => new AbstractState
            {
                Counter = x.Counter,
                ConcreteStateIds = x.ConcreteStateIds.Select(x => new ConcreteStateId(x)).ToArray(),
                InAbstractActions = x.InAbstractActions,
                IsInitial = x.IsInitial,
                ModelIdentifier = new ModelIdentifier(x.ModelIdentifier),
                OutAbstractActions = x.OutAbstractActions
            }).ToArray(),
        };
    }

    private Task<AbstractStateEntity[]> GetAbstractStates(ModelIdentifier modelIdentifier)
    {
        var request = new AbstractStateRequest
        {
            ModelIdentifier = modelIdentifier
        };

        return mediator.Send(request);
    }

    private Task<AbstractStateModel?> GetAbstractStateModelAsync(string name, string version)
    {
        var request = new AbstractStateModelRequest
        {
            ApplicationName = name,
            ApplicationVersion = version
        };

        return mediator.Send(request);
    }
}
