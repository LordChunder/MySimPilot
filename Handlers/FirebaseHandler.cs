using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using Firebase.Database.Query;
using MySimPilot.Data;
using MySimPilot.Pages;
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
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public string SLoginStatus
        {
            get => _mSLoginStatus;
            private set => SetProperty(ref _mSLoginStatus, value);
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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

        private bool _mBIsLoggedIn;

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
        private FirebaseClient _databaseClient;


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
                    MainWindow.MyPilotPageMenuItem.IsEnabled = false;
                    MainWindow.FlightPageMenuItem.IsEnabled = false;
                });
                return;
            }
            _databaseClient = new FirebaseClient("https://mysimpilot-default-rtdb.firebaseio.com/",new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => User.GetIdTokenAsync()
            });
            
            User = userEventArgs.User;
            SLoginStatus = "Log Out";
            SCurrentUserName = User.Info.Email;
            BIsLoggedIn = true;
    
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.UserMenuItem.Visibility = Visibility.Visible;
                MainWindow.SignUpMenuItem.Visibility = Visibility.Collapsed;
                MainWindow.MyPilotPageMenuItem.IsEnabled = true;
                MainWindow.FlightPageMenuItem.IsEnabled = true;
            });

            PilotHandler.GetInstance().PilotData = GetPilotData().Result;
            Console.WriteLine(@"Logged in as " + User.Info.Email);
        }

        public async Task<UserCredential> SignUpWithEmail(string email, string pass)
        {
            var userCredential = await _client.CreateUserWithEmailAndPasswordAsync(email, pass);
     
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

        private async void SetPilotData()
        {
             await _databaseClient.Child("UserData").Child(User.Uid).PutAsync(PilotHandler.GetInstance().PilotData);
        }

        private async Task<MyPilotData> GetPilotData()
        {
            var data = await _databaseClient.Child("UserData").Child(User.Uid).OnceSingleAsync<MyPilotData>();
            
            return data;
        }

        public void SignOut()
        {
            _client.SignOutAsync();
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            File.Create(Path.Combine(directory, "MySimPilot", "user.dat"));
            if (!(MainWindow.ParentFrame.Content.GetType() == typeof(HomePage)))
            {
                MainWindow.ParentFrame.Content = new HomePage();
            }
        }
    }
}