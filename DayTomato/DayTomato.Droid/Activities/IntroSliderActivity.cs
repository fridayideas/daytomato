using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Text;
using Android.Views;
using Android.Widget;
using DayTomato.Droid.Util;

namespace DayTomato.Droid.Activities
{
    [Activity(Label = "DayTomato", MainLauncher = true, Theme = "@style/MyTheme.Splash",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class IntroSliderActivity : AppCompatActivity
    {
        ViewPager viewPager;
        LinearLayout dotsLayout;
        TextView[] dots;
        public int[] layouts;
        Button btnNext, btnSkip;
        LayoutManager layoutManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            layoutManager = new LayoutManager(this);
            if (!layoutManager.isFirstTimeLauch())
            {
                lauchHomeScreen();
                Finish();
            }

            layoutManager.setFirstTimeLauch(false);
            SetContentView(Resource.Layout.IntroSlider);




            layouts = new int[]
            {
                        Resource.Layout.LayoutSlide1,
                        Resource.Layout.LayoutSlide2,
                        Resource.Layout.LayoutSlide3,
                        Resource.Layout.LayoutSlide4
            };

            viewPager = (ViewPager)FindViewById(Resource.Id.viewPager);
            dotsLayout = (LinearLayout)FindViewById(Resource.Id.layoutPanel);
            btnNext = (Button)FindViewById(Resource.Id.btn_next);
            btnSkip = (Button)FindViewById(Resource.Id.btn_skip);

            addDots(0);

            ViewPagerAdapter adapter = new ViewPagerAdapter(layouts);
            viewPager.Adapter = adapter;

            viewPager.PageSelected += ViewPager_PageSelected;
            //viewPager.AddOnPageChangeListener(new ViewPager.IOnPageChangeListener());

            btnNext.Click += (sender, e) =>
            {
                int current = GetItem(+1);
                if (current < layouts.Length)
                    //move to next screen
                    viewPager.CurrentItem = current;
                else
                {
                    //lauch main screen here
                    Intent intent = new Intent(this, typeof(SplashActivity));
                    StartActivity(intent);

                }
            };

            btnSkip.Click += (sender, e) =>
            {
                Intent intent = new Intent(this, typeof(SplashActivity));
                StartActivity(intent);

            };


        }

        void ViewPager_PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            addDots(e.Position);

            //changing the next button text
            // Next or Got it

            if (e.Position == layouts.Length - 1)
            {
                // if it is a last page. make button text to "Got it"
                btnNext.Text = (GetString(Resource.String.start));
                btnSkip.Visibility = ViewStates.Gone;

            }
            else
            {
                // if it is not a last page.
                btnNext.Text = (GetString(Resource.String.next));
                btnSkip.Visibility = ViewStates.Visible;
            }
        }



        private void addDots(int currentPage)
        {
            dots = new TextView[layouts.Length];
            string[] colorsActive = { "#d1395c", "#14a895", "#2278d4", "#a854d4" };
            string[] colorsInactive = { "#f98da5", "#8cf9eb", "#93c6fd", "#e4b5fc" };


            dotsLayout.RemoveAllViews();
            for (int i = 0; i < dots.Length; i++)
            {
                dots[i] = new TextView(this);
                dots[i].Text = ("\u2022");
                dots[i].TextSize = 35;
                dots[i].SetTextColor(Color.ParseColor(colorsActive[currentPage]));
                dotsLayout.AddView(dots[i]);
            }

            if (dots.Length > 0)
            {
                dots[currentPage].SetTextColor(Color.ParseColor(colorsInactive[currentPage]));
            }
        }

        int GetItem(int i)
        {
            return viewPager.CurrentItem + i;
        }

        private void lauchHomeScreen()
        {
            layoutManager.setFirstTimeLauch(false);
            Intent intent = new Intent(this, typeof(SplashActivity));
            StartActivity(intent);
            Finish();
        }

        public class ViewPagerAdapter : PagerAdapter
        {
            LayoutInflater layoutInflater;
            int[] _layout;

            public ViewPagerAdapter(int[] layout)
            {
                _layout = layout;
            }

            public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
            {
                layoutInflater = (LayoutInflater)Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService);
                View view = layoutInflater.Inflate(_layout[position], container, false);
                container.AddView(view);

                return view;
            }

            public override int Count
            {
                get
                {
                    return _layout.Length;
                }
            }

            public override bool IsViewFromObject(View view, Java.Lang.Object objectValue)
            {
                return view == objectValue;
            }

            public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object objectValue)
            {
                View view = (View)objectValue;

                container.RemoveView(view);
            }
        }
    }
}
