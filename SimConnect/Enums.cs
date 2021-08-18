namespace MySimPilot.SimConnect
{
    internal enum Event
    {
        PlaneCrashed,
        PositionChanged,

        ToggleEngine1Failure
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
    public enum GaugeStateMode   
    {
        OK = 0,
        Fail = 1,
        Blank = 2
    }
}