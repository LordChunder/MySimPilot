using System.Windows.Controls;
using MySimPilot.Handlers;

namespace MySimPilot.Pages
{
    public partial class FlightPage : Page
    {
        public FlightPage()
        {
            InitializeComponent();

            LblAircraftInfo.DataContext = DataHandler.GetInstance();
        }
    }
}