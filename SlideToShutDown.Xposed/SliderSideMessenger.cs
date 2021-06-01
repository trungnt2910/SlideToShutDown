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
    public class SliderSideMessenger
    {
        public const string ActionRequestHide = "SLIDER_REQUEST_HIDE";
    }
}