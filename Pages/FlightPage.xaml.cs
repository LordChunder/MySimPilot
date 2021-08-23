using System;
using System.Windows;
using System.Windows.Input;
using GMap.NET;
using MySimPilot.Handlers;
using MySimPilot.SimConnect;

namespace MySimPilot.Pages
{
    public partial class FlightPage
    {
        private bool _snapMapToAircraft = true;
        private bool _rotateMapWithPlane;
        public FlightPage()
        {
            DataContext = FlightHandler.GetInstance();
            InitializeComponent();
            LblAircraftInfo.DataContext = DataHandler.GetInstance();
          
            var mapUpdate = FlightHandler.GetInstance().MapUpdate;
            mapUpdate.Tick += OnTick;
            if (mapUpdate.IsEnabled) return;
            mapUpdate.Interval = new TimeSpan(0, 0, 0, 1, 0);
            mapUpdate.Start();
        }


        private void BtnEngineFail(object sender, RoutedEventArgs e)
        {
            FlightHandler.FailGauge(new[] { FailableGauge.VaccuumInstruments });
        }

        private void OnTick(object sender, EventArgs e)
        {
            NoPositionDataMsg.Visibility = DataHandler.GetInstance().PlaneVariables.HasValue
                ? Visibility.Collapsed
                : Visibility.Visible;
            
            if (_snapMapToAircraft)
                MapView.Position = FlightHandler.GetInstance().LatLongPlanePosition;
            if (_rotateMapWithPlane)
            {
                MapView.Bearing = (float)FlightHandler.GetInstance().DAircraftHeadingTrue;
            }
        }

        private void mapView_Loaded(object sender, RoutedEventArgs e)
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            // choose your provider here
            MapView.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            MapView.MinZoom = 2;
            MapView.MaxZoom = 17;
            // whole world zoom
            MapView.Zoom = 10;
            // lets the map use the mousewheel to zoom
            MapView.MouseWheelZoomType = MouseWheelZoomType.ViewCenter;

            // lets the user drag the map
            MapView.CanDragMap = true;
            // lets the user drag the map with the left mouse button
            MapView.DragButton = MouseButton.Left;

            MapView.OnMapDrag += () =>
            {
                _snapMapToAircraft = false;
            };
        }

        private void SnapToAircraft(object sender, RoutedEventArgs e)
        {
            _snapMapToAircraft = true;
            MapView.Position = FlightHandler.GetInstance().LatLongPlanePosition;
        }

        private void ToggleMapRotation(object sender, RoutedEventArgs e)
        {
            _rotateMapWithPlane = !_rotateMapWithPlane;
            if(!_rotateMapWithPlane)
                MapView.Bearing = 0;
        }
    }
}