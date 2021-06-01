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
    public class Runnable : Java.Lang.Object, Java.Lang.IRunnable
    {
        Action _action;

        public Runnable(Action action)
        {
            _action = action;
        }

        public void Run()
        {
            _action?.Invoke();
        }
    }
}