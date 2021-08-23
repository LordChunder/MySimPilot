using System;
using System.Windows.Threading;
using GMap.NET;
using Microsoft.FlightSimulator.SimConnect;
using MySimPilot.SimConnect;
using MySimPilot.ViewModel;

namespace MySimPilot.Handlers
{
    public class FlightHandler : ObservableObject
    {
        private static FlightHandler _instance;

        public static FlightHandler GetInstance()
        {
            return _instance ?? (_instance = new FlightHandler());
        }

        public readonly DispatcherTimer MapUpdate = new DispatcherTimer();


        public PointLatLng LatLongPlanePosition
        {
            get => _mLatLongPlanePosition;
            internal set => SetProperty(ref _mLatLongPlanePosition, value);
        }
        public double DAircraftHeadingTrue
        {
            get => _mDAircraftHeadingTrue;
            internal set => SetProperty(ref _mDAircraftHeadingTrue, value);
        }
        
        public double DTouchdownPitch
        {
            get => _mDTouchdownPitch;
            internal set => SetProperty(ref _mDTouchdownPitch, value);
        }
        public double DTouchdownRate
        {
            get => _mDTouchdownRate;
            internal set => SetProperty(ref _mDTouchdownRate, value);
        }
        
        private PointLatLng _mLatLongPlanePosition;
        private double _mDAircraftHeadingTrue;
        private double _mDTouchdownRate;
        private double _mDTouchdownPitch;


        public static void FailGauge(FailableGauge[] gaugesToFail)
        {
            var planeGaugeStates = DataHandler.GetInstance().PlaneGaugeStates;
            if (planeGaugeStates == null) return;
            var values = planeGaugeStates.Value;
            foreach (var gauge in gaugesToFail)
            {
                switch (gauge)
                {
                    case FailableGauge.AdfIndicator:
                        values.PARTIAL_PANEL_ADF = 1;
                        break;
                    case FailableGauge.AttitudeIndicator:
                        values.PARTIAL_PANEL_ATTITUDE = 1;
                        break;
                    case FailableGauge.AirspeedIndicator:
                        values.PARTIAL_PANEL_AIRSPEED = 1;
                        break;
                    case FailableGauge.AltitudeIndicator:
                        values.PARTIAL_PANEL_ALTIMETER = 1;
                        break;
                    case FailableGauge.CommsPanel:
                        values.PARTIAL_PANEL_COMM = 1;
                        break;
                    case FailableGauge.Compass:
                        values.PARTIAL_PANEL_COMPASS = 1;
                        break;
                    case FailableGauge.ElectricalPanel:
                        values.PARTIAL_PANEL_ELECTRICAL = 1;
                        break;
                    case FailableGauge.HeadingIndicator:
                        values.PARTIAL_PANEL_HEADING = 1;
                        break;
                    case FailableGauge.TransponderPanel:
                        values.PARTIAL_PANEL_TRANSPONDER = 1;
                        break;
                    case FailableGauge.VerticalSpeedIndicator:
                        values.PARTIAL_PANEL_VERTICAL_VELOCITY = 1;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
      
  
            SimvarsViewModel.GetInstance().SimConnection.SetDataOnSimObject(
                Definition.PlaneGaugeStates,
                Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_OBJECT_ID_USER,
                SIMCONNECT_DATA_SET_FLAG.DEFAULT, values);
            //SimvarsViewModel.GetInstance().SendEvent(Event.ToggleEngine1Failure);
        }
    }
}