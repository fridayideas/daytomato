using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using DayTomato.Droid.Fragments;

namespace DayTomato.Droid
{
	[Activity (MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
    {
        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

            // Enable Tabbed Navigation
            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

            // Add Tabs
            // Home Tab
            AddTab(Resource.Drawable.ic_home_white_24dp, new HomeFragment());
            // Create Pin Tab
            AddTab(Resource.Drawable.ic_place_white_24dp, new CreatePinFragment());
        }

        /*
        * This method is used to create and add dynamic tab view
        * @Param,
        * tabText: title to be displayed in tab
        * iconResourceId: image/resource id
        * fragment: fragment reference
        */
        void AddTab(int iconResourceId, Fragment fragment)
        {
            var tab = this.ActionBar.NewTab();
            tab.SetIcon(iconResourceId);

            // must set event handler for replacing tabs tab
            tab.TabSelected += delegate (object sender, ActionBar.TabEventArgs e) {
                e.FragmentTransaction.Replace(Resource.Id.main_fragment, fragment);
            };

            this.ActionBar.AddTab(tab);
        }

    }

}



