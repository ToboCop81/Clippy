/// Clippy - File: "ClippySettings.cs"
/// Copyright © 2018 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Functionality;
using System;
using System.Windows;

namespace Clippy.Common
{
    public sealed class ClippySettings
    {
        private static ClippySettings s_instance;
        private bool m_initialized = false;
        private bool m_autoSave;
        private bool m_textNameFromContent;
        private bool m_saveWindowLayout;
        private bool m_useClipboardFiles;
        private bool m_allowEmptyClipboardFiles;
        private bool m_mainWindowAlwaysOnTop;
        private double m_FontSize;
        private string m_clipboardTextFileName;
        private int m_clipboardTextFileEncoding;

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

            /// <summary>
            /// Count of settings. Increase this value if a new setting is added
            /// </summary>
            public const int Count = 17;
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
            get { return m_autoSave; }
            set
            {
                m_autoSave = value;
                if (m_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.AutoSave, m_autoSave);
                }
            }
        }

        public bool SaveWindowLayoutState
        {
            get { return m_saveWindowLayout; }
            set
            {
                m_saveWindowLayout = value;
                if (m_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.SaveWindow, m_saveWindowLayout);
                }
            }
        }

        public bool TextItemNameFromContent
        {
            get { return m_textNameFromContent; }
            set
            {
                m_textNameFromContent = value;
                if (m_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.TextNameFromContent, m_textNameFromContent);
                }
            }
        }

        public bool UseClipboardFiles
        {
            get { return m_useClipboardFiles; }
            set
            {
                m_useClipboardFiles = value;
                if (m_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.UseClipboardFiles, m_useClipboardFiles);
                }
            }
        }

        public bool AllowEmptyClipboardFiles
        {
            get { return m_allowEmptyClipboardFiles; }
            set
            {
                m_allowEmptyClipboardFiles = value;
                if (m_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.EmptyClipboardFileAllowed, m_allowEmptyClipboardFiles);
                }
            }
        }

        public bool MainWindowAlwaysOnTop
        {
            get { return m_mainWindowAlwaysOnTop; }
            set
            {
                m_mainWindowAlwaysOnTop = value;
                if (m_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.MainWindowAlwaysOnTop, m_mainWindowAlwaysOnTop);
                }
            }
        }

        public string ClipboardTextFileName
        {
            get { return m_clipboardTextFileName; }
            set
            {
                m_clipboardTextFileName = value;
                if (m_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.ClipboardTextFileName, m_clipboardTextFileName);
                }
            }
        }

        public int ClipboardTextFileEncoding
        {
            get { return m_clipboardTextFileEncoding; }
            set
            {
                m_clipboardTextFileEncoding = value;
                if (m_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.ClipboardTextFileEncoding, m_clipboardTextFileEncoding);
                }
            }
        }

        public double FontSize
        {
            get { return m_FontSize; }
            set
            {
                m_FontSize = value;
                if (m_initialized)
                {
                    SettingsManager.Instance.UpdateSetting(SettingNames.PreviewFontSize, m_FontSize);
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

            m_initialized = true;

            LoadDefaultSettings();

        }

        public void SaveAllSettings()
        {
            SettingsManager.Instance.SaveSettings();
        }

        public void SaveWindowLayout(Window window)
        {
            if (!m_initialized || window == null) { return; }

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
            if (!m_initialized || window == null) { return; }

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
                m_autoSave = (bool)SettingsManager.Instance.GetValue(SettingNames.AutoSave);
                m_saveWindowLayout = (bool)SettingsManager.Instance.GetValue(SettingNames.SaveWindow);
                m_textNameFromContent = (bool)SettingsManager.Instance.GetValue(SettingNames.TextNameFromContent);
                m_FontSize = (double)SettingsManager.Instance.GetValue(SettingNames.PreviewFontSize);
                m_useClipboardFiles = (bool)SettingsManager.Instance.GetValue(SettingNames.UseClipboardFiles);
                m_clipboardTextFileName = (string)SettingsManager.Instance.GetValue(SettingNames.ClipboardTextFileName);
                m_clipboardTextFileEncoding = (int)SettingsManager.Instance.GetValue(SettingNames.ClipboardTextFileEncoding);
                m_allowEmptyClipboardFiles = (bool)SettingsManager.Instance.GetValue(SettingNames.EmptyClipboardFileAllowed);
                m_mainWindowAlwaysOnTop = (bool)SettingsManager.Instance.GetValue(SettingNames.MainWindowAlwaysOnTop);

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
