using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

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
            var firebaseHandler = FirebaseHandler.Instance;
            
            InitializeComponent();
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

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var sText = e.Text;
            foreach (var _ in sText.Where(c => !(('0' <= c && c <= '9') || c == '+' || c == '-' || c == ',')))
            {
                e.Handled = true;
                break;
            }
        }
    }
}
