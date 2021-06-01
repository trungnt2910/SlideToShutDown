using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Runtime = Java.Lang.Runtime;

namespace SlideToShutDown
{
    [BroadcastReceiver(Name = "com.trungnt2910.slidetoshutdown.SliderSideMessenger")]
    public class SliderSideMessenger : BroadcastReceiver
    {
        public const string ActionRequestHide = "SLIDER_REQUEST_HIDE";
        private SliderActivity _activity;

        public bool IsXposedModuleInstalled
        {
            get
            {
                // We must invoke this method as a Java method.
                // Else, the Xamarin method will be called, bypassing any Xposed hooks.
                var clazz = Class.FromType(typeof(SliderSideMessenger));
                var method = clazz.GetMethod("isXposedModuleInstalled");
                return (method.Invoke(this) as Java.Lang.Boolean).BooleanValue();
            }
        }

        public SliderSideMessenger(SliderActivity activity)
        {
            _activity = activity;
            Register();
        }

        public SliderSideMessenger()
        {

        }

        public override void OnReceive(Context context, Intent intent)
        {
            switch (intent.Action)
            {
                case ActionRequestHide:
                    {
                        _activity.RequestHide();
                    }
                    break;
            }
        }

        private void Register()
        {
            var filter = new IntentFilter();
            filter.AddAction(ActionRequestHide);

            _activity.RegisterReceiver(this, filter);
        }

        public void OnGlobalActionsShown()
        {
            _activity.SendBroadcast(new Intent(SystemUIMessenger.ActionInvokeGlobalActionsShown));
        }

        public void OnGlobalActionsHidden()
        {
            _activity.SendBroadcast(new Intent(SystemUIMessenger.ActionInvokeGlobalActionsHidden));
        }

        public void Shutdown()
        {
            if (!IsXposedModuleInstalled)
            {
                Runtime.GetRuntime().Exec(new string[] { "su", "-c", "svc power shutdown" });
            }
            _activity.SendBroadcast(new Intent(SystemUIMessenger.ActionInvokeShutdown));
        }

        public void Log(string text)
        {
            var intent = new Intent(SystemUIMessenger.ActionXposedLog);
            intent.PutExtra("Message", text);
            _activity.SendBroadcast(intent);
        }

        [Export("isXposedModuleInstalled")]
        public Java.Lang.Boolean IsXposedModuleInstalledQuery()
        {
            return Java.Lang.Boolean.False;
        }
    }
}