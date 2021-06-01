using Android;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Google.Android.Material.Snackbar;
using System;
using System.Threading.Tasks;

namespace SlideToShutDown.Xposed
{
    public class Wallpaper
    {
        private TaskCompletionSource<bool> _tcs;
        private bool? _permission;
        private Activity _activity;
        public Wallpaper(AppCompatActivity activity)
        {
            _activity = activity;
            var status = ContextCompat.CheckSelfPermission(activity, Manifest.Permission.ReadExternalStorage);
            _tcs = new TaskCompletionSource<bool>();
            if (status == Permission.Denied)
            {
                if (ActivityCompat.ShouldShowRequestPermissionRationale(activity, Manifest.Permission.ReadExternalStorage))
                {
                    // Provide an additional rationale to the user if the permission was not granted
                    // and the user would benefit from additional context for the use of the permission.
                    // For example if the user has previously denied the permission.

                    Snackbar.Make(activity.Window.DecorView,
                                   "Please let this application read the external storage to display your lock screen wallpaper",
                                   Snackbar.LengthLong)
                            .Show();
                }
                var launcher = activity.RegisterForActivityResult(new ActivityResultContracts.RequestPermission(), new Callback(_tcs));
                launcher.Launch(Manifest.Permission.ReadExternalStorage);
                // This is EVIL, but we MUST do this as Android only let us register for permissions on Startup.
            }
            else
            {
                _permission = true;
            }
        }

        public async Task<Drawable> GetDrawableAsync()
        {
            _permission = _permission ?? await _tcs.Task;
            WallpaperManager wallpaperManager = WallpaperManager.GetInstance(_activity);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                ParcelFileDescriptor pfd = wallpaperManager.GetWallpaperFile(WallpaperManagerFlags.Lock);
                if (pfd == null)
                    pfd = wallpaperManager.GetWallpaperFile(WallpaperManagerFlags.System);
                if (pfd != null)
                {
                    var result = new BitmapDrawable(_activity.Resources, BitmapFactory.DecodeFileDescriptor(pfd.FileDescriptor));

                    try
                    {
                        pfd.Close();
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }

                    return result;
                }
            }
            return wallpaperManager.Drawable;
        }

        public Matrix GetWallpaperMatrix(float screenHeight, float screenWidth, float wallpaperHeight)
        {
            System.Diagnostics.Debug.WriteLine($"Android version: {Build.VERSION.SdkInt}");

            // Android default behaviour: Scales the height so the wallpaper fits the screeen,
            // and get the leftmost part.
            var matrix = new Matrix();
            var scale = screenHeight / wallpaperHeight;
            matrix.SetScale(scale, scale);

            // Dummy Xamarin developers decided to use code "10000" for version R.
            // This will be fixed when updated to Xamarin.Android 11.3
            if (Build.VERSION.SdkInt > BuildVersionCodes.Q)
            {
                System.Diagnostics.Debug.WriteLine("Using Android 11 zoomed wallpapers");
                // Stupid Android 11 zoomed wallpaper feature.
                // https://www.reddit.com/r/GooglePixel/comments/ip5yif/disabling_wallpaper_zoom_on_home_screen_android_11/g4ifsqe?utm_source=share&utm_medium=web2x&context=3
                matrix.PostScale(1.1f, 1.1f);
                matrix.PostTranslate(screenWidth * -0.05f, 0);
            }

            return matrix;
        }

        private class Callback : Java.Lang.Object, IActivityResultCallback
        {
            TaskCompletionSource<bool> _tcs;
            public Callback(TaskCompletionSource<bool> tcs)
            {
                _tcs = tcs;
            }

            public void OnActivityResult(Java.Lang.Object p0)
            {
                try
                {
                    var boolean = p0.JavaCast<Java.Lang.Boolean>();
                    _tcs.SetResult(boolean.BooleanValue());
                }
                catch (Exception e)
                {
                    _tcs.SetException(e);
                }
            }
        }
    }
}