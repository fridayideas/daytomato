using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Runtime;
using Android.Support.V4.View;
using DayTomato.Droid.Fragments;
using Java.Lang;
using DayTomato.Services;
using Android.Gms.Maps.Model;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Android.Util;
using DayTomato.Models;
using System;

namespace DayTomato.Droid
{

    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
	public class SplashActivity : AppCompatActivity
	{
		static readonly string TAG = "X:" + typeof(SplashActivity).Name;

		public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
		{
			base.OnCreate(savedInstanceState, persistentState);
			Log.Debug(TAG, "SplashActivity.OnCreate");
		}

		protected override void OnResume()
		{
			base.OnResume();

			Task startupWork = new Task(() =>
										{
											//Thread.Sleep(1000);
											Task.Delay(7500); // Simulate a bit of startup work.
											
										});

			startupWork.ContinueWith(t =>
									 {
										 
							StartActivity(new Intent(Application.Context, typeof(MainActivity)));
									 }, TaskScheduler.FromCurrentSynchronizationContext());

			startupWork.Start();
		}
	}
}

