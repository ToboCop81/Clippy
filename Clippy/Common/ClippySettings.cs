using Clippy.Functionality;
using System.Windows;

namespace Clippy.Common
{
    public class ClippySettings
    {
        private static ClippySettings s_instance;
        private bool m_initialized = false;
        private bool m_autoSave;
        private bool m_textNameFromContent;
        private bool m_saveWindowLayout;
        private double m_FontSize;

        /// <summary>
        /// Count of settings. Increase this value if a new setting is added
        /// </summary>
        const int m_settingsCount = 12;

        #region settingNames
        const string AutoSave = "Asv";
        const string TextNameFromContent = "TxtNfC";
        const string SaveWindow = "SvWnd";
        const string WindowLeft = "Left";
        const string WindowTop = "Top";
        const string WindowWidth = "Width";
        const string WindowHeight = "Height";
        const string MainWindowName = "ClippyMainWindow";
        const string PreviewWindowName = "ContentPreviewWindow";
        const string PreviewFontSize = "PreviewFontSize";
        #endregion

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
                    SettingsManager.Instance.UpdateSetting(AutoSave, m_autoSave);
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
                    SettingsManager.Instance.UpdateSetting(SaveWindow, m_saveWindowLayout);
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
                    SettingsManager.Instance.UpdateSetting(TextNameFromContent, m_textNameFromContent);
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
                    SettingsManager.Instance.UpdateSetting(PreviewFontSize, m_FontSize);
                }
            }
        }

        public void InitializeSettings()
        {
            SettingsManager.Instance.Initialize();

            // Initialize default settings when no settings file was found
            if (SettingsManager.Instance.SettingsCount != m_settingsCount)
            {
                AddDefaultSettings();
            }

            m_initialized = true;
            m_autoSave = (bool)SettingsManager.Instance.GetValue(AutoSave);
            m_saveWindowLayout = (bool)SettingsManager.Instance.GetValue(SaveWindow);
            m_textNameFromContent = (bool)SettingsManager.Instance.GetValue(TextNameFromContent);
            m_FontSize = (double)SettingsManager.Instance.GetValue(PreviewFontSize);
        }

        public void SaveAllSettings()
        {
            SettingsManager.Instance.SaveSettings();
        }

        private void AddDefaultSettings()
        {
            SettingsManager.Instance.Initialize(forceNew: true);

            SettingsManager.Instance.AddSetting(AutoSave, true);
            SettingsManager.Instance.AddSetting(TextNameFromContent, true);
            SettingsManager.Instance.AddSetting(SaveWindow, true);
            SettingsManager.Instance.AddSetting(MainWindowName + WindowLeft, (double)-1);
            SettingsManager.Instance.AddSetting(MainWindowName + WindowTop, (double)-1);
            SettingsManager.Instance.AddSetting(MainWindowName + WindowWidth, (double)230);
            SettingsManager.Instance.AddSetting(MainWindowName + WindowHeight, (double)387);
            SettingsManager.Instance.AddSetting(PreviewWindowName + WindowLeft, (double)-1);
            SettingsManager.Instance.AddSetting(PreviewWindowName + WindowTop, (double)-1);
            SettingsManager.Instance.AddSetting(PreviewWindowName + WindowWidth, (double)400);
            SettingsManager.Instance.AddSetting(PreviewWindowName + WindowHeight, (double)450);
            SettingsManager.Instance.AddSetting(PreviewFontSize, (double)12);
        }

        public void SaveWindowLayout(Window window)
        {
            if (!m_initialized || window == null) { return; }

            double left = window.Left;
            double top = window.Top;
            
            if (ValidateWindowPosition(window, top, left))
            {
                SettingsManager.Instance.UpdateSetting(window.Name + WindowLeft, left);
                SettingsManager.Instance.UpdateSetting(window.Name + WindowTop, top);
            }

            double width = window.Width;
            double height = window.Height;

            if (ValidateWindowSize(window, height, width))
            {
                SettingsManager.Instance.UpdateSetting(window.Name + WindowWidth, window.Width);
                SettingsManager.Instance.UpdateSetting(window.Name + WindowHeight, window.Height);
            }
        }

        public void RestoreWindowLayout(Window window)
        {
            if (!m_initialized || window == null) { return; }

            double top = (double)SettingsManager.Instance.GetValue(window.Name + WindowTop);
            double left = (double)SettingsManager.Instance.GetValue(window.Name + WindowLeft);

            if (top == -1 || left == -1) { return; }

            if (ValidateWindowPosition(window, top, left))
            {
                window.Top = top;
                window.Left = left;
            }

            double width = (double)SettingsManager.Instance.GetValue(window.Name + WindowWidth);
            double height = (double)SettingsManager.Instance.GetValue(window.Name + WindowHeight);

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
    }
}
