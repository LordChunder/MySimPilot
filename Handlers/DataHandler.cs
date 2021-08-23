using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using MySimPilot.Data;
using MySimPilot.SimConnect;
using MySimPilot.ViewModel;

namespace MySimPilot.Handlers
{
    public class DataHandler: ObservableObject
    {
        private static DataHandler _instance;

        public static DataHandler GetInstance()
        {
            return _instance ?? (_instance = new DataHandler());
        }

        private DataHandler()
        {
            LMessages = new ObservableCollection<Message>();
        }
        
       public PlaneMetadatas? PlaneMetadata = null;
       public PlaneVariables? PlaneVariables = null;
       public PlaneLandingData? PlaneLandingData = null;
       public SimulationVariables? SimVariables = null;
       public PlaneGaugeStates? PlaneGaugeStates = null;
       
       
       
       [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
       // ReSharper disable once CollectionNeverQueried.Global
       public ObservableCollection<Message> LMessages { get; set; }
       
       [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
       public bool BSimVarTickOdd
       {
           get => _mBSimVarTickOdd;
           internal set => SetProperty(ref _mBSimVarTickOdd, value);
       }


       [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
       public bool BConnected
       {
           get => _mBConnected;
           internal set => SetProperty(ref _mBConnected, value);
       }
       [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
       public string SUserAircraftMetaInfo
       {
           get => _mSUserAircraftMetaInfo;
           internal set => SetProperty(ref _mSUserAircraftMetaInfo, value);
       }

       [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
       public string SUserSimulationInfo
       {
           get => _mSUserSimulationInfo;
           internal set => SetProperty(ref _mSUserSimulationInfo, value);
       }
       
       private bool _mBConnected;
    
       private string _mSUserAircraftMetaInfo = "Aircraft:\nSelect an Aircraft";
       private string _mSUserSimulationInfo = "Simulation Settings:\nConnect to Flight Sim";
       private bool _mBSimVarTickOdd;

 
    }
}