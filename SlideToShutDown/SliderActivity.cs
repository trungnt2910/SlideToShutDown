using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.Snackbar;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlideToShutDown
{
    [Activity(Name = "com.trungnt2910.slidetoshutdown.SliderActivity", Label = "SliderActivity", Theme = "@style/AppTheme.Transparent", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, LaunchMode = Android.Content.PM.LaunchMode.SingleInstance, MainLauncher = false)]
    public partial class SliderActivity : AppCompatActivity
    {
        public static bool IsActivityActive { get; private set; }

        private Wallpaper _wallpaper;
        private SliderManager _manager;
        private SliderSideMessenger _messenger;
        private double _screenHeight;
        private double _screenWidth;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Error("SlideToShutDown", "Activity created.");
            System.Diagnostics.Debug.WriteLine("OnCreate");

            if (Intent.HasExtra("exit"))
            {
                // finish immediately
                Finish();
                return;
            }

            IsActivityActive = true;

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            _wallpaper = new Wallpaper(this);
            _messenger = new SliderSideMessenger(this);

            _messenger.Log("Activity created.");

            HideStatusBar();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                Window.SetFlags(WindowManagerFlags.LayoutNoLimits, WindowManagerFlags.LayoutNoLimits);
            }

            var metrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetRealMetrics(metrics);
            _screenHeight = metrics.HeightPixels;
            _screenWidth = metrics.WidthPixels;

            SetShowWhenLocked(true);
        }

        public override async void OnAttachedToWindow()
        {
            await ShowSliderAsync();
        }

        private async Task ShowSliderAsync()
        {
            var group = (ViewGroup)FindViewById(Android.Resource.Id.Content);
            var sliderBackgroundView = LayoutInflater.Inflate(Resource.Layout.slider_background, group, true) as LinearLayout;
            var sliderView = LayoutInflater.Inflate(Resource.Layout.slider, group, false) as FrameLayout;

            var background = sliderView.FindViewById<ImageView>(Resource.Id.lockScreenBackground);

            var drawable = await _wallpaper.GetDrawableAsync();
            background.SetImageDrawable(drawable);

            background.ImageMatrix = _wallpaper.GetWallpaperMatrix((float)_screenHeight, (float)_screenWidth, drawable.IntrinsicHeight);

            System.Diagnostics.Debug.WriteLine(background.ImageMatrix.ToShortString());

            System.Diagnostics.Debug.WriteLine(_screenHeight);
            sliderView.SetY(-(float)_screenHeight);
            group.AddView(sliderView);

            _manager = new SliderManager(sliderView, _screenHeight, 0.5, 0.5, 0.8, 15000);
            _manager.Slided += (sender, args) =>
            {
                try
                {
                    _messenger.Shutdown();
                }
                catch (Java.IO.IOException)
                {
                    System.Diagnostics.Debug.WriteLine("Device not rooted.");
                    Snackbar.Make(sliderView, "Failed to shut down. Device isn't rooted.", Snackbar.LengthLong).Show();
                }
            };
            _manager.Canceled += (sender, args) =>
            {
                _messenger.OnGlobalActionsHidden();
                FinishAndRemoveTaskPortable();
            };
            System.Diagnostics.Debug.WriteLine("Showing...");
            _messenger.Log("Showing...");
            _messenger.OnGlobalActionsShown();
            _manager.Show(_screenHeight / 50.0);
            _messenger.Log("Showed power slider.");
        }

        private void HideStatusBar()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBean)
            {
                Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            }
            else
            {
                var decorView = Window.DecorView;
                decorView.SystemUiVisibility = (StatusBarVisibility)(
                    SystemUiFlags.Immersive |
                    SystemUiFlags.LayoutStable |
                    SystemUiFlags.LayoutHideNavigation |
                    SystemUiFlags.LayoutFullscreen |
                    SystemUiFlags.HideNavigation |
                    SystemUiFlags.Fullscreen);


                // Remember that you should never show the action bar if the
                // status bar is hidden, so hide that too if necessary.
                ActionBar?.Hide();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnUserLeaveHint()
        {
            base.OnUserLeaveHint();
            _manager?.Hide();
            _messenger.OnGlobalActionsHidden();
            FinishAndRemoveTaskPortable();
        }

        // https://stackoverflow.com/a/50745444/14009285
        private void FinishAndRemoveTaskPortable()
        {
            IsActivityActive = false;
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                Intent clearIntent = new Intent(this, GetType());
                clearIntent.AddFlags(ActivityFlags.NewTask |
                                     ActivityFlags.ClearTask |
                                     ActivityFlags.ExcludeFromRecents);
                clearIntent.PutExtra("exit", true);
                StartActivity(clearIntent);
            }
            else
            {
                FinishAndRemoveTask();
            }

            // To kill the Mono runtime and debugger.
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public void RequestHide()
        {
            _manager.HideAsync().ContinueWith((task) =>
            {
                _messenger.OnGlobalActionsHidden();
                FinishAndRemoveTaskPortable();
            });
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent keyEvent)
        {
            if (new[] { Keycode.AppSwitch, Keycode.Back }.Contains(keyCode))
            {
                RequestHide();
                return true;
            }
            else
            {
                return base.OnKeyDown(keyCode, keyEvent);
            }
        }
    }

}