using Microsoft.FlightSimulator.SimConnect;

namespace MySimPilot.SimConnect
{
    public class SimvarRequest
    {
        public Definition Definition { get; set; }
        public Request Request { get; set; }
        public (string Name, string Unit) NameUnitTuple { get; set; }
        public SIMCONNECT_DATATYPE DataType { get; set; } = SIMCONNECT_DATATYPE.FLOAT64;
    };
}