using System;
using System.Windows.Threading;

namespace MySimPilot.Handlers
{
    public class FlightHandler
    {
        private static FlightHandler _instance;

        public static FlightHandler GetInstance()
        {
            return _instance ?? (_instance = new FlightHandler());
        }
        private DispatcherTimer _timer = new DispatcherTimer();
        private FlightHandler()
        {
            _timer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _timer.Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            
        }


        private void EngineFailure(int engineNum = 0)
        {
            
        } 
    }
}