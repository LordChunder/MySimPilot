using Microsoft.FlightSimulator.SimConnect;

public class DataHandler
    {
        private static DataHandler _instance;
        public static DataHandler GetInstance()
        {
            return _instance ?? (_instance = new DataHandler());
        }

        public SIMCONNECT_DATA_XYZ AircraftVelocity;
        public SIMCONNECT_DATA_LATLONALT AircraftPosition;
        public string AircraftTitle;
        public bool AircraftOnGround;
    }