using System.Windows;
using MySimPilot.Handlers;

namespace MySimPilot.ViewModel
{
    public partial class LoginWindow
    {
    
        public LoginWindow()
        {
        
            InitializeComponent();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var pass = TxtPasswordInput.Password;
            var email = TxtEmailInput.Text;

            if (email.Contains(" ") || !email.Contains("@") || email.Length == 0)
            {
                MessageBox.Show("Invalid Email");
                return;
            } 
            if (pass.Length == 0)
            {
                MessageBox.Show("Invalid Password");
                return;
            } 
            await FirebaseHandler.GetInstance().LoginWithEmail(email, pass).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    MessageBox.Show("Could not sign in with those credentials");
                    
                }else if (task.IsCompleted)
                {
                    if (task.Result.User.Uid == null)
                    {
                        MessageBox.Show("User not found");
                      
                    }
                    else
                    {
                        MessageBox.Show("Login Success");
       
                        Application.Current.Dispatcher.Invoke(Close);
                    }
                }
               
            });
        }
        
        
    }
}