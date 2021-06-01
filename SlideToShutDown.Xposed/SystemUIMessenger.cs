using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DE.Robv.Android.Xposed;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DE.Robv.Android.Xposed.XC_MethodHook;

namespace SlideToShutDown.Xposed
{
    class SystemUIMessenger : BroadcastReceiver
    {
        public const string ActionInvokeGlobalActionsShown = "GLOBAL_ACTIONS_DIALOG_ON_GLOBAL_ACTIONS_SHOWN";
        public const string ActionInvokeGlobalActionsHidden = "GLOBAL_ACTIONS_DIALOG_ON_GLOBAL_ACTIONS_HIDDEN";
        public const string ActionInvokeShutdown = "GLOBAL_ACTIONS_DIALOG_SHUTDOWN";
        public const string ActionXposedLog = "XPOSED_LOG_ACTION";

        public static bool IsActivityActive { get; private set; }

        Unhook ShowOrHideDialogHook;

        private Java.Lang.Object _windowManagerFuncs;
        private bool _registeredBroadcast;

        public Context Context {get; private set;}

        public SystemUIMessenger(ClassLoader loader)
        {
            var clazz = Class.ForName("com.android.systemui.globalactions.GlobalActionsDialog", true, loader);
            var globalActionsPluginClass = Class.ForName("com.android.systemui.plugins.GlobalActionsPanelPlugin", true, loader);
            var booleanClass = Class.FromType(typeof(Java.Lang.Boolean));

            var method = XposedHelpers.FindMethodBestMatch(clazz, "showOrHideDialog", booleanClass, booleanClass, globalActionsPluginClass);
            ShowOrHideDialogHook = XposedBridge.HookMethod(method, new MethodHook(ShowOrHideDialog));
        }

        public void ShowOrHideDialog(MethodHookParam param)
        {
            XposedBridge.Log("Detected power long press");

            var _this = param.ThisObject;
            Context = (Context)XposedHelpers.GetObjectField(_this, "mContext");
            _windowManagerFuncs = XposedHelpers.GetObjectField(_this, "mWindowManagerFuncs");

            // Silence the default SystemUI stuff. After that, we may still manipulate the states.
            XposedHelpers.CallMethod(_windowManagerFuncs, "onGlobalActionsShown");
            XposedBridge.Log("Global actions shown.");
            XposedHelpers.CallMethod(_windowManagerFuncs, "onGlobalActionsHidden");

            if (!_registeredBroadcast)
            {
                Register();
                _registeredBroadcast = true;
            }

            if (IsActivityActive)
            {
                XposedBridge.Log("Activity is active...");

                RequestHide();

                param.Result = null;
                return;
            }

            // For some reasons, the application started here MUST NOT be the Xposed Module itself.
            // Else, SystemUI will hang itself.
            Intent intent = new Intent(Intent.ActionMain);
            intent.SetClassName("com.trungnt2910.slidetoshutdown", "com.trungnt2910.slidetoshutdown.SliderActivity");
            intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ExcludeFromRecents | ActivityFlags.NoAnimation);

            Context.StartActivity(intent);
            IsActivityActive = true;

            param.Result = null;
        }

        private void Register()
        {
            var filter = new IntentFilter();
            filter.AddAction(ActionInvokeGlobalActionsShown);
            filter.AddAction(ActionInvokeGlobalActionsHidden);
            filter.AddAction(ActionInvokeShutdown);
            filter.AddAction(ActionXposedLog);

            XposedBridge.Log($"Attempting to register broadcast {filter} to {this}");
            XposedBridge.Log($"Using context: {Context}");
            Context.RegisterReceiver(this, filter);
            XposedBridge.Log("Created Broadcast Receiver hoooked to SystemUI");
        }

        public override void OnReceive(Context context, Intent intent)
        {
            XposedBridge.Log("SystemUIMessenger: Recieved broadcast.");
            XposedBridge.Log($"SystemUIMessenger: {intent.Action}");
            if (_windowManagerFuncs == null)
            {
                XposedBridge.Log("WTF: _windowsManagerFuncs NULL???");
                return;
            }
            switch (intent.Action)
            {
                case ActionInvokeGlobalActionsShown:
                {
                    XposedHelpers.CallMethod(_windowManagerFuncs, "onGlobalActionsShown");
                    XposedBridge.Log("SystemUIMessenger: Global actions shown.");
                }
                break;
                case ActionInvokeGlobalActionsHidden:
                {
                    XposedHelpers.CallMethod(_windowManagerFuncs, "onGlobalActionsHidden");
                    XposedBridge.Log("SystemUIMessenger: Global actions hidden.");
                    IsActivityActive = false;
                }
                break;
                case ActionInvokeShutdown:
                {
                    XposedHelpers.CallMethod(_windowManagerFuncs, "shutdown");
                    XposedBridge.Log("SystemUIMessenger: Shutdown.");
                }
                break;
                case ActionXposedLog:
                {
                    XposedBridge.Log($"SystemUIMessenger: SlideToShutDown: {intent.Extras.GetString("Message")}");
                }
                break;
            }
        }

        public void RequestHide()
        {
            Context.SendBroadcast(new Intent(SliderSideMessenger.ActionRequestHide));
        }
    }
}