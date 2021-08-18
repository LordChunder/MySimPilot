using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using MySimPilot.ViewModel;

namespace MySimPilot.Handlers
{
    public class FirebaseHandler : ObservableObject
    {
        private static FirebaseHandler _instance;
        public MainWindow MainWindow;

        public static FirebaseHandler GetInstance()
        {
            return _instance ?? (_instance = new FirebaseHandler());
        }

        private FirebaseHandler()
        {
            Init();
        }

        public string SLoginStatus
        {
            get => _mSLoginStatus;
            private set => SetProperty(ref _mSLoginStatus, value);
        }

        public string SCurrentUserName
        {
            get => _mSCurrentUserName;
            private set => SetProperty(ref _mSCurrentUserName, value);
        }

        private string _mSCurrentUserName;
        private string _mSLoginStatus = "Log In";


        public bool BIsLoggedIn
        {
            get => _mBIsLoggedIn;
            private set => SetProperty(ref _mBIsLoggedIn, value);
        }

        private bool _mBIsLoggedIn = false;

        // Configure...
        private readonly FirebaseAuthConfig _config = new FirebaseAuthConfig
        {
            ApiKey = "AIzaSyAj5gt2mDza4hmEObWcGge37e19117vapo",
            AuthDomain = "mysimpilot.firebaseapp.com",
            Providers = new FirebaseAuthProvider[]
            {
                // Add and configure individual providers
                new EmailProvider()
                // ...
            },
        };

        private FirebaseAuthClient _client;


        public User User { get; private set; }




        private void Init()
        {
            _client = new FirebaseAuthClient(_config);
            _client.AuthStateChanged += AuthStateChanged;


            var directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            try
            {
                var fs = File.OpenRead(Path.Combine(directory, "MySimPilot", "user.dat"));

                var sr = new StreamReader(fs);
                var email = sr.ReadLine();
                var pass = sr.ReadLine();

                _client.SignInWithEmailAndPasswordAsync(email, pass);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void AuthStateChanged(object obj, UserEventArgs userEventArgs)
        {
            if (userEventArgs.User == null)
            {
                BIsLoggedIn = false;
                SLoginStatus = "Log In";
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.UserMenuItem.Visibility = Visibility.Collapsed;
                    MainWindow.SignUpMenuItem.Visibility = Visibility.Visible;
                });
                return;
            }
            
            User = userEventArgs.User;
            SLoginStatus = "Log Out";
            SCurrentUserName = User.Info.Email;
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.UserMenuItem.Visibility = Visibility.Visible;
                MainWindow.SignUpMenuItem.Visibility = Visibility.Collapsed;
            });

            BIsLoggedIn = true;


            Console.WriteLine(@"Logged in as " + User.Info.Email);
        }

        public async Task<UserCredential> SignUpWithEmail(string email, string pass)
        {
            var userCredential = await _client.CreateUserWithEmailAndPasswordAsync(email, pass);
            EmailProvider.
            return userCredential;
        }

        public async Task<UserCredential> LoginWithEmail(string email, string pass)
        {
            var userCredential = await _client.SignInWithEmailAndPasswordAsync(email, pass);
            if (userCredential.User == null) return userCredential;

            var directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var creds = new[] { email, pass };

            using (var fs = File.Create(Path.Combine(directory, "MySimPilot", "user.dat")))
            {
                var streamWriter = new StreamWriter(fs);
                await streamWriter.WriteLineAsync(creds[0]);
                await streamWriter.WriteLineAsync(creds[1]);
                streamWriter.Close();
            }

            return userCredential;
        }

        public void SignOut()
        {
            _client.SignOutAsync();
        }
    }
}