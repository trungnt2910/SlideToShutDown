using System;
using System.IO;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using DE.Robv.Android.Xposed;
using DE.Robv.Android.Xposed.Callbacks;
using Android.App;

namespace SlideToShutDown.Xposed
{
    public partial class Main
    {
        /// <summary>
        /// Write your logic here
        /// </summary>
        public class Loader : Java.Lang.Object, IXposedHookLoadPackage, IXposedHookZygoteInit, IXposedHookInitPackageResources
        {
            public string BaseApkPath;
            public string PackageName;
            public bool IsXamarinApp = false;

            private SystemUIMessenger _systemUI;

            public Loader()
            {
                
            }

            public Loader(string baseApkPath, string packageName)
            {
                BaseApkPath = baseApkPath;
                PackageName = packageName;
            }

            /// <summary>
            /// Write your logic here
            /// </summary>
            /// <param name="param"></param>
            public void HandleLoadPackage(XC_LoadPackage.LoadPackageParam param)
            {
                //DetectAndFixXamarinApp(param); //This is required for Xamarin app compatibility
                Log.Info("XamarinPosed", "XamarinPosed HandleLoadPackage: " + param.PackageName);

                switch (param.PackageName.ToLowerInvariant())
                {
                    case "com.android.systemui":
                        _systemUI = new SystemUIMessenger(param.ClassLoader);
                    break;
                    case "com.trungnt2910.slidetoshutdown":
                        DetectAndFixXamarinApp(param);
                        var classLoader = param.ClassLoader;
                        var clazz = Class.ForName("com.trungnt2910.slidetoshutdown.SliderSideMessenger", false, classLoader);

                        XposedHelpers.FindAndHookMethod(clazz, "isXposedModuleInstalled", new MethodHook((param) =>
                        {
                            XposedBridge.Log("SlideToShutDown.Xposed: Hooked method called");
                            param.Result = Java.Lang.Boolean.True;
                        }));

                        XposedBridge.Log("SlideToShutDown.Xposed: Hooked method.");
                    break;
                }
            }

            /// <summary>
            /// Write your logic here
            /// </summary>
            /// <param name="param"></param>
            public void InitZygote(XposedHookZygoteInitStartupParam param)
            {
            }

            /// <summary>
            /// Write your logic here
            /// </summary>
            /// <param name="param"></param>
            public void HandleInitPackageResources(XC_InitPackageResources.InitPackageResourcesParam param)
            {
                XposedBridge.Log("XamarinPosed HandleInitPackageResources: " + param.PackageName);
            }

            private bool DetectAndFixXamarinApp(XC_LoadPackage.LoadPackageParam param)
            {
                var nativeDir = param.AppInfo.NativeLibraryDir;
                if (nativeDir == null)
                {
                    XposedBridge.Log("native dir is null");
                    IsXamarinApp = false;
                    return false;
                }

                try
                {
                    foreach (var file in Directory.EnumerateFiles(nativeDir))
                    {
                        var lib = Path.GetFileName(file);
                        if (lib == "libxamarin-app.so" || lib == "libmono-native.so" || lib == "libmonodroid.so" || lib == "libxamarin-debug-app-helper.so")
                        {
                            XposedBridge.Log("XamarinPosed found Xamarin App: " + param.PackageName);
                            //TODO:
                            //var unhook = XposedHelpers.FindAndHookMethod("android.content.Context", param.ClassLoader, "getClassLoader",
                            //    new Context_GetClassLoaderHook());

                            IsXamarinApp = true;
                            return true;
                        }
                    }
                }
                catch (Java.Lang.Exception e)
                {
                    XposedBridge.Log($"Caught: {e}");
                    XposedBridge.Log("Probably not a Xamarin App: " + param.PackageName);
                    IsXamarinApp = false;
                }

                return false;
            }
        }
    }

    class MethodReplacement : XC_MethodReplacement
    {
        Func<MethodHookParam, Java.Lang.Object> _method;

        protected override Java.Lang.Object ReplaceHookedMethod(MethodHookParam param)
        {
            return _method?.Invoke(param);
        }

        public MethodReplacement(Func<MethodHookParam, Java.Lang.Object> replaced)
        {
            _method = replaced;
        }
    }
}