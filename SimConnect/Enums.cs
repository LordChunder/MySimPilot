namespace MySimPilot.SimConnect
{
    internal enum Event
    {
        PlaneCrashed,
        PositionChanged,
    }
    public enum Definition
    {
        PlaneMetadatas,
        PlaneVariables,
        SimulationVariables,
    }

    public enum MessageType
    {
        Error,
        Alert,
    }
}