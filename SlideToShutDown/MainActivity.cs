using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using Google.Android.Material.BottomSheet;
using Android.Widget;
using System.Threading.Tasks;
using Android.Util;
using Java.Lang;
using Android.Content;
using System.Linq;
using Android.Graphics;

namespace SlideToShutDown
{
    [Activity(Name="com.trungnt2910.slidetoshutdown.MainActivity", Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, LaunchMode = Android.Content.PM.LaunchMode.SingleInstance)]
    public class MainActivity : AppCompatActivity
    {
        private Wallpaper _wallpaper;
        private SliderManager _manager;
        private double _screenHeight;
        private double _screenWidth;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            _wallpaper = new Wallpaper(this);
        }

        public override void OnAttachedToWindow()
        {
            var group = (ViewGroup)FindViewById(Android.Resource.Id.Content);
            LayoutInflater.Inflate(Resource.Layout.activity_main, group, true);
        }
    }
}
