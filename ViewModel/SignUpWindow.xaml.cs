using System.Windows;
using MySimPilot.Handlers;

namespace MySimPilot.ViewModel
{
    public partial class SignUpWindow : Window
    {
        public SignUpWindow()
        {
            InitializeComponent();
        }


        private void btnSignUp_Click(object sender, RoutedEventArgs e)
        {
            var pass = txtPasswordInput.Password;
            var passConfirm = txtPasswordInput.Password;
            var email = txtEmailInput.Text;

            if (email.Contains(" ") || !email.Contains("@") || email.Length == 0)
            {
                MessageBox.Show("Invalid email");
                return;
            }

            if (pass.Length == 0)
            {
                MessageBox.Show("Invalid password");
                return;
            }

            if (passConfirm.Length == 0 || !passConfirm.Equals(pass))
            {
                MessageBox.Show("Passwords do not match");
                return;
            }

            FirebaseHandler.GetInstance().SignUpWithEmail(email, pass).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    MessageBox.Show("Could not sign up with those credentials");
                }
                else if (task.IsCompleted)
                {
                    MessageBox.Show("Sign Up Success");
                    Application.Current.Dispatcher.Invoke(Close);
                }
            });
        }
    }
}