﻿
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Content;
using System.Threading.Tasks;
using DayTomato.Droid.Activities;

namespace DayTomato.Droid
{

    [Activity(
        Theme = "@style/MyTheme.Splash",
       // MainLauncher = true,
        NoHistory = true,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashActivity : AppCompatActivity
	{
		public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
		{
			base.OnCreate(savedInstanceState, persistentState);
		}

		protected override void OnResume()
		{
			base.OnResume();

			Task startupWork = new Task(() =>
			{
				Task.WaitAll(); // Let application startup
			});

			startupWork.ContinueWith(t =>
			{
				// Should get users location here and other long loading things
				StartActivity(new Intent(Application.Context, typeof(NativeLoginActivity)));
			}, 
            TaskScheduler.FromCurrentSynchronizationContext());
			startupWork.Start();
		}
	}
}

