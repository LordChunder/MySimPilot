using System;
using System.Globalization;
using MySimPilot.Data;
using MySimPilot.ViewModel;

namespace MySimPilot.Handlers
{
    public class PilotHandler: ObservableObject
    {
        private static PilotHandler _instance;

        public static PilotHandler GetInstance()
        {
            return _instance ?? (_instance = new PilotHandler());
        }

        public MyPilotData PilotData = new MyPilotData();

        public double DTotalFlightHours
        {
            get => PilotData.LogBook.TotalFightTime;
            private set => SetProperty(ref PilotData.LogBook.TotalFightTime,value);
        }
    }
}