using System.Windows.Controls;
using MySimPilot.Handlers;

namespace MySimPilot.Pages
{
    public partial class HomePage : Page
    {
        public HomePage()
        {
            DataContext = DataHandler.GetInstance();
            InitializeComponent();
            BtnConnect.DataContext = SimvarsViewModel.GetInstance();
        }
    }
}