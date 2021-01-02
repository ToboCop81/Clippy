/// Clippy - File: "ClippySettings.cs"
/// Copyright © 2020 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Functionality;
using System;
using System.Windows;

namespace Clippy.Common
{
    public sealed class ClippySettings
    {
        private static ClippySettings s_instance;
        private bool _initialized = false;
        private bool _autoSave;
        private bool _textNameFromContent;
        private bool _saveWindowLayout;
        private bool _useClipboardFiles;
        private bool _allowEmptyClipboardFiles;
        private bool _mainWindowAlwaysOnTop;
        private bool _showIconInSystemTray;
        private double _FontSize;
        private string _clipboardTextFileName;
        private int _clipboardTextFileEncoding;

        public static class SettingNames
        {
            public const string AutoSave = "AutoSave";
            public const string TextNameFromContent = "TextNameFromContent";
            public const string SaveWindow = "SaveWindow";
            public const string WindowLeft = "Left";
            public const string WindowTop = "Top";
            public const string WindowWidth = "Width";
            public const string WindowHeight = "Height";
            public const string MainWindowName = "ClippyMainWindow";
            public const string PreviewWindowName = "ContentPreviewWindow";
            public const string PreviewFontSize = "PreviewFontSize";
            public const string UseClipboardFiles = "UseClipoardFiles";
            public const string ClipboardTextFileName = "ClipboardTextFileName";
            public const string ClipboardTextFileEncoding = "ClipboardTextFileEncoding";
            public const string EmptyClipboardFileAllowed = "EmptyClipboardFileAllowed";
            public const string MainWindowAlwaysOnTop = "MainWindowAlwaysOnTop";
            public const string ShowIconInSystemTray = "ShowIconInSystemTray";

            /// <summary>
            /// Count of settings. Increase this value if a new setting is added
            /// </summary>
            public const int Count = 18;
        }

        private ClippySettings() { }

        public static ClippySettings Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new ClippySettings();
                }

                return s_instance;
            }
        }

        public bool AutoSaveState
        {
            get { return _autoSave; }
            set
            {
                _autoSave = value;
                if (_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.AutoSave, _autoSave);
                }
            }
        }

        public bool SaveWindowLayoutState
        {
            get { return _saveWindowLayout; }
            set
            {
                _saveWindowLayout = value;
                if (_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.SaveWindow, _saveWindowLayout);
                }
            }
        }

        public bool TextItemNameFromContent
        {
            get { return _textNameFromContent; }
            set
            {
                _textNameFromContent = value;
                if (_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.TextNameFromContent, _textNameFromContent);
                }
            }
        }

        public bool UseClipboardFiles
        {
            get { return _useClipboardFiles; }
            set
            {
                _useClipboardFiles = value;
                if (_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.UseClipboardFiles, _useClipboardFiles);
                }
            }
        }

        public bool AllowEmptyClipboardFiles
        {
            get { return _allowEmptyClipboardFiles; }
            set
            {
                _allowEmptyClipboardFiles = value;
                if (_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.EmptyClipboardFileAllowed, _allowEmptyClipboardFiles);
                }
            }
        }

        public bool MainWindowAlwaysOnTop
        {
            get { return _mainWindowAlwaysOnTop; }
            set
            {
                _mainWindowAlwaysOnTop = value;
                if (_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.MainWindowAlwaysOnTop, _mainWindowAlwaysOnTop);
                }
            }
        }

        public bool ShowIconInSystemTray
        {
            get { return _showIconInSystemTray; }
            set
            {
                _showIconInSystemTray = value;
                if (_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.ShowIconInSystemTray, _showIconInSystemTray);
                }
            }
        }

        public string ClipboardTextFileName
        {
            get { return _clipboardTextFileName; }
            set
            {
                _clipboardTextFileName = value;
                if (_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.ClipboardTextFileName, _clipboardTextFileName);
                }
            }
        }

        public int ClipboardTextFileEncoding
        {
            get { return _clipboardTextFileEncoding; }
            set
            {
                _clipboardTextFileEncoding = value;
                if (_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.ClipboardTextFileEncoding, _clipboardTextFileEncoding);
                }
            }
        }

        public double FontSize
        {
            get { return _FontSize; }
            set
            {
                _FontSize = value;
                if (_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.PreviewFontSize, _FontSize);
                }
            }
        }

        public void InitializeSettings()
        {
            SettingsManager.Instance.Initialize();

            // Initialize default settings when no settings file was found
            if (SettingsManager.Instance.SettingsCount != SettingNames.Count)
            {
                AddDefaultSettings();
            }

            _initialized = true;

            LoadDefaultSettings();

        }

        public void SaveAllSettings()
        {
            SettingsManager.Instance.SaveSettings();
        }

        public void SaveWindowLayout(Window window)
        {
            if (!_initialized || window == null) { return; }

            double left = window.Left;
            double top = window.Top;
            
            if (ValidateWindowPosition(window, top, left))
            {
                SettingsManager.Instance.UpdateSetting(window.Name + SettingNames.WindowLeft, left);
                SettingsManager.Instance.UpdateSetting(window.Name + SettingNames.WindowTop, top);
            }

            double width = window.Width;
            double height = window.Height;

            if (ValidateWindowSize(window, height, width))
            {
                SettingsManager.Instance.UpdateSetting(window.Name + SettingNames.WindowWidth, window.Width);
                SettingsManager.Instance.UpdateSetting(window.Name + SettingNames.WindowHeight, window.Height);
            }
        }

        public void RestoreWindowLayout(Window window)
        {
            if (!_initialized || window == null) { return; }

            double top = (double)SettingsManager.Instance.GetValue(window.Name + SettingNames.WindowTop);
            double left = (double)SettingsManager.Instance.GetValue(window.Name + SettingNames.WindowLeft);

            if (top == -1 || left == -1) { return; }

            if (ValidateWindowPosition(window, top, left))
            {
                window.Top = top;
                window.Left = left;
            }

            double width = (double)SettingsManager.Instance.GetValue(window.Name + SettingNames.WindowWidth);
            double height = (double)SettingsManager.Instance.GetValue(window.Name + SettingNames.WindowHeight);

            if (ValidateWindowSize(window, height, width))
            {
                window.Width = width;
                window.Height = height;
            }
        }

        private bool ValidateWindowPosition(Window window, double top, double left)
        {
            if (window == null) { return false; }
            if (top < 0 || left < 0) { return false; }
            if (top + window.Height > SystemParameters.VirtualScreenHeight) { return false; }
            if (left + window.Width > SystemParameters.VirtualScreenWidth) { return false; }

            return true;
        }

        private bool ValidateWindowSize(Window window, double height, double width)
        {
            if (window == null) { return false; }
            if (height < 0 || width < 0) { return false; }
            if (height > window.MaxHeight || width > window.MaxWidth) { return false; }
            if (height < window.MinHeight || width < window.MinWidth) { return false; }
            if (window.Top + height > SystemParameters.VirtualScreenHeight) { return false; }
            if (window.Left + width > SystemParameters.VirtualScreenWidth) { return false; }

            return true;
        }

        private void LoadDefaultSettings()
        {
            try
            {
                _autoSave = (bool)SettingsManager.Instance.GetValue(SettingNames.AutoSave);
                _saveWindowLayout = (bool)SettingsManager.Instance.GetValue(SettingNames.SaveWindow);
                _textNameFromContent = (bool)SettingsManager.Instance.GetValue(SettingNames.TextNameFromContent);
                _FontSize = (double)SettingsManager.Instance.GetValue(SettingNames.PreviewFontSize);
                _useClipboardFiles = (bool)SettingsManager.Instance.GetValue(SettingNames.UseClipboardFiles);
                _clipboardTextFileName = (string)SettingsManager.Instance.GetValue(SettingNames.ClipboardTextFileName);
                _clipboardTextFileEncoding = (int)SettingsManager.Instance.GetValue(SettingNames.ClipboardTextFileEncoding);
                _allowEmptyClipboardFiles = (bool)SettingsManager.Instance.GetValue(SettingNames.EmptyClipboardFileAllowed);
                _mainWindowAlwaysOnTop = (bool)SettingsManager.Instance.GetValue(SettingNames.MainWindowAlwaysOnTop);
                _showIconInSystemTray = (bool)SettingsManager.Instance.GetValue(SettingNames.ShowIconInSystemTray);
            }
            catch (NullReferenceException)
            {
                throw new InvalidOperationException(SettingsManager.Instance.Status);
            }
        }

        private void AddDefaultSettings()
        {
            SettingsManager.Instance.Initialize(forceNew: true);

            // If a setting is added here it is necessary to increase the settings count (SettingNames.Count)
            SettingsManager.Instance.AddSetting(SettingNames.AutoSave, true);
            SettingsManager.Instance.AddSetting(SettingNames.TextNameFromContent, true);
            SettingsManager.Instance.AddSetting(SettingNames.SaveWindow, true);
            SettingsManager.Instance.AddSetting(SettingNames.UseClipboardFiles, false);
            SettingsManager.Instance.AddSetting(SettingNames.EmptyClipboardFileAllowed, false);
            SettingsManager.Instance.AddSetting(SettingNames.MainWindowAlwaysOnTop, false);
            SettingsManager.Instance.AddSetting(SettingNames.ShowIconInSystemTray, false);
            SettingsManager.Instance.AddSetting(SettingNames.MainWindowName + SettingNames.WindowLeft, (double)-1);
            SettingsManager.Instance.AddSetting(SettingNames.MainWindowName + SettingNames.WindowTop, (double)-1);
            SettingsManager.Instance.AddSetting(SettingNames.MainWindowName + SettingNames.WindowWidth, (double)230);
            SettingsManager.Instance.AddSetting(SettingNames.MainWindowName + SettingNames.WindowHeight, (double)387);
            SettingsManager.Instance.AddSetting(SettingNames.PreviewWindowName + SettingNames.WindowLeft, (double)-1);
            SettingsManager.Instance.AddSetting(SettingNames.PreviewWindowName + SettingNames.WindowTop, (double)-1);
            SettingsManager.Instance.AddSetting(SettingNames.PreviewWindowName + SettingNames.WindowWidth, (double)400);
            SettingsManager.Instance.AddSetting(SettingNames.PreviewWindowName + SettingNames.WindowHeight, (double)450);
            SettingsManager.Instance.AddSetting(SettingNames.PreviewFontSize, (double)12);
            SettingsManager.Instance.AddSetting(SettingNames.ClipboardTextFileName, string.Empty);
            SettingsManager.Instance.AddSetting(SettingNames.ClipboardTextFileEncoding, System.Text.Encoding.UTF8.CodePage);
        }
    }
}
