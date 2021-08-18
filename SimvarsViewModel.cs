using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.FlightSimulator.SimConnect;
using MySimPilot.SimConnect;
using MySimPilot.ViewModel;


namespace MySimPilot
{
    public class Message
    {
        public Message(string body, DateTime time, MessageType type)
        {
            Body = $"({time.ToShortTimeString()}) {body}";
            Type = type;

            switch (Type)
            {
                case MessageType.Error:
                {
                    TextColor = "Red";
                    break;
                }
                case MessageType.Alert:
                {
                    TextColor = "Orange";
                    break;
                    
                }
            }
        }

        public string Body { get; set; }
        private MessageType Type { get; set; }

        public string TextColor { get; set; }
    }

    public class SimvarsViewModel : ObservableObject, IBaseSimConnectWrapper
    {
        // User-defined win32 event
        private const int WmUserSimconnect = 0x0402;

        // Writer to use
        private readonly SimVarMapper _mapper;

        private IntPtr _hWnd = new IntPtr(0);
        private Microsoft.FlightSimulator.SimConnect.SimConnect _simConnection;
        private readonly DispatcherTimer _pullDataTimer = new DispatcherTimer();

        public SimvarsViewModel()
        {
            _mapper = new SimVarMapper();
            _pullDataTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _pullDataTimer.Tick += OnTickPullData;

            CmdToggleConnect = new BaseCommand((p) => { Connect(); });

            SConnectButtonLabel = "Connect";
            BConnected = false;

            LMessages = new ObservableCollection<Message>();
        }


        #region UIMappings

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public string SConnectButtonLabel
        {
            get => _mSConnectedButtonLabel;
            private set => SetProperty(ref _mSConnectedButtonLabel, value);
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public string SUserAircraftMetaInfo
        {
            get => _mSUserAircraftMetaInfo;
            private set => SetProperty(ref _mSUserAircraftMetaInfo, value);
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public string SUserSimulationInfo
        {
            get => _mSUserSimulationInfo;
            private set => SetProperty(ref _mSUserSimulationInfo, value);
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public bool BOddTick
        {
            get => _mBOddTick;
            private set => SetProperty(ref _mBOddTick, value);
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public bool BConnected
        {
            get => _mBConnected;
            private set => SetProperty(ref _mBConnected, value);
        }
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<Message> LMessages { get; set; }

        private bool _mBConnected;
        private string _mSConnectedButtonLabel = "Connect";
        private bool _mBOddTick;
        private string _mSUserAircraftMetaInfo = "Aircraft:\nSelect an Aircraft";
        private string _mSUserSimulationInfo = "Simulation Settings:\nConnect to Flight Sim";


        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public BaseCommand CmdToggleConnect { get; }

        #endregion

        private void Connect()
        {
            Console.WriteLine(@"Trying to connect to sim");
            if (BConnected) return;
            try
            {
                // The constructor is similar to SimConnect_Open in the native API
                _simConnection = new Microsoft.FlightSimulator.SimConnect.SimConnect(
                    $"Simconnect{Guid.NewGuid()}",
                    _hWnd,
                    WmUserSimconnect,
                    null,
                    0);

                BConnected = true;

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
                BConnected = false;
                LMessages.Add(new Message("Failed to connect to simulator",
                   DateTime.Now, MessageType.Error)
                );
            }
        }

        private void OnTickPullData(object sender, EventArgs e)
        {
            BOddTick = !BOddTick;
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
                BConnected = false;
            }

            var dataHandler = Handlers.DataHandler.GetInstance();
            SUserAircraftMetaInfo = dataHandler.PlaneMetadata != null
                ? $"Aircraft: {dataHandler.PlaneMetadata.Value.TITLE}\n" +
                  $"Type: {dataHandler.PlaneMetadata.Value.ATC_MODEL.Split(' ')[1].Split('.')[0]}\n" +
                  $"Callsign {dataHandler.PlaneMetadata.Value.ATC_ID}"
                : "Aircraft:\nSelect an Aircraft";

            var crashDection = dataHandler.SimVariables != null &&
                               (dataHandler.SimVariables.Value.REALISM_CRASH_DETECTION != 0);
            var unlimitedFuel =
                dataHandler.SimVariables != null && (dataHandler.SimVariables.Value.UNLIMITED_FUEL != 0);
            SUserSimulationInfo = dataHandler.SimVariables != null
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

            var dataHandler = Handlers.DataHandler.GetInstance();

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
            LMessages.Add(new Message(eException.ToString(),
                DateTime.Now, MessageType.Error)
            );
        }

        private void OnRecvQuit(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine(@"Disconnected from sim");
            SConnectButtonLabel = "Re-connect";
            BConnected = false;
            _pullDataTimer.Stop();
            LMessages.Add(new Message("Disconnected from sim",
                DateTime.Now, MessageType.Alert)
            );
        }

        private void OnRecvOpen(Microsoft.FlightSimulator.SimConnect.SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine(@"Connected to sim");
            LMessages.Add(new Message("Connected to sim",
                DateTime.Now, MessageType.Alert)
            );
            _pullDataTimer.Start();

            SConnectButtonLabel = "Connected";
            BConnected = true;
            BOddTick = false;
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