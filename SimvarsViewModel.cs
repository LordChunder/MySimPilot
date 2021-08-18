using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;
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
        private Microsoft.FlightSimulator.SimConnect.SimConnect _simConnection;
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
                _simConnection = new Microsoft.FlightSimulator.SimConnect.SimConnect(
                    $"Simconnect{Guid.NewGuid()}",
                    _hWnd,
                    WmUserSimconnect,
                    null,
                    1);
                

                _simConnection.OnRecvOpen += OnRecvOpen;
                _simConnection.OnRecvQuit += OnRecvQuit;
                _simConnection.OnRecvException += OnRecvException;
                _simConnection.OnRecvSimobjectDataBytype += OnRecvSimobjectDataByType;
                _simConnection.OnRecvEvent += OnRecvEvent;

                // Setup crash
                _simConnection.SubscribeToSystemEvent(
                    Event.PlaneCrashed,
                    "Crashed");
                _simConnection.SubscribeToSystemEvent(
                    Event.PositionChanged,
                    "PositionChanged");


                var definition = Definition.PlaneMetadatas;
                foreach (var value in _mapper.GetRequestsForStruct<PlaneMetadatas>())
                {
                    _simConnection.AddToDataDefinition(
                        definition,
                        value.NameUnitTuple.Name,
                        value.NameUnitTuple.Unit,
                        value.DataType,
                        0.0f,
                        Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
                }

                _simConnection.RegisterDataDefineStruct<PlaneMetadatas>(definition);

                definition = Definition.PlaneVariables;
                foreach (var value in _mapper.GetRequestsForStruct<PlaneVariables>())
                {
                    _simConnection.AddToDataDefinition(
                        definition,
                        value.NameUnitTuple.Name,
                        value.NameUnitTuple.Unit,
                        value.DataType,
                        0.0f,
                        Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
                }

                _simConnection.RegisterDataDefineStruct<PlaneVariables>(definition);

                definition = Definition.SimulationVariables;
                foreach (var value in _mapper.GetRequestsForStruct<SimulationVariables>())
                {
                    _simConnection.AddToDataDefinition(
                        definition,
                        value.NameUnitTuple.Name,
                        value.NameUnitTuple.Unit,
                        value.DataType,
                        0.0f,
                        Microsoft.FlightSimulator.SimConnect.SimConnect.SIMCONNECT_UNUSED);
                }

                _simConnection.RegisterDataDefineStruct<SimulationVariables>(definition);
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
                _simConnection?.RequestDataOnSimObjectType(
                    Request.PLANE_ALTITUDE,
                    Definition.PlaneVariables,
                    0,
                    SIMCONNECT_SIMOBJECT_TYPE.USER);

                _simConnection?.RequestDataOnSimObjectType(
                    Request.REALISM_CRASH_DETECTION,
                    Definition.SimulationVariables,
                    0,
                    SIMCONNECT_SIMOBJECT_TYPE.USER);

                _simConnection?.RequestDataOnSimObjectType(
                    Request.TITLE,
                    Definition.PlaneMetadatas,
                    0,
                    SIMCONNECT_SIMOBJECT_TYPE.USER);
            }
            catch (COMException ex)
            {
                Console.WriteLine(@"Connection to KH failed: " + ex.Message);
                DataHandler.GetInstance().BConnected = false;
            }

            var dataHandler = DataHandler.GetInstance();
            DataHandler.GetInstance().SUserAircraftMetaInfo = dataHandler.PlaneMetadata != null
                ? $"Aircraft: {dataHandler.PlaneMetadata.Value.TITLE}\n" +
                  $"Type: {dataHandler.PlaneMetadata.Value.ATC_MODEL}\n" +
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
        }

        private void OnRecvEvent(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
            switch ((Event)data.uEventID)
            {
                case Event.PositionChanged:
                    Console.WriteLine(@"Position changed!");
                    break;
                case Event.PlaneCrashed:
                    Console.WriteLine(@"Plane crashed");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnRecvException(Microsoft.FlightSimulator.SimConnect.SimConnect sender,
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
        }

        #endregion

        public int GetUserSimConnectWinEvent()
        {
            return WmUserSimconnect;
        }

        public void ReceiveSimConnectMessage()
        {
            _simConnection?.ReceiveMessage();
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