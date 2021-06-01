//by Ulysses, wdwxy12345@gmail.com
package xamarin.posed;

import java.util.Locale;
import java.io.File;
import java.io.*;
import mono.android.BuildConfig;
import mono.android.DebugRuntime;
import mono.android.Runtime;
import mono.android.app.ApplicationRegistration;
import mono.android.app.NotifyTimeZoneChanges;
import mono.MonoPackageManager_Resources;
import android.os.Build;
import android.os.Environment;
import android.util.Log;

public class XamarinPosedLoader
	//extends java.lang.Object
	implements
		//mono.android.IGCUserPeer,
		de.robv.android.xposed.IXposedHookLoadPackage,
		de.robv.android.xposed.IXposedHookZygoteInit,
		de.robv.android.xposed.IXposedHookInitPackageResources
{
/** @hide */
	public com.trungnt2910.slidetoshutdown.xposed.Main_Loader _loader;
	public boolean isInited = false;
	static {}

	public XamarinPosedLoader ()
	{
		super ();
	}

	public void handleLoadPackage (de.robv.android.xposed.callbacks.XC_LoadPackage.LoadPackageParam p0)
	{
		if (isInited && _loader != null)
		{
			_loader.handleLoadPackage(p0);
		}
	}

	public void initZygote (de.robv.android.xposed.IXposedHookZygoteInit.StartupParam p0) throws IOException
	{
		if (!isInited)
		{
			String modulePath = p0.modulePath; // /data/user/0/io.va.exposed/virtual/data/app/{package}/base.apk
			Locale locale = Locale.getDefault();
			String localeStr = locale.getLanguage() + "-" + locale.getCountry();

			Path currentModulePath = Paths.get(modulePath);
			String parent = currentModulePath.getParent().getFileName().toString();
			String packageName = parent;
			int subPos = parent.lastIndexOf("-");
			if (subPos > 0)
			{
				packageName = parent.substring(0, subPos);
			}
			Log.i("XamarinPosed", "packageName: " + packageName);

			File externalStorageDirectory = Environment.getExternalStorageDirectory();
			File filesDirFile = new File(externalStorageDirectory, "Android/data/" + packageName + "/files");
			File cachesDirFile = new File(externalStorageDirectory, "Android/data/" + packageName + "/files/cache");
			cachesDirFile.mkdirs();
			String filesDir = filesDirFile.getAbsolutePath();
			Log.i("XamarinPosed", "filesDir: " + filesDir);
			//String filesDir = context.getFilesDir().getAbsolutePath(); // /data/user/0/io.va.xposed/virtual/data/user/0/{package}/

			String cacheDir = cachesDirFile.getAbsolutePath();
			//String cacheDir = context.getCacheDir().getAbsolutePath(); // filesDir + "cache"
			String dataAppDir = modulePath.substring(0, modulePath.lastIndexOf("/"));
			String nativeLibraryPath = dataAppDir + "/lib"; // getNativeLibraryPath(context);
			//tring nativeLibraryPath = getNativeLibraryPath(context); //{baseApkDir}/../lib
			
			File nativeLibraryPathFile = new File(nativeLibraryPath);			
			for (File f : nativeLibraryPathFile.listFiles())
			{
				if(f.isDirectory())
				{
					nativeLibraryPath = f.getAbsolutePath();
					break;
				}
			}

			ClassLoader classLoader = this.getClass().getClassLoader();
			//TODO: hook context.getClassLoader() and replaced to this classLoader
			//ClassLoader classLoader = de.robv.android.xposed.XposedBridge.BOOTCLASSLOADER;
			String externalOverrridePath = new File(externalStorageDirectory, "Android/data/" + packageName + "/files/.__override__").getAbsolutePath();
			String externalOverrridePathLegacy = new File(externalStorageDirectory, "../legacy/Android/data/" + packageName + "/files/.__override__").getAbsolutePath();

			String nativeLibraryPath2 = nativeLibraryPath; //getNativeLibraryPath(applicationInfo);

			String[] sourceDirs = new String[1]; //append ApplicationInfo.splitPublicSourceDirs if needed
			sourceDirs[0] = modulePath;

			String[] initParams = {filesDir, cacheDir, nativeLibraryPath};
			String[] externalOverrrideParams = {externalOverrridePath, externalOverrridePathLegacy}; //deprecated

			String cloneLibraryPath = dataAppDir + "/libclone";
			if (!Path.exist(cloneLibraryPath))
			{
				Runtime.getRuntime().exec(new String[] {"su", "-c", "cp -f -r " + dataAppDir + "/lib" + " " + cloneLibraryPath } );
			}

			File cloneLibraryPathFile = new File(cloneLibraryPath);			
			for (File f : cloneLibraryPathFile.listFiles())
			{
				if(f.isDirectory())
				{
					cloneLibraryPath = f.getAbsolutePath();
					break;
				}
			}

			initParams[2] = cloneLibraryPath;

			Log.i("XamarinPosed", "nativeLibraryPath: " + cloneLibraryPath);
			String nativeLibraryPath3 = cloneLibraryPath + "/";
			try
			{
				if (BuildConfig.Debug) 
				{
					System.load(nativeLibraryPath3 + "libxamarin-debug-app-helper.so");
					//DebugRuntime.init(sourceDirs, nativeLibraryPath2, initParams, externalOverrrideParams);
					DebugRuntime.init(sourceDirs, nativeLibraryPath2, initParams);
				} 
				else 
				{
					System.load(nativeLibraryPath3 + "libmonosgen-2.0.so");
				}
			}
			catch (UnsatisfiedLinkError e) 
			{
				Log.e("XamarinPosed", "Failed to load mono lib, could be architecture mismatch (64bit module vs 32bit app or vice versa)", e);
				isInited = false;
				return;
			}

			System.load(nativeLibraryPath3 + "libxamarin-app.so");
			try 
			{
				System.load(nativeLibraryPath3 + "libmono-native.so");
			} 
			catch (UnsatisfiedLinkError e) 
			{
				Log.i("monodroid", "Failed to preload libmono-native.so (may not exist), ignoring", e);
			}

			System.load(nativeLibraryPath3 + "libmonodroid.so");
			System.load(nativeLibraryPath3 + "libmono-btls-shared.so");
			System.load(nativeLibraryPath3 + "libxa-internal-api.so");
			Log.i("XamarinPosed", "load lib done");
			//Runtime.initInternal(localeStr, sourceDirs, nativeLibraryPath2, initParams, classLoader, externalOverrrideParams, MonoPackageManager_Resources.Assemblies, Build.VERSION.SDK_INT, isEmulator());
			Runtime.initInternal(localeStr, sourceDirs, nativeLibraryPath2, initParams, classLoader, MonoPackageManager_Resources.Assemblies, Build.VERSION.SDK_INT, isEmulator());
			ApplicationRegistration.registerApplications();
			Log.i("XamarinPosed", "init internal done");
			
			// /data/data/com.my.app/files
			_loader = new xamarin.posed.Main_Loader();
			//_loader = new xamarin.posed.Main_Loader(modulePath, packageName);
			isInited = true;
		}

		if (isInited && _loader != null)
		{
			_loader.initZygote (p0);
		}
	}

	public void handleInitPackageResources (de.robv.android.xposed.callbacks.XC_InitPackageResources.InitPackageResourcesParam p0)
	{
		if (isInited && _loader != null)
		{
			_loader.handleInitPackageResources(p0);
		}
	}
	
    static boolean isEmulator() {
        String str = Build.HARDWARE;
        return str.contains("ranchu") || str.contains("goldfish");
    }

    private static void copyDirectory(File sourceDir, File destDir) throws IOException {
        if (!destDir.exists()) {
            destDir.mkdir();
        }

        for (String f : sourceDir.list()) {
            File source = new File(sourceDir, f);
            File destination = new File(destDir, f);

            if (source.isDirectory()) {
                copyDirectory(source, destination);
            } else {
                copyFile(source, destination);
            }
        }
    }

    private static void copyFile(File sourceFile, File destinationFile) throws IOException {
        FileInputStream input = new FileInputStream(sourceFile);
		destinationFile.getParentFile().mkdirs();
		destinationFile.createNewFile();
        FileOutputStream output = new FileOutputStream(destinationFile, false);

        byte[] buf = new byte[1024];
        int bytesRead;

        while ((bytesRead = input.read(buf)) > 0) {
            output.write(buf, 0, bytesRead);
        }

        input.close();
        output.close();
    }
}
