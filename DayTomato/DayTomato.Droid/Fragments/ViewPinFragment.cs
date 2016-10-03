﻿
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace DayTomato.Droid
{
	public class ViewPinFragment : Fragment
	{
		public static ViewPinFragment NewInstance(Bundle bundle)
		{
			ViewPinFragment fragment = new ViewPinFragment();
			fragment.Arguments = bundle;
			return fragment;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			return base.OnCreateView(inflater, container, savedInstanceState);
		}
	}
}
