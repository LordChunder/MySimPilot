using System;
using System.Threading.Tasks;
using System.Windows.Data;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;

namespace MySimPilot
{
    public class FirebaseHandler
    {
        private SimvarsViewModel viewModel;
        public FirebaseHandler(SimvarsViewModel viewModel)
        {
            _instance = this;
            this.viewModel = viewModel;
            Init();
        }  
        private static FirebaseHandler _instance;
        public static FirebaseHandler Instance => _instance;

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
            // WPF:
            UserRepository = new FileUserRepository("FirebaseSample") // persist data into %AppData%\FirebaseSample
        };

        public User User { get; private set; }


        private void Init()
        {
            var client = new FirebaseAuthClient(_config);
            
            client.AuthStateChanged += AuthStateChanged;
        }

        private void AuthStateChanged(object obj, UserEventArgs userEventArgs)
        {
            if (userEventArgs.User == null) return;
            User = userEventArgs.User;
            viewModel.SLoginStatus ="Logged in as "+User.Info.Email;
            Console.WriteLine(@"Logged in as "+User.Info.Email);
        }
    }
}