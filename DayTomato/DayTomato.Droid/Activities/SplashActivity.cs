using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Content;
using System.Threading.Tasks;

namespace DayTomato.Droid
{

  //  [Activity(
		//Theme = "@style/MyTheme.Splash", 
		//MainLauncher = true, 
		//NoHistory = true)]
    [Activity(
        Theme = "@style/MyTheme.Splash",
        NoHistory = true)]
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
				StartActivity(new Intent(Application.Context, typeof(MainActivity)));
			}, 
            TaskScheduler.FromCurrentSynchronizationContext());
			startupWork.Start();
		}
	}
}

