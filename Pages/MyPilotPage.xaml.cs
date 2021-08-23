using MySimPilot.Handlers;

namespace MySimPilot.Pages
{
    public partial class MyPilotPage
    {
        public MyPilotPage()
        {
            DataContext = PilotHandler.GetInstance();
            InitializeComponent();
        }
    }
}