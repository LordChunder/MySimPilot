using System.Runtime.InteropServices;
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace MySimPilot.SimConnect
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PlaneMetadatas
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string TITLE;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string ATC_MODEL;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string ATC_ID;
    }
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PlaneVariables
    {
        public double PLANE_ALTITUDE;
        public double PLANE_HEADING_DEGREES_TRUE;
        public double PLANE_LONGITUDE;
        public double PLANE_LATITUDE;
        public double AIRSPEED_INDICATED;
        public double SIM_ON_GROUND;
    }
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PlaneLandingData
    {
        public double PLANE_TOUCHDOWN_PITCH_DEGREES;
        public double PLANE_TOUCHDOWN_NORMAL_VELOCITY;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PlaneGaugeStates
    {
        public double PARTIAL_PANEL_ATTITUDE;
        public double PARTIAL_PANEL_AIRSPEED;
        public double PARTIAL_PANEL_ALTIMETER;
        public double PARTIAL_PANEL_COMM;
        public double PARTIAL_PANEL_COMPASS;
        public double PARTIAL_PANEL_ADF;
        public double PARTIAL_PANEL_ELECTRICAL;
        public double PARTIAL_PANEL_ENGINE;
        public double PARTIAL_PANEL_HEADING;
        public double PARTIAL_PANEL_NAV;
        public double PARTIAL_PANEL_TRANSPONDER;
        public double PARTIAL_PANEL_VERTICAL_VELOCITY;
    }
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SimulationVariables
    {
        public double UNLIMITED_FUEL;
        public double REALISM_CRASH_DETECTION;
        public double SIMULATION_RATE;
        public double REALISM;
    }


}