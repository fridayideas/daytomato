using System;
using Android.Content;

namespace DayTomato.Droid.Util
{
    class LayoutManager 
    {
        ISharedPreferences sharePref;
        ISharedPreferencesEditor editor;
        Context context;

        // mode

        //shared preferene file name
        private static string pref_name = "Intro Slider";
        private static string is_first_time_lauch = "thefirst";



        public LayoutManager(Context context)
        {
            this.context = context;
            sharePref = this.context.GetSharedPreferences(pref_name, FileCreationMode.Private);
            editor = sharePref.Edit();
        }

        public void setFirstTimeLauch(bool isFirstTime)
        {
            editor.PutBoolean(is_first_time_lauch, isFirstTime);
            editor.Commit();

        }

        public Boolean isFirstTimeLauch()
        {
            bool isFirstTime = sharePref.GetBoolean(is_first_time_lauch, true);
            return isFirstTime;
        }
    }
}