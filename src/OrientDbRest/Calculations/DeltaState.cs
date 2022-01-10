using OrientDbRest.Queries;

namespace MyApp;

public class DeltaState
{
    public AbstractStateId StateId { get; set; }
    public IEnumerable<ConcreteStateEntity> ConcreteStates { get; set; }
    public List<DeltaAction> OutgoingDeltaActions { get; set; }
    public List<DeltaAction> incommingDeltaActions { get; set; }
}
