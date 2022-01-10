using MoreLinq;
using MediatR;
using OrientDbRest.Queries;

namespace MyApp;

public class AbstractStateDifferenceProvider : IDifferenceProvider
{
    private readonly IMediator mediator;

    public AbstractStateDifferenceProvider(IMediator mediator)
    {
        this.mediator = mediator;
    }

    public async Task<ApplicationDifferences> FindDifferences(Application application1, Application application2)
    {
        var abstractStatesEqual = application1.AbstractionAttributes.SequenceEqual(application2.AbstractionAttributes);

        if (!abstractStatesEqual)
        {
            // if Abstract Attributes are different, Abstract Layer is different and no sense to continue
            throw new AbstractAttributesNotTheSameException();
        }

        var applicationDifferences = new ApplicationDifferences()
        {
            FirstVersion = application1,
            SecondVersion = application2
        };

        var removeStates = application1.AbstractStates
            .Where(x => !application2.AbstractStates.Select(y => y.StateId).Contains(x.StateId))
            .ToList();

        var addedStates = application2.AbstractStates
            .Where(x => !application1.AbstractStates.Select(y => y.StateId).Contains(x.StateId))
            .ToList();

        // we now know which ABSTRACT state is removed from application2 and which abstract state has been added in application2
        // we need to map the abstract states to concrete state. We now we assume that with the removal of remove state
        // the concrete state are also removed.


        foreach (var removedState in removeStates)
        {
            var concreteState = await GetConcreteStateEntities(removedState);
            var outgoingDeltaActions = await GetDeltaAction(removedState.OutAbstractActions, ActionType.Outgoing);
            var incommingDeltaActions = await GetDeltaAction(removedState.OutAbstractActions, ActionType.Incomming);

            applicationDifferences.AddRemovedState(removedState.StateId, concreteState, outgoingDeltaActions, incommingDeltaActions);
        }

        foreach (var addedState in addedStates)
        {
            var concreteState = await GetConcreteStateEntities(addedState);
            var outgoingDeltaActions = await GetDeltaAction(addedState.OutAbstractActions, ActionType.Outgoing);
            var incommingDeltaActions = await GetDeltaAction(addedState.OutAbstractActions, ActionType.Incomming);

            applicationDifferences.AddAddedState(addedState.StateId, concreteState, outgoingDeltaActions, incommingDeltaActions);
        }

        return applicationDifferences;
    }

    private async Task<IEnumerable<ConcreteStateEntity>> GetConcreteStateEntities(AbstractState abstractState)
    {
        var returns = new List<ConcreteStateEntity>();
        foreach (var id in abstractState.ConcreteStateIds)
        {
            var concreteState = await mediator.Send(new ConcreteStateEntityRequest { ConcreteStateId = id });
            if (concreteState is not null)
            {
                returns.Add(concreteState);
            }
        }

        return returns;
    }

    private async Task<List<DeltaAction>> GetDeltaAction(AbstractActionId[] actionIds, ActionType actionType)
    {
        var returns = new List<DeltaAction>();

        // for the delta action we need to retrieve the Description from a ConcreteAction
        // Every AbstractAction must have a corresponding concrete action

        // we have the id of the action -> find the abstract action

        foreach (var id in actionIds)
        {
            var abstractAction = await mediator.Send(new AbstractActionRequest { AbstractActionId = id });
            var aConcreteActionId = abstractAction.ConcreteActionIds.First();

            var concreteAction = await mediator.Send(new ConcreteActionRequest { ConcreteActionId = new ConcreteActionId(aConcreteActionId) });
            var description = concreteAction?.First().Description ?? "TILT";

            returns.Add(new DeltaAction 
            {
                ActionId = id,
                Description = description,
                ActionType = actionType,
            });
        }
     
        return returns;
    }
}
