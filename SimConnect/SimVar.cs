using Microsoft.FlightSimulator.SimConnect;

namespace MySimPilot.SimConnect
{
    public class SimVar
    {
        public string Unit { get; set; }
        public SIMCONNECT_DATATYPE DataType { get; set; }
        public bool Settable { get; set; }
    }
}