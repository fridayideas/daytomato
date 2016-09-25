using Android.OS;
using Android.Support.V4.App;
using Android.Views;

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
            return view;
        }
    }
}