using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Auth0.SDK;
using Org.W3c.Dom;

namespace DayTomato.Droid.Activities
{
    [Activity(Label = "DayTomato")]
    public class LoginActivity : Activity
    {
        private readonly string _domain = "fridayideas.auth0.com";
        private readonly string _clientId = "EnDUj86KOPtd9B9hBsMEMhDppAuU8778";

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //TODO: Check for internet connection, handle if not available
            //TODO: Auto sign-in without pressing "Sign in as..."

            var auth0 = new Auth0Client(
                _domain,
                _clientId);

            var auth0User = await Login(auth0);

            //Pass account info to MainActivity and start MainActivity
            var mainActivityIntent = new Intent(this, typeof(MainActivity));
            mainActivityIntent.PutExtra("AuthUserJSON", auth0User.Profile.ToString());
            mainActivityIntent.PutExtra("AuthAccessToken", auth0User.Auth0AccessToken);
            mainActivityIntent.PutExtra("AuthIdToken", auth0User.IdToken);
            mainActivityIntent.PutExtra("RefreshToken", auth0User.RefreshToken);

            StartActivity(mainActivityIntent);
        }

        /// <summary>
        /// Usage:
        /// Auth0User user = await Login(AUTH0_CLIENT);
        /// 
        /// Examples:
        /// -get user email => auth0user.Profile["email"].ToString()
        /// -get facebook/google/twitter/etc access token => auth0user.Profile["identities"][0]["access_token"]
        /// -get Windows Azure AD groups => auth0user.Profile["groups"]
        /// 
        /// </summary>
        /// <param name="auth0"></param>
        /// <returns></returns>
        private async Task<Auth0User> Login(Auth0Client auth0)
        {
            // 'this' could be a Context object (Android) or UIViewController, UIView, UIBarButtonItem (iOS)
            return await auth0.LoginAsync(this, withRefreshToken:true);
        }
    }
}