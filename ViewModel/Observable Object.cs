using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MySimPilot.ViewModel
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string sPropertyName = null)
        {
            PropertyChangedEventHandler hEventHandler = this.PropertyChanged;
            if (hEventHandler != null && !string.IsNullOrEmpty(sPropertyName))
            {
                hEventHandler(this, new PropertyChangedEventArgs(sPropertyName));
            }
        }

        protected bool SetProperty<T>(ref T tField, T tValue, [CallerMemberName] string sPropertyName = null)
        {
            return this.SetProperty(ref tField, tValue, out T tPreviousValue, sPropertyName);
        }

        protected bool SetProperty<T>(ref T tField, T tValue, out T tPreviousValue, [CallerMemberName] string sPropertyName = null)
        {
            if (!object.Equals(tField, tValue))
            {
                tPreviousValue = tField;
                tField = tValue;
                this.OnPropertyChanged(sPropertyName);
                return true;
            }

            tPreviousValue = default(T);
            return false;
        }
    }
}