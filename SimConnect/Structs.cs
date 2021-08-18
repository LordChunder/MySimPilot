using System;
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
        public double PLANE_LONGITUDE;
        public double PLANE_LATITUDE;
        public double AIRSPEED_INDICATED;
        public double SIM_ON_GROUND;
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