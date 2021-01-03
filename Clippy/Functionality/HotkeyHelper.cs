/// Clippy - File: "HotKeyHelper.cs"
/// Copyright © 2021 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Common;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
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

        /// <summary>
        /// Callback for hotkeys
        /// </summary>
        Action<int> _onHotKeyPressed;

        /// <summary>
        /// Unique ID to receive hotkey messages
        /// </summary>
        public short HotkeyID { get; private set; }

        public HotKeyHelper(Window handlerWindow, Action<int> hotKeyHandler)
        {
            _onHotKeyPressed = hotKeyHandler;
            string atomName = Thread.CurrentThread.ManagedThreadId.ToString("X8") + GetType().FullName;
            HotkeyID = GlobalAddAtom(atomName);
            _windowHandle = new WindowInteropHelper(handlerWindow).Handle;

            if (_windowHandle == null)
            {
                throw new ApplicationException("Unable to find window handle.");
            }

            var source = HwndSource.FromHwnd(_windowHandle);
            source.AddHook(HotkeyHook);
        }

        /// <summary>
        /// Listen to the given kotkey
        /// </summary>
        public uint ListenForHotKey(Keys key, KeyModifiers modifiers)
        {
            bool isKeyRegisterd = RegisterHotKey(_windowHandle, HotkeyID, (uint)modifiers, (uint)key);
            if (!isKeyRegisterd)
            {
                // Unregister hotkey and try again if first attempts fails
                UnregisterHotKey(IntPtr.Zero, HotkeyID);
                isKeyRegisterd = RegisterHotKey(_windowHandle, HotkeyID, (uint)modifiers, (uint)key);

                if (!isKeyRegisterd)
                {
                    throw new ApplicationException("The hotkey is already in use");
                }
            }

            return (uint)modifiers | (((uint)key) << 16);
        }

        public void Dispose()
        {
            StopListening();
        }

        private void StopListening()
        {
            if (HotkeyID != 0)
            {
                UnregisterHotKey(_windowHandle, HotkeyID);
                GlobalDeleteAtom(HotkeyID);
                HotkeyID = 0;
            }
        }

        private IntPtr HotkeyHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HotkeyID)
            {
                _onHotKeyPressed?.Invoke(lParam.ToInt32());
                handled = true;
            }
            return IntPtr.Zero;
        }

    }
}
