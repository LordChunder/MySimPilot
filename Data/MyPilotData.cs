using MySimPilot.ViewModel;

namespace MySimPilot.Data
{
    public class MyPilotData: ObservableObject
    {
        public LogBook LogBook;

        public MyPilotData()
        {
            LogBook = new LogBook();
        }
    }
}