namespace MySimPilot.SimConnect
{

    public enum ReceiveEvents
    {
        PlaneCrashed,
        PositionChanged,
    }

    public enum SendEvents
    {
        ToggleEngine1Failure
    }
    public enum Definition
    {
        PlaneMetadatas,
        PlaneVariables,
        PlaneLandingData,
        SimulationVariables,
        PlaneGaugeStates
    }

    public enum MessageType
    {
        Error,
        Alert,
    }

    public enum GroupPriority
    {
        IdPriorityStandard = 1900000000
    }
    public enum FailableGauge   
    {
        AttitudeIndicator,
        AirspeedIndicator,
        AltitudeIndicator,
        AdfIndicator,
        CommsPanel,
        Compass,
        ElectricalPanel,
        HeadingIndicator,
        TransponderPanel,
        VaccuumInstruments,
        VerticalSpeedIndicator
    }
}