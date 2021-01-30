/// Clippy - File: "HotKeyHelper.cs"
/// Copyright © 2021 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Resources;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Clippy.Functionality
{
    /// <summary>
    /// Class to register multiple hotkeys
    /// </summary>
    public class HotKeyHelper : IDisposable
    {
        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hwnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32", SetLastError = true)]
        public static extern int UnregisterHotKey(IntPtr hwnd, int id);

        [DllImport("kernel32", SetLastError = true)]
        public static extern short GlobalAddAtom(string lpString);

        [DllImport("kernel32", SetLastError = true)]
        public static extern short GlobalDeleteAtom(short nAtom);

        public const int WM_HOTKEY = 0x312;
        private IntPtr _windowHandle;
        private static HotKeyHelper _instance;

        private  HotKeyHelper()
        {
            IsInitialized = false;
            CurrentHotkey = new GlobalHotkey();
            IsListening = false;
        }

        /// <summary>
        /// Returns the working instance of HotkeyHelper
        /// </summary>
        public static HotKeyHelper Instance
        {
            get
            {
                if (_instance == null) _instance = new HotKeyHelper();
                return _instance;
            }
        }

        /// <summary>
        /// Callback for hotkeys
        /// </summary>
        Action _onHotKeyPressed;

        /// <summary>
        /// Unique ID to receive hotkey messages
        /// </summary>
        public short HotkeyID { get; private set; }

        /// <summary>
        /// Indicates if the HotkeyHelper is initialized
        /// </summary>
        public bool IsInitialized { get; private set; }

        public bool IsListening { get; private set; }

        /// <summary>
        /// The current hotkey which is registered
        /// </summary>
        public GlobalHotkey CurrentHotkey { get; private set; }

        /// <summary>
        /// Initialize the helper before first use
        /// </summary>
        public void Initialize(IntPtr windowHandle, Action hotKeyHandler)
        {
            if (IsInitialized) return;

            _onHotKeyPressed = hotKeyHandler;
            string atomName = Thread.CurrentThread.ManagedThreadId.ToString("X8") + GetType().FullName;
            HotkeyID = GlobalAddAtom(atomName);
        
            if (windowHandle == null)
            {
                throw new ApplicationException("Unable to find window handle.");
            }

            _windowHandle = windowHandle;
            var source = HwndSource.FromHwnd(windowHandle);
            source.AddHook(HotkeyHook);
            IsInitialized = true;
        }

        /// <summary>
        /// Listen to the given kotkey
        /// </summary>
        public void ListenForHotKey(GlobalHotkey globalHotkey)
        {
            if (!IsInitialized) throw new ApplicationException("Hotkey helper is not initialized");
            if (IsListening) return;

            var winFormsKey = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(globalHotkey.Key);
            bool isKeyRegisterd = RegisterHotKey(_windowHandle, HotkeyID, (uint)globalHotkey.Modifiers, (uint)winFormsKey);
            if (!isKeyRegisterd)
            {
                // Unregister hotkey and try again if first attempts fails
                UnregisterHotKey(IntPtr.Zero, HotkeyID);
                isKeyRegisterd = RegisterHotKey(_windowHandle, HotkeyID, (uint)globalHotkey.Modifiers, (uint)winFormsKey);

                if (!isKeyRegisterd) throw new ApplicationException("The hotkey is already in use");
            }

            IsListening = true;
        }

        public void StopListening()
        {
            if (HotkeyID != 0)
            {
                UnregisterHotKey(_windowHandle, HotkeyID);
                GlobalDeleteAtom(HotkeyID);
                HotkeyID = 0;
                CurrentHotkey = new GlobalHotkey();
                IsListening = false;
            }
        }

        public void Dispose()
        {
            StopListening();
            IsListening = false;
            IsInitialized = false;
            _instance = null;
        }

        private IntPtr HotkeyHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HotkeyID)
            {
                _onHotKeyPressed?.Invoke();
                handled = true;
            }
            return IntPtr.Zero;
        }

    }
}
