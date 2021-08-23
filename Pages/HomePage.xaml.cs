using MySimPilot.Handlers;

namespace MySimPilot.Pages
{
    public partial class HomePage
    {
        public HomePage()
        {
            DataContext = DataHandler.GetInstance();
            InitializeComponent();
            BtnConnect.DataContext = SimvarsViewModel.GetInstance();
        }
    }
}