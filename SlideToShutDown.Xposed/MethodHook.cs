using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DE.Robv.Android.Xposed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlideToShutDown.Xposed
{
    public class MethodHook : XC_MethodHook
    {
        private Action<MethodHookParam> _before;
        private Action<MethodHookParam> _after;

        public MethodHook(Action<MethodHookParam> before = null, Action<MethodHookParam> after = null)
        {
            _before = before;
            _after = after;
        }

        protected override void BeforeHookedMethod(MethodHookParam param)
        {
            _before?.Invoke(param);
            base.BeforeHookedMethod(param);
        }

        protected override void AfterHookedMethod(MethodHookParam param)
        {
            _after?.Invoke(param);
            base.AfterHookedMethod(param);
        }
    }
}