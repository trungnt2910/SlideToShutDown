using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlideToShutDown
{
    public class SystemUIMessenger
    {
        public const string ActionInvokeGlobalActionsShown = "GLOBAL_ACTIONS_DIALOG_ON_GLOBAL_ACTIONS_SHOWN";
        public const string ActionInvokeGlobalActionsHidden = "GLOBAL_ACTIONS_DIALOG_ON_GLOBAL_ACTIONS_HIDDEN";
        public const string ActionInvokeShutdown = "GLOBAL_ACTIONS_DIALOG_SHUTDOWN";
        public const string ActionXposedLog = "XPOSED_LOG_ACTION";
    }
}