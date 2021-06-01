using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Android.App;
using Android.Runtime;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: MetaData("xposedmodule", Value = "true")]
[assembly: MetaData("xposeddescription", Value = "SlideToShutDown Xposed Bindings")]
[assembly: MetaData("xposedminversion", Value = "82")]
[assembly: MetaData("xposedscope", Resource = "@array/appScope")]
[assembly: NamespaceMapping(Java = "com.trungnt2910.slidetoshutdown.xposed", Managed = "SlideToShutDown.Xposed")]

[assembly: AssemblyTitle("Slide To Shut Down")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("SlideToShutDown.Xposed")]
[assembly: AssemblyCopyright("Copyright © 2021 Trung Nguyen")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
