using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using GMap.NET;
using Microsoft.FlightSimulator.SimConnect;
using MySimPilot.Data;
using MySimPilot.Handlers;
using MySimPilot.SimConnect;
using MySimPilot.ViewModel;

namespace MySimPilot
{
    public class SimvarsViewModel : ObservableObject, IBaseSimConnectWrapper
    {
        // User-defined win32 event
        private const int WmUserSimconnect = 0x0402;

        // Writer to use
        private readonly SimVarMapper _mapper;

        private IntPtr _hWnd = new IntPtr(0);
        public Microsoft.FlightSimulator.SimConnect.SimConnect SimConnection;
        private readonly DispatcherTimer _pullDataTimer = new DispatcherTimer();
        private static SimvarsViewModel _instance;

        public static SimvarsViewModel GetInstance()
        {
            return _instance ?? (_instance = new SimvarsViewModel());
        }

        private SimvarsViewModel()
        {
            _mapper = new SimVarMapper();
            _pullDataTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _pullDataTimer.Tick += OnTickPullData;

            CmdToggleConnect = new BaseCommand(p => { Connect(); });

            DataHandler.GetInstance().BConnected = false;
        }

        #region UIMappings

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public string SConnectButtonLabel
        {
            get => _mSConnectedButtonLabel;
            internal set => SetProperty(ref _mSConnectedButtonLabel, value);
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public BaseCommand CmdToggleConnect { get; }

        private string _mSConnectedButtonLabel = "Connect";

        #endregion

        private void Connect()
        {
            Console.WriteLine(@"Trying to connect to sim");
            if (DataHandler.GetInstance().BConnected) return;
            try
            {
                // The constructor is similar to SimConnect_Open in the native API
                SimConnection = new Microsoft.FlightSimulator.SimConnect.SimConnect(
                    $"Simconnect{Guid.NewGuid()}",
                    _hWnd,
                    WmUserSimconnect,
                    null,
                    0);


                SimConnection.OnRecvOpen += OnRecvOpen;
                SimConnection.OnRecvQuit += OnRecvQuit;
                SimConnection.OnRecvException += OnRecvException;
                SimConnection.OnRecvSimobjectDataBytype += OnRecvSimobjectDataByType;
                SimConnection.OnRecvEvent += OnRecvEvent;

                // Setup Evens
                SimConnection.SubscribeToSystemEvent(
                    ReceiveEvents.PlaneCrashed,
                    "Crashed");
                SimConnection.SubscribeToSystemEvent(
                    ReceiveEvents.PositionChanged,
                    "PositionChanged");

                //setup sim Vars
                var definition = Definition.PlaneMetadatas;
                foreach (var value in _mapper.GetRequestsForStruct<PlaneMetadatas>())
                {
                    SimConnection.AddToDataDefinition(
                        definition,
                        value.NameUnitTuple.Name,
                        value.NameUnitTuple.Unit,
                        value.DataType,
                        0.0f,
                        Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
                }

                SimConnection.RegisterDataDefineStruct<PlaneMetadatas>(definition);

                definition = Definition.PlaneVariables;
                foreach (var value in _mapper.GetRequestsForStruct<PlaneVariables>())
                {
                    SimConnection.AddToDataDefinition(
                        definition,
                        value.NameUnitTuple.Name,
                        value.NameUnitTuple.Unit,
                        value.DataType,
                        0.0f,
                        Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
                }

                SimConnection.RegisterDataDefineStruct<PlaneVariables>(definition);

                definition = Definition.SimulationVariables;
                foreach (var value in _mapper.GetRequestsForStruct<SimulationVariables>())
                {
                    SimConnection.AddToDataDefinition(
                        definition,
                        value.NameUnitTuple.Name,
                        value.NameUnitTuple.Unit,
                        value.DataType,
                        0.0f,
                        Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
                }

                SimConnection.RegisterDataDefineStruct<SimulationVariables>(definition);

                definition = Definition.PlaneGaugeStates;
                foreach (var value in _mapper.GetRequestsForStruct<PlaneGaugeStates>())
                {
                    SimConnection.AddToDataDefinition(
                        definition,
                        value.NameUnitTuple.Name,
                        value.NameUnitTuple.Unit,
                        value.DataType,
                        0.0f,
                        Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
                }

                SimConnection.RegisterDataDefineStruct<PlaneGaugeStates>(definition);

                definition = Definition.PlaneLandingData;
                foreach (var value in _mapper.GetRequestsForStruct<PlaneLandingData>())
                {
                    Console.WriteLine(value.NameUnitTuple.Name);
                    SimConnection.AddToDataDefinition(
                        definition,
                        value.NameUnitTuple.Name,
                        value.NameUnitTuple.Unit,
                        value.DataType,
                        0.0f,
                        Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
                }

                SimConnection.RegisterDataDefineStruct<PlaneLandingData>(definition);
            }
            catch (COMException ex)
            {
                Console.WriteLine(@"Connection to KH failed: " + ex.Message);
                DataHandler.GetInstance().BConnected = false;
                DataHandler.GetInstance().LMessages.Add(new Message("Failed to connect to simulator",
                    DateTime.Now, MessageType.Error)
                );
            }
        }


        private void OnTickPullData(object sender, EventArgs e)
        {
            DataHandler.GetInstance().BSimVarTickOdd = !DataHandler.GetInstance().BSimVarTickOdd;
            try
            {
                SimConnection?.RequestDataOnSimObjectType(
                    null,
                    Definition.PlaneGaugeStates,
                    0,
                    SIMCONNECT_SIMOBJECT_TYPE.USER);
                SimConnection?.RequestDataOnSimObjectType(
                    null,
                    Definition.PlaneVariables,
                    0,
                    SIMCONNECT_SIMOBJECT_TYPE.USER);

                SimConnection?.RequestDataOnSimObjectType(
                    null,
                    Definition.SimulationVariables,
                    0,
                    SIMCONNECT_SIMOBJECT_TYPE.USER);
                SimConnection?.RequestDataOnSimObjectType(
                    null,
                    Definition.PlaneLandingData,
                    0,
                    SIMCONNECT_SIMOBJECT_TYPE.USER);
                SimConnection?.RequestDataOnSimObjectType(
                    null,
                    Definition.PlaneMetadatas,
                    0,
                    SIMCONNECT_SIMOBJECT_TYPE.USER);
            }
            catch (COMException ex)
            {
                Console.WriteLine(@"Connection to KH failed: " + ex.Message);
                DataHandler.GetInstance().LMessages.Add(new Message("Failed to connect to simulator",
                    DateTime.Now, MessageType.Error));
                DataHandler.GetInstance().BConnected = false;
            }


            //TODO Tidy up
            var dataHandler = DataHandler.GetInstance();
            DataHandler.GetInstance().SUserAircraftMetaInfo = dataHandler.PlaneMetadata != null
                ? $"Aircraft: {dataHandler.PlaneMetadata.Value.TITLE}\n" +
                  $"Type: {dataHandler.PlaneMetadata.Value.ATC_MODEL.Split('.')[dataHandler.PlaneMetadata.Value.ATC_MODEL.Split('.').Length-3]}\n" +
                  $"Callsign {dataHandler.PlaneMetadata.Value.ATC_ID}"
                : "Aircraft:\nSelect an Aircraft";

            var crashDection = dataHandler.SimVariables != null &&
                               (dataHandler.SimVariables.Value.REALISM_CRASH_DETECTION != 0);
            var unlimitedFuel =
                dataHandler.SimVariables != null && (dataHandler.SimVariables.Value.UNLIMITED_FUEL != 0);
            DataHandler.GetInstance().SUserSimulationInfo = dataHandler.SimVariables != null
                ? "Simulation Settings:\n" +
                  $"Realsim: Crash Detection ({crashDection})\n" +
                  $"Unlimited Fuel ({unlimitedFuel})"
                : "Simulation Settings:\nConnect to Flight Sim";


            if (dataHandler.PlaneVariables.HasValue)
            {
                FlightHandler.GetInstance().LatLongPlanePosition = new PointLatLng(dataHandler.PlaneVariables.Value.PLANE_LATITUDE,
                    dataHandler.PlaneVariables.Value.PLANE_LONGITUDE);
                FlightHandler.GetInstance().DAircraftHeadingTrue =
                    (180 / Math.PI) * dataHandler.PlaneVariables.Value.PLANE_HEADING_DEGREES_TRUE;


                if (dataHandler.PlaneVariables.Value.SIM_ON_GROUND == 0)
                    dataHandler.PlaneLandingData = null;
                else if (dataHandler.PlaneLandingData != null)
                {
                    FlightHandler.GetInstance().DTouchdownPitch = dataHandler.PlaneLandingData.Value.PLANE_TOUCHDOWN_PITCH_DEGREES;
                    FlightHandler.GetInstance().DTouchdownRate = dataHandler.PlaneLandingData.Value.PLANE_TOUCHDOWN_NORMAL_VELOCITY;
                }
                    
            }
        }

        private void OnRecvEvent(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
            switch ((ReceiveEvents)data.uEventID)
            {
                case ReceiveEvents.PositionChanged:
                    Console.WriteLine(@"Position changed!");
                    break;
                case ReceiveEvents.PlaneCrashed:
                    Console.WriteLine(@"Plane crashed");
                    break;
            }
        }

        #region Client reciever events

        private void OnRecvSimobjectDataByType(Microsoft.FlightSimulator.SimConnect.SimConnect sender,
            SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            var val = data.dwData.FirstOrDefault();
            if (val is null)
                return;

            var dataHandler = DataHandler.GetInstance();

            switch ((Definition)data.dwDefineID)
            {
                case Definition.PlaneMetadatas:
                    dataHandler.PlaneMetadata = (PlaneMetadatas)val;
                    break;
                case Definition.PlaneVariables:
                    dataHandler.PlaneVariables = (PlaneVariables)val;
                    break;
                case Definition.SimulationVariables:
                    dataHandler.SimVariables = (SimulationVariables)val;
                    break;
                case Definition.PlaneGaugeStates:
                    dataHandler.PlaneGaugeStates = (PlaneGaugeStates)val;
                    break;
                case Definition.PlaneLandingData:
                    dataHandler.PlaneLandingData = (PlaneLandingData)val;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void OnRecvException(Microsoft.FlightSimulator.SimConnect.SimConnect sender,
            SIMCONNECT_RECV_EXCEPTION data)
        {
            var eException = (SIMCONNECT_EXCEPTION)data.dwException;
            Console.WriteLine(@"SimConnect_OnRecvException: " + eException);
            DataHandler.GetInstance().LMessages.Add(new Message(eException.ToString(),
                DateTime.Now, MessageType.Error)
            );
        }

        private void OnRecvQuit(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine(@"Disconnected from sim");
            SConnectButtonLabel = "Re-connect";
            DataHandler.GetInstance().BConnected = false;
            _pullDataTimer.Stop();
            DataHandler.GetInstance().LMessages.Add(new Message("Disconnected from sim",
                DateTime.Now, MessageType.Alert)
            );
        }

        private void OnRecvOpen(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine(@"Connected to sim");
            DataHandler.GetInstance().LMessages.Add(new Message("Connected to sim",
                DateTime.Now, MessageType.Alert)
            );
            _pullDataTimer.Start();

            SConnectButtonLabel = "Connected";
            DataHandler.GetInstance().BConnected = true;
            DataHandler.GetInstance().BSimVarTickOdd = false;

            //SimConnection?.MapClientEventToSimEvent(SendEvents.ToggleEngine1Failure, "TOGGLE_ENGINE1_FAILURE");
        }

        #endregion

        public void SendEvent(ReceiveEvents receiveEventsName)
        {
            SimConnection?.TransmitClientEvent(
                Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_OBJECT_ID_USER,
                receiveEventsName, 0, GroupPriority.IdPriorityStandard, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }

        public int GetUserSimConnectWinEvent()
        {
            return WmUserSimconnect;
        }

        public void ReceiveSimConnectMessage()
        {
            SimConnection?.ReceiveMessage();
        }

        public void SetWindowHandle(IntPtr hWnd)
        {
            _hWnd = hWnd;
        }

        public void Disconnect()
        {
        }
    }
}