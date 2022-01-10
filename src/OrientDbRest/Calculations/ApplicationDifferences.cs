using OrientDbRest.Queries;

namespace MyApp;

public class ApplicationDifferences
{
    public Application FirstVersion { get; set; }
    public Application SecondVersion { get; set; }

    public List<DeltaState> AddedStates { get; } = new();

    public List<DeltaState> RemovedStates { get;  } = new();

    public void AddAddedState(AbstractStateId stateId, IEnumerable<ConcreteStateEntity> concreteState, List<DeltaAction> outgoingDeltaActions, List<DeltaAction> incommingDeltaActions)
    {
        AddedStates.Add(new DeltaState
        {
            StateId = stateId,
            ConcreteStates = concreteState,
            incommingDeltaActions = incommingDeltaActions,
            OutgoingDeltaActions = outgoingDeltaActions,
        });
    }

    public void AddRemovedState(AbstractStateId stateId, IEnumerable<ConcreteStateEntity> concreteState, List<DeltaAction> outgoingDeltaActions, List<DeltaAction> incommingDeltaActions)
    {
        RemovedStates.Add(new DeltaState
        {
            StateId = stateId,
            ConcreteStates = concreteState,
            incommingDeltaActions = incommingDeltaActions,
            OutgoingDeltaActions = outgoingDeltaActions,
        });
    }
}
