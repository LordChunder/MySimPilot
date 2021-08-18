using System;
using System.Windows;
using System.Windows.Interop;
using MySimPilot.Handlers;
using MySimPilot.ViewModel;

namespace MySimPilot
{
    internal interface IBaseSimConnectWrapper
    {
        int GetUserSimConnectWinEvent();
        void ReceiveSimConnectMessage();
        void SetWindowHandle(IntPtr hWnd);
        void Disconnect();
    }

    public partial class MainWindow
    {
        public MainWindow()
        {
            DataContext = new SimvarsViewModel();

   
            InitializeComponent();
            LoginButton.DataContext = FirebaseHandler.GetInstance();
            UserMenuItem.DataContext = FirebaseHandler.GetInstance();
            FirebaseHandler.GetInstance().MainWindow = this;
        }

        private HwndSource GetHWinSource()
        {
            return PresentationSource.FromVisual(this) as HwndSource;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            GetHWinSource().AddHook(WndProc);
            if (DataContext is IBaseSimConnectWrapper oBaseSimConnectWrapper)
            {
                oBaseSimConnectWrapper.SetWindowHandle(GetHWinSource().Handle);
            }
        }

        private IntPtr WndProc(IntPtr hWnd, int iMsg, IntPtr hWParam, IntPtr hLParam, ref bool bHandled)
        {
            if (!(DataContext is IBaseSimConnectWrapper oBaseSimConnectWrapper)) return IntPtr.Zero;
            try
            {
                if (iMsg == oBaseSimConnectWrapper.GetUserSimConnectWinEvent())
                {
                    oBaseSimConnectWrapper.ReceiveSimConnectMessage();
                }
            }
            catch
            {
                oBaseSimConnectWrapper.Disconnect();
            }

            return IntPtr.Zero;
        }

        private void MnuLogin(object sender, RoutedEventArgs e)
        {
            if (FirebaseHandler.GetInstance().BIsLoggedIn)
            {
                FirebaseHandler.GetInstance().SignOut();
                MessageBox.Show("Logged Out");
                return;
            }

            var modalWindow = new LoginWindow();
            modalWindow.ShowDialog();
            
            
        }

        private void MnuSignUp(object sender, RoutedEventArgs e)
        {
            var modalWindow = new SignUpWindow();
            modalWindow.ShowDialog();
        }
    }
}