using System.Windows.Controls;
using MySimPilot.Handlers;

namespace MySimPilot.Pages
{
    public partial class MyPilotPage : Page
    {
        public MyPilotPage()
        {
            DataContext = PilotHandler.GetInstance();
            InitializeComponent();
        }
    }
}