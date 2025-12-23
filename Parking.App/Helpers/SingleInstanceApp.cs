using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Runtime.InteropServices;

namespace Parking.App.Helpers
{
    public static class SingleInstanceApp
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

        /// <summary>
        /// Checks if this is the first instance of the app.
        /// </summary>
        public static bool IsFirstInstance()
        {
            _mutex = new Mutex(true, MutexName, out bool isNewInstance);
            return isNewInstance;
        }

        /// <summary>
        /// Sets the reference to the main window for single-instance restore.
        /// </summary>
        public static void SetMainWindow(MainWindow window)
        {
            _mainWindow = window;
        }

        /// <summary>
        /// Activates the previous instance if another is running.
        /// Restores the hidden main window if available.
        /// </summary>
        public static void ActivatePreviousInstance()
        {
            try
            {
                if (_mainWindow != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Restore if minimized
                        if (_mainWindow.WindowState == WindowState.Minimized)
                            _mainWindow.WindowState = WindowState.Normal;

                        // Show if hidden
                        if (_mainWindow.Visibility != Visibility.Visible)
                            _mainWindow.Show();

                        // Bring to front
                        _mainWindow.Activate();
                        _mainWindow.Topmost = true;  // optional to force focus
                        _mainWindow.Topmost = false;
                    });
                }
                else
                {
                    // Fallback to process-based activation
                    var current = Process.GetCurrentProcess();
                    foreach (var process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id && process.MainWindowHandle != IntPtr.Zero)
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
                Debug.WriteLine($"Error activating previous instance: {ex.Message}");
            }
        }


        /// <summary>
        /// Releases the mutex and clears references.
        /// </summary>
        public static void Cleanup()
        {
            try
            {
                _mutex?.ReleaseMutex();
                _mutex?.Dispose();
            }
            catch { /* ignore */ }

            _mainWindow = null;
        }
    }
}
