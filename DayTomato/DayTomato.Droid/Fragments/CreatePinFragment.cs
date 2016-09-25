using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace DayTomato.Droid.Fragments
{
    class CreatePinFragment : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.create_pin_fragment, container, false);
            var createPinText = view.FindViewById<TextView>(Resource.Id.create_pin);
            createPinText.Text = "Create Pin Tab";
            return view;
        }
    }
}