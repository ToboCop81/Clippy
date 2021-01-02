/// Clippy - File: "TrayIcon_Api.cs"
/// Copyright © 2020 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace Clippy.UiElements
{
    public partial class TrayIcon
    {
        private const int _mouseGlobal = 14;
        private const int _leftButtonDown = 0x201;
        private const int _rightButtonDown = 0x204;

        private int mouseHookHandle;
        private HookDelegate hook;

        private delegate int HookDelegate(int code, int wParam, IntPtr structPointer);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int hookId, int code, int param, IntPtr dataPointer);


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int SetWindowsHookEx(int hookId, HookDelegate function, IntPtr instance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int UnhookWindowsHookEx(int hookId);

        private static Point GetHitPoint(IntPtr structPointer)
        {
            MouseLLHook mouseHook = (MouseLLHook)Marshal.PtrToStructure(structPointer, typeof(MouseLLHook));
            return new Point(mouseHook.X, mouseHook.Y);
        }

        private static Rect GetContextMenuLocation(ContextMenu menu)
        {
            var topLeft = menu.PointToScreen(new Point(0, 0));
            var bottomRight = menu.PointToScreen(new Point(menu.ActualWidth, menu.ActualHeight));
            return new Rect(topLeft, bottomRight);
        }

        partial void AttachContextMenu()
        {
            ContextMenu.Opened += OnContextMenuOpened;
            ContextMenu.Closed += OnContextMenuClosed;
        }

        partial void InitializeNativeHooks()
        {
            hook = OnMouseEventProc;
        }

        private void OnContextMenuClosed(object sender, RoutedEventArgs e)
        {
            UnhookWindowsHookEx(mouseHookHandle);

            ContextMenu.Opened -= OnContextMenuOpened;
            ContextMenu.Closed -= OnContextMenuClosed;
        }

        private void OnContextMenuOpened(object sender, RoutedEventArgs e)
        {
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                mouseHookHandle = SetWindowsHookEx(
                    _mouseGlobal, hook, GetModuleHandle(module.ModuleName), 0);
            }

            if (mouseHookHandle == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private int OnMouseEventProc(int code, int button, IntPtr dataPointer)
        {
            if (button == _leftButtonDown || button == _rightButtonDown)
            {
                var contextMenuRect = GetContextMenuLocation(ContextMenu);
                var hitPoint = GetHitPoint(dataPointer);

                if (!contextMenuRect.Contains(hitPoint))
                {
                    ContextMenu.IsOpen = false;
                }
            }

            return CallNextHookEx(mouseHookHandle, code, button, dataPointer);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MouseLLHook
        {
            internal int X;
            internal int Y;
            internal int MouseData;
            internal int Flags;
            internal int Time;
            internal int ExtraInfo;
        }
    }
}