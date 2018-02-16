using System;
using System.IO;
using System.Runtime.InteropServices;

namespace VroomJs
{
  public static class AssemblyLoader
  {
    public static object _lock = new object();
    public static bool _isLoaded = false;

    private static readonly string FileExtension = Environment.OSVersion.Platform == PlatformID.Unix ? ".so" : ".dll";

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr LoadLibrary(string lpFileName);

    private static void LoadDllWindows(string dllName, string architecture)
    {
      var dirName = Path.Combine(Path.GetTempPath(), "VroomJs");

      if (!Directory.Exists(dirName))
        Directory.CreateDirectory(dirName);

      dirName = Path.Combine(dirName, architecture);

      if (!Directory.Exists(dirName))
        Directory.CreateDirectory(dirName);

      var dllPath = Path.Combine(dirName, dllName + FileExtension);


      using (Stream stm = typeof(JsEngine).Assembly.GetManifestResourceStream("VroomJs." + dllName + "-" + architecture + FileExtension))
      {
        try
        {
          using (Stream outFile = File.Create(dllPath))
          {
            const int sz = 4096;
            byte[] buf = new byte[sz];
            while (true)
            {
              int nRead = stm.Read(buf, 0, sz);
              if (nRead < 1)
                break;
              outFile.Write(buf, 0, nRead);
            }
          }
        }
        catch
        {
          // This may happen if another process has already created and loaded the file.
          // Since the directory includes the version number of this assembly we can
          // assume that it's the same bits, so we just ignore the excecption here and
          // load the DLL.
        }
      }

      IntPtr h = LoadLibrary(dllPath);
      if (h == IntPtr.Zero)
        throw new Exception("Couldn't load native assembly at " + dllPath);
    }

    public static void EnsureLoaded()
    {
      if (_isLoaded) return;

      lock (_lock)
      {
        if (_isLoaded) return;
        _isLoaded = true;

        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.MacOSX:
            LoadDllMac();
            break;
          case PlatformID.Unix:
            LoadDllLinux();
            break;
          default:
            if (Environment.Is64BitOperatingSystem)
            {
              LoadDllWindows("v8", "x64");
              LoadDllWindows("VroomJsNative", "x64");

            }
            else
            {
              LoadDllWindows("v8", "x86");
              LoadDllWindows("VroomJsNative", "x86");
            }
            break;
        }
      }
    }

    private static void LoadDllLinux()
    {
      if (Environment.Is64BitOperatingSystem)
      {
      }
    }

    private static void LoadDllMac()
    {
      throw new NotImplementedException();
    }
  }
}
