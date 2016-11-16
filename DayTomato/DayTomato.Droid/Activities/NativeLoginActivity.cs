using System;
using Android.App;
using Android.Content;
using Android.Gms.Auth;
using Android.Gms.Auth.Api;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.OS;
using Android.Support.V4.App;
using Android.Widget;

namespace DayTomato.Droid.Activities
{
    /// <summary>
    /// Name: Login Activity
    /// Content View: login.axml
    /// 
    /// Description:
    /// This authentication method uses in-app 
    /// authentication where the user can press "Add Account"
    /// and provide consent to supply information to the app.
    /// 
    /// Issues:
    /// -Requires that the OAuth Credential be signed with a fingerprint (unique to dev machine, each dev needs to add
    /// credentials in Google API Console)
    /// 
    /// References: 
    /// https://developer.xamarin.com/samples/monodroid/google-services%5CSigninQuickstart/
    /// https://www.youtube.com/watch?v=3wH-g59JfFY
    /// https://developers.google.com/identity/sign-in/android/start-integrating
    /// </summary>
    [Activity(Label = "DayTomato")]
    public class NativeLoginActivity : FragmentActivity, GoogleApiClient.IOnConnectionFailedListener
    {

        //private SignInButton _signInButton;
        private GoogleApiClient _googleApiClient;
        private readonly string _clientId = "511074657498-04u2ih6kg97s8gh1cl0kmcc7k52v5hrl.apps.googleusercontent.com";

        /// <summary>
        /// Build the Google API Client with the Android.Gms.Auth 
        /// Google Sign In API and assign a click handler for 
        /// the sign in button.
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                .RequestProfile()
                .RequestEmail()
                .RequestIdToken(_clientId)//Allows for acquiring the ID Token
                .Build();
            _googleApiClient = new GoogleApiClient.Builder(this)
                .EnableAutoManage(this, this)
                .AddApi(Auth.GOOGLE_SIGN_IN_API, gso)
                .Build();

            //Fire sign in intent
            var signInIntent = Auth.GoogleSignInApi.GetSignInIntent(_googleApiClient);
            StartActivityForResult(signInIntent, 9001);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == 9001)
            {
                var result = Auth.GoogleSignInApi.GetSignInResultFromIntent(data);
                HandleSignInResult(result);
            }

        }

        /// <summary>
        /// Respond to sign in attempt. If successful,
        /// create an account object containing information 
        /// specified in the Google Sign In Options (in OnCreate()).
        /// </summary>
        /// <param name="result"></param>
        private void HandleSignInResult(GoogleSignInResult result)
        {
            if (result.IsSuccess)
            {
                var userAccount = result.SignInAccount;

                //Start MainActivity here
                //Pass account info to MainActivity and start MainActivity
                var mainActivityIntent = new Intent(this, typeof(MainActivity));

                mainActivityIntent.PutExtra("IdToken", userAccount.IdToken);
                mainActivityIntent.PutExtra("DisplayName", userAccount.DisplayName);
                mainActivityIntent.PutExtra("PhotoUrl", userAccount.PhotoUrl.ToString());
                mainActivityIntent.PutExtra("Email", userAccount.Email);

                StartActivity(mainActivityIntent);

            }
            else
            {
                Toast.MakeText(this, "Cannot sign in.",
                    ToastLength.Short).Show();
            }
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            Toast.MakeText(this, "Sign in failed.", ToastLength.Short).Show();
        }

    }
}