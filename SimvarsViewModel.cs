using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.FlightSimulator.SimConnect;


namespace MySimPilot
{
    public enum Definition
    {
        Dummy = 0
    }

    public enum Request
    {
        Dummy = 0,
        SimVarStruct
    }

    // String properties must be packed inside of a struct
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct SimVarStruct
    {
        // this is how you declare a fixed size string
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string sValue;

        // other definitions can be added to this struct
        // ...
    };

    public class SimvarRequest : ObservableObject
    {
        public Definition EDef = Definition.Dummy;
        public Request ERequest = Request.Dummy;

        public string SName { get; set; }
 
        public bool BIsString { get; set; }

        public double DValue
        {
            get => _mDValue;
            set => SetProperty(ref _mDValue, value);
        }

        private double _mDValue = 0.0;

        public string SValue
        {
            get => _mSValue;
            set => SetProperty(ref _mSValue, value);
        }

        private string _mSValue = null;

        public string SUnits { get; set; }

        public bool BPending = true;

        public bool BStillPending
        {
            get => _mBStillPending;
            set => SetProperty(ref _mBStillPending, value);
        }

        private bool _mBStillPending = false;
    };

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class SimvarsViewModel : BaseViewModel, IBaseSimConnectWrapper
    {
        #region IBaseSimConnectWrapper implementation

        /// User-defined win32 event
        public const int WmUserSimconnect = 0x0402;

        /// Window handle
        private IntPtr _mHWnd = new IntPtr(0);

        /// SimConnect object
        private SimConnect _mOSimConnect = null;

        public bool BConnected
        {
            get => _mBConnected;
            private set => SetProperty(ref _mBConnected, value);
        }

        private bool _mBConnected = false;

        private uint _mICurrentDefinition = 0;
        private uint _mICurrentRequest = 0;

        public int GetUserSimConnectWinEvent()
        {
            return WmUserSimconnect;
        }

        public void ReceiveSimConnectMessage()
        {
            _mOSimConnect?.ReceiveMessage();
        }

        public void SetWindowHandle(IntPtr hWnd)
        {
            _mHWnd = hWnd;
        }

        public void Disconnect()
        {
            Console.WriteLine(@"Disconnect");

            _mOTimer.Stop();
            BOddTick = false;

            if (_mOSimConnect != null)
            {
                // Dispose serves the same purpose as SimConnect_Close()
                _mOSimConnect.Dispose();
                _mOSimConnect = null;
            }

            SConnectButtonLabel = "Connect";
            SFlightStatus = "Not Connected";
            BConnected = false;

            // Set all requests as pending
            foreach (var oSimvarRequest in LSimvarRequests)
            {
                oSimvarRequest.BPending = true;
                oSimvarRequest.BStillPending = true;
            }
        }

        #endregion

        #region UI bindings


        public string SLoginStatus
        {
            get => _mSLoginStatus; set => SetProperty(ref _mSLoginStatus, value); }
        
        private string _mSLoginStatus = "Log In";
        
        
        public string SFlightStatus {
            get => _mSFlightStatus;
            private set => SetProperty(ref _mSFlightStatus, value); }
        
        private string _mSFlightStatus = "Not Connected";
        public string SConnectButtonLabel
        {
            get => _mSConnectButtonLabel;
            private set => SetProperty(ref _mSConnectButtonLabel, value);
        }

        private string _mSConnectButtonLabel = "Connect To Sim";

        public bool BObjectIdSelectionEnabled
        {
            get => _mBObjectIdSelectionEnabled;
            set => SetProperty(ref _mBObjectIdSelectionEnabled, value);
        }

        private bool _mBObjectIdSelectionEnabled = false;

        public SIMCONNECT_SIMOBJECT_TYPE ESimObjectType
        {
            get => _mESimObjectType;
            set
            {

                SetProperty(ref _mESimObjectType, value);
                BObjectIdSelectionEnabled = (_mESimObjectType != SIMCONNECT_SIMOBJECT_TYPE.USER);
                ClearResquestsPendingState();
            }
        }

        private SIMCONNECT_SIMOBJECT_TYPE _mESimObjectType = SIMCONNECT_SIMOBJECT_TYPE.USER;
        public ObservableCollection<uint> LObjectIDs { get; }

        public uint ObjectIdRequest
        {
            get => _mIObjectIdRequest;
            set
            {
                SetProperty(ref _mIObjectIdRequest, value);
                ClearResquestsPendingState();
            }
        }

        private uint _mIObjectIdRequest = 0;


        public string[] ASimvarNames => SimVars.Names;

        public string SSimvarRequest
        {
            get => _mSSimvarRequest;
            set => this.SetProperty(ref _mSSimvarRequest, value);
        }

        private string _mSSimvarRequest = null;


        public string[] AUnitNames => Units.Names;

        public string SUnitRequest
        {
            get => _mSUnitRequest;
            set => SetProperty(ref _mSUnitRequest, value);
        }

        private string _mSUnitRequest = null;

        public string SSetValue
        {
            get => _mSSetValue;
            set => SetProperty(ref _mSSetValue, value);
        }

        private string _mSSetValue = null;

        public ObservableCollection<SimvarRequest> LSimvarRequests { get; }

        public SimvarRequest OSelectedSimvarRequest
        {
            get => _mOSelectedSimvarRequest;
            set => SetProperty(ref _mOSelectedSimvarRequest, value);
        }

        private SimvarRequest _mOSelectedSimvarRequest;
        
        
        public bool BFsXcompatible
        {
            get => _mBFsXcompatible;
            set => SetProperty(ref _mBFsXcompatible, value);
        }

        private bool _mBFsXcompatible = true;

        public bool BIsString
        {
            get => _mBIsString;
            set => SetProperty(ref _mBIsString, value);
        }

        private bool _mBIsString;

        public bool BOddTick
        {
            get => _mBOddTick;
            set => SetProperty(ref _mBOddTick, value);
        }

        private bool _mBOddTick;

        public ObservableCollection<string> LErrorMessages { get;set; }


        public BaseCommand CmdToggleConnect { get; set; }
        public BaseCommand CmdAddRequest { get;set; }
        public BaseCommand CmdRemoveSelectedRequest { get;set; }
        public BaseCommand CmdTrySetValue { get;set; }


        #endregion

        #region Real time

        private readonly DispatcherTimer _mOTimer = new DispatcherTimer();

        #endregion

        public SimvarsViewModel()
        {
            var firebaseHandler = new FirebaseHandler(this);
            LObjectIDs = new ObservableCollection<uint> { 1 };

            LSimvarRequests = new ObservableCollection<SimvarRequest>();
            LErrorMessages = new ObservableCollection<string>();

            CmdToggleConnect = new BaseCommand((p) => { ToggleConnect(); });
            CmdAddRequest = new BaseCommand((p) =>
            {
                AddRequest( _mSSimvarRequest,
                    SUnitRequest, BIsString);
            });
            CmdRemoveSelectedRequest = new BaseCommand((p) => { RemoveSelectedRequest(); });
            CmdTrySetValue = new BaseCommand((p) => { TrySetValue(); });
            
            _mOTimer.Interval = new TimeSpan(0, 0, 0, 0, 750);
            _mOTimer.Tick += OnTick;
        }

        
        /// <summary>
        /// Connect to flight sim via Sim connect, ConfigIndex of 1 means FSX compatible. Possibly needed for P3D also? 
        /// </summary>
        private void Connect()
        {
            Console.WriteLine(@"Connect");
            SFlightStatus = "Connecting";
            try
            {
                // The constructor is similar to SimConnect_Open in the native API
                _mOSimConnect = new SimConnect("MySimPilot SimConnect", _mHWnd, WmUserSimconnect, null,
                     1);

                // Listen to connect and quit msgs
                _mOSimConnect.OnRecvOpen += SimConnect_OnRecvOpen;
                _mOSimConnect.OnRecvQuit += SimConnect_OnRecvQuit;

                // Listen to exceptions
                _mOSimConnect.OnRecvException += SimConnect_OnRecvException;

                // Catch a simobject data request
                _mOSimConnect.OnRecvSimobjectDataBytype +=
                    SimConnect_OnRecvSimobjectDataBytype;
            }
            catch (COMException ex)
            {
                SFlightStatus = "Failed, try again";
                Console.WriteLine(@"Connection to KH failed: " + ex.Message);
            }
        }

        /// <summary>
        ///Call when connected to FS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine(@"SimConnect_OnRecvOpen");
            Console.WriteLine(@"Connected to KH");

            SConnectButtonLabel = "Disconnect";
            BConnected = true;
            SFlightStatus = "Connected";
            // Register pending requests
            foreach (var oSimvarRequest in LSimvarRequests)
            {
                if (!oSimvarRequest.BPending) continue;
                oSimvarRequest.BPending = !RegisterToSimConnect(oSimvarRequest);
                oSimvarRequest.BStillPending = oSimvarRequest.BPending;
            }

            _mOTimer.Start();
            BOddTick = false;
        }

        // The case where the user closes game
        private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine(@"SimConnect_OnRecvQuit");
            Console.WriteLine(@"KH has exited");
            SFlightStatus = "Disconnected from sim";
            Disconnect();
        }

        private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            SIMCONNECT_EXCEPTION eException = (SIMCONNECT_EXCEPTION)data.dwException;
            Console.WriteLine(@"SimConnect_OnRecvException: " + eException);

            LErrorMessages.Add("SimConnect : " + eException);
        }

        private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            Console.WriteLine(@"SimConnect_OnRecvSimobjectDataBytype");

            var iRequest = data.dwRequestID;
            var iObject = data.dwObjectID;
            if (!LObjectIDs.Contains(iObject))
            {
                LObjectIDs.Add(iObject);
            }

            foreach (var oSimvarRequest in LSimvarRequests)
            {
                if (iRequest != (uint)oSimvarRequest.ERequest ||
                    (BObjectIdSelectionEnabled && iObject != _mIObjectIdRequest)) continue;
                if (oSimvarRequest.BIsString)
                {
                    var result = (SimVarStruct)data.dwData[0];
                    oSimvarRequest.DValue = 0;
                    oSimvarRequest.SValue = result.sValue;
                }
                else
                {
                    var dValue = (double)data.dwData[0];
                    oSimvarRequest.DValue = dValue;
                    oSimvarRequest.SValue = dValue.ToString("F9");
                }

                oSimvarRequest.BPending = false;
                oSimvarRequest.BStillPending = false;
            }
        }

        // May not be the best way to achive regular requests.
        // See SimConnect.RequestDataOnSimObject
        private void OnTick(object sender, EventArgs e)
        {
            Console.WriteLine(@"OnTick");

            BOddTick = !BOddTick;

            foreach (var oSimvarRequest in LSimvarRequests)
            {
                if (!oSimvarRequest.BPending)
                {
                    _mOSimConnect?.RequestDataOnSimObjectType(oSimvarRequest.ERequest, oSimvarRequest.EDef, 0,
                        _mESimObjectType);
                    oSimvarRequest.BPending = true;
                }
                else
                {
                    oSimvarRequest.BStillPending = true;
                }
            }
        }

        private void ToggleConnect()
        {
            if (_mOSimConnect == null)
            {
                try
                {
                    Connect();
                }
                catch (COMException ex)
                {
                    Console.WriteLine(@"Unable to connect to KH: " + ex.Message);
                }
            }
            else
            {
                Disconnect();
            }
        }

        private void ClearResquestsPendingState()
        {
            foreach (var oSimvarRequest in LSimvarRequests)
            {
                oSimvarRequest.BPending = false;
                oSimvarRequest.BStillPending = false;
            }
        }

        private bool RegisterToSimConnect(SimvarRequest oSimvarRequest)
        {
            if (_mOSimConnect != null)
            {
                if (oSimvarRequest.BIsString)
                {
                    // Define a data structure containing string value
                    _mOSimConnect.AddToDataDefinition(oSimvarRequest.EDef, oSimvarRequest.SName, "",
                        SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    // IMPORTANT: Register it with the simconnect managed wrapper marshaller
                    // If you skip this step, you will only receive a uint in the .dwData field.
                    _mOSimConnect.RegisterDataDefineStruct<SimVarStruct>(oSimvarRequest.EDef);
                }
                else
                {
                    // Define a data structure containing numerical value
                    _mOSimConnect.AddToDataDefinition(oSimvarRequest.EDef, oSimvarRequest.SName,
                        oSimvarRequest.SUnits, SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    // IMPORTANT: Register it with the simconnect managed wrapper marshaller
                    // If you skip this step, you will only receive a uint in the .dwData field.
                    _mOSimConnect.RegisterDataDefineStruct<double>(oSimvarRequest.EDef);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private void AddRequest(string sNewSimvarRequest, string sNewUnitRequest, bool bIsString)
        {
            Console.WriteLine(@"AddRequest");

            //string sNewSimvarRequest = _sOverrideSimvarRequest != null ? _sOverrideSimvarRequest : ((m_iIndexRequest == 0) ? m_sSimvarRequest : (m_sSimvarRequest + ":" + m_iIndexRequest));
            //string sNewUnitRequest = _sOverrideUnitRequest != null ? _sOverrideUnitRequest : m_sUnitRequest;
            var oSimvarRequest = new SimvarRequest
            {
                EDef = (Definition)_mICurrentDefinition,
                ERequest = (Request)_mICurrentRequest,
                SName = sNewSimvarRequest,
                BIsString = bIsString,
                SUnits = bIsString ? null : sNewUnitRequest
            };

            oSimvarRequest.BPending = !RegisterToSimConnect(oSimvarRequest);
            oSimvarRequest.BStillPending = oSimvarRequest.BPending;

            LSimvarRequests.Add(oSimvarRequest);

            ++_mICurrentDefinition;
            ++_mICurrentRequest;
        }

        private void RemoveSelectedRequest()
        {
            LSimvarRequests.Remove(OSelectedSimvarRequest);
        }

        private void TrySetValue()
        {
            Console.WriteLine(@"TrySetValue");

            if (_mOSelectedSimvarRequest == null || _mSSetValue == null) return;
            if (!_mOSelectedSimvarRequest.BIsString)
            {
                if (double.TryParse(_mSSetValue, NumberStyles.Any, null, out var dValue))
                {
                    _mOSimConnect.SetDataOnSimObject(_mOSelectedSimvarRequest.EDef,
                        SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, dValue);
                }
            }
            else
            {
                var sValueStruct = new SimVarStruct()
                {
                    sValue = _mSSetValue
                };
                _mOSimConnect.SetDataOnSimObject(_mOSelectedSimvarRequest.EDef,
                    SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, sValueStruct);
            }
        }
    }
}