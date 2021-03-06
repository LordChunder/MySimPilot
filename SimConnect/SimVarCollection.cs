namespace MySimPilot.SimConnect
{
    public class SimVarCollection
    {
        public SimVarCollection(
            PlaneMetadatas metadata,
            PlaneVariables planeVariables,
            SimulationVariables simVariables )
        {
            SimulationRate = simVariables.SIMULATION_RATE;
            RealismCrashDetection = simVariables.REALISM_CRASH_DETECTION is 1.0;
            Grounded = planeVariables.SIM_ON_GROUND is 1.0;
            UnlimitedFuelFlag = simVariables.UNLIMITED_FUEL is 1.0;
            Altitude = planeVariables.PLANE_ALTITUDE;
            Latitude = planeVariables.PLANE_LATITUDE;
            Longitude = planeVariables.PLANE_LONGITUDE;
            Airspeed = planeVariables.AIRSPEED_INDICATED;
            Title = metadata.TITLE;
        }

        public double SimulationRate { get; set; }
        public bool Grounded { get; set; }
        public bool RealismCrashDetection { get; set; }
        public bool UnlimitedFuelFlag { get; set; }
        public double Altitude { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Airspeed { get; set; }
        public string Title { get; set; }
        public bool Arrived { get; set; }

   
    }
}