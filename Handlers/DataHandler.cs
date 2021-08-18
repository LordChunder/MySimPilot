using MySimPilot.SimConnect;

namespace MySimPilot.Handlers
{
    public class DataHandler
    {
        private static DataHandler _instance;

        public static DataHandler GetInstance()
        {
            return _instance ?? (_instance = new DataHandler());
        }
       public PlaneMetadatas? PlaneMetadata = null;
       public PlaneVariables? PlaneVariables = null;
       public SimulationVariables? SimVariables = null;
    }
}