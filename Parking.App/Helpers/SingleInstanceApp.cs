using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Parking.App.Helpers;

public class SingleInstanceApp
{
    private static Mutex? _mutex;
    private const string MutexName = "Global\\Parking.App";

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private static MainWindow? _mainWindow;

    private const int SW_RESTORE = 9;
    private const int SW_SHOW = 5;

    public static bool IsFirstInstance()
    {
        _mutex = new Mutex(true, MutexName, out bool isNewInstance);
        return isNewInstance;
    }

    public static void SetMainWindow(MainWindow window)
    {
        _mainWindow = window;
    }
    public static void ActivatePreviousInstance()
    {
        try
        {

            if (_mainWindow != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindow.Show();
                    _mainWindow.WindowState = WindowState.Normal;
                    _mainWindow.Activate();
                });
            }
            else
            {
                // Fallback to process-based activation if window reference is not available
                Process current = Process.GetCurrentProcess();
                foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                {
                    if (process.Id != current.Id)
                    {
                        ShowWindow(process.MainWindowHandle, SW_SHOW);
                        ShowWindow(process.MainWindowHandle, SW_RESTORE);
                        SetForegroundWindow(process.MainWindowHandle);
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            //MessageBox.Show($"Error activating previous instance: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static void Cleanup()
    {
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        _mainWindow = null;
    }
}
