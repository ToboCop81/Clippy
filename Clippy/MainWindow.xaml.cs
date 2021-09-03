/// Clippy - File: "MainWindow.xaml.cs"
/// Copyright © 2021 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Common;
using Clippy.Functionality;
using Clippy.Interfaces;
using Clippy.UiElements;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Clippy
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _hasStartupArgs;
        private bool _isInitializing;
        private string[] _startupArgs;
        private SettingsWindow _settingsWindow;
        private TermsOfUseWindow _termsOfUseWindow;

        private AsyncPipeServer _pipeServer;
        
        public delegate void ServerMessageInvoker(string content);

        public MainWindow(bool hasStartupArgs = false, string[] startupArgs = null)
        {
            _isInitializing = true;
            _hasStartupArgs = hasStartupArgs;
            _startupArgs = startupArgs;
            _pipeServer = new AsyncPipeServer();
            _pipeServer.MessageRecieved += new ServerMessage(MessageRecievedHandler);          
            InitializeComponent();
            ClippySettings.Instance.InitializeSettings();
            ClipDataManager.Instance.ItemsChanged += ItemsChangedHandler;

            SetupWindow();
            Action hotkeyPressed = GlobalHotkeyPressed;
            var interopHelper = new WindowInteropHelper(this);
            interopHelper.EnsureHandle();
            HotKeyHelper.Instance.Initialize(interopHelper.Handle, hotkeyPressed);

            LoadItemsList();
            ApplySettings();
            string pipeName = StaticHelper.GetPipeName();
            _pipeServer.Listen(pipeName);
            _isInitializing = false;
        }

        private void SetupWindow()
        {
            ListBoxClipboardItems.ItemsSource = ClipDataManager.Instance.Items;

            if (ClippySettings.Instance.SaveWindowLayoutState)
            {
                ClippySettings.Instance.RestoreWindowLayout(this);
            }
        }

        private void ApplySettings()
        {
            Topmost = ClippySettings.Instance.MainWindowAlwaysOnTop;
            AlwaysOnTopMenuItem.IsChecked = Topmost;
            if (Topmost && !_isInitializing) Activate();

            bool showIcon = ClippySettings.Instance.ShowIconInSystemTray;
            TrayIcon.Visibility = (showIcon) ? Visibility.Visible : Visibility.Hidden;

            ButtonGetFromFile.Visibility = ClippySettings.Instance.UseClipboardFiles ? Visibility.Visible : Visibility.Collapsed;
            UpdateItemsList();

            try
            {
                var globalHotkey = ClippySettings.Instance.GlobalHotkey;
                if (globalHotkey.IsActive && globalHotkey.Key != Key.None && HotKeyHelper.Instance.IsInitialized)
                {
                    HotKeyHelper.Instance.StopListening();
                    HotKeyHelper.Instance.ListenForHotKey(globalHotkey);
                }
                else if (!globalHotkey.IsActive && HotKeyHelper.Instance.IsListening)
                {
                    HotKeyHelper.Instance.StopListening();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"{Title}: Global hotkey",MessageBoxButton.OK, MessageBoxImage.Error);
                ClippySettings.Instance.GlobalHotkey.IsActive = false;
            }

        }

        private void LoadItemsList()
        {
            if (_hasStartupArgs)
            {
                string startupFilename = String.Join(" ", _startupArgs).Trim();
                bool extOk = startupFilename.ToLower().EndsWith("." + ClipDataManager.Instance.FileExtension);
                if (extOk)
                {
                    ClipDataManager.Instance.LoadList(startupFilename);
                }
            }

            else if (ClippySettings.Instance.AutoSaveState == true)
            {
                ClipDataManager.Instance.AutoLoad();
            }
        }

        private void ShutdownProgram()
        {
            TrayIcon.Visibility = Visibility.Hidden;
            ClippySettings.Instance.SaveAllSettings();
            Environment.Exit(Environment.ExitCode);
        }

        // Event when another instance of clippy was started and sent file name to open
        private void MessageRecievedHandler(string content)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new ServerMessageInvoker(MessageRecievedHandler), content);
                return;
            }

            if (string.IsNullOrEmpty(content)) return;

            content = StaticHelper.Base64Decode(content);
            if (content.ToUpper() == ":BRINGTOFRONT:")
            {
                BringToFront();
                return;
            }

            if (!content.ToLower().EndsWith("." + ClipDataManager.Instance.FileExtension))
            {
                return;
            }

            if (ClipDataManager.Instance.Items.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show(
                   $"The list is not empty. Do you want to load a new list from the file '{content}' and dismiss this one?",
                    $"{Title}: Confirm Loading",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }            
            }

            ClipDataManager.Instance.LoadList(content);
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            ShutdownProgram();
        }

        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsWindow != null) return;

            if (_termsOfUseWindow != null)
            {
                _termsOfUseWindow.Activate();
                return;
            }

            _termsOfUseWindow = new TermsOfUseWindow();
            StaticHelper.CenterOnScreen(_termsOfUseWindow);
            _termsOfUseWindow.ShowDialog();
            _termsOfUseWindow = null;
        }

        private void MenuItemSettings_Click(object sender, RoutedEventArgs e)
        {
            if (_termsOfUseWindow != null) return;

            if (_settingsWindow != null)
            {
                _settingsWindow.Activate();
                return;
            }

            _settingsWindow = new SettingsWindow();
            StaticHelper.CenterOnScreen(_settingsWindow);
            bool? dialogResult = _settingsWindow.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value == true)
            {
                ApplySettings();
            }

            _settingsWindow = null;
        }

        private void MenuItemClear_Click(object sender, RoutedEventArgs e)
        {
            if (ClipDataManager.Instance.Items.Count == 0)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Do you really want to remove all items from the list?",
                $"{Title}: Confirm Clearing",
                MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);

            if (result == MessageBoxResult.Yes)
            {
                ClipDataManager.Instance.ClearList(clearAutoSaveFile: true);
            }
        }

        private void MenuItemClearSelected_Click(object sender, RoutedEventArgs e)
        {
            if (ClipDataManager.Instance.Items.Count == 0)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Do you really want to remove the selected items from the list?",
                $"{Title}: Confirm Removing",
                MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);

            if (result == MessageBoxResult.Yes)
            {
                ClipDataManager.Instance.RemoveSelectedItems();
                GC.Collect();
            }
        }

        private void ButtonGetFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            ClipDataManager.Instance.GetDataFromClipboard();
        }

        private void ButtonGetFromTextFile_Click(object sender, RoutedEventArgs e)
        {
            ClipDataManager.Instance.GetDataFromFile(DataKind.PlainText);
        }

        private void AlwaysOnTopMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AlwaysOnTopMenuItem.IsChecked = !AlwaysOnTopMenuItem.IsChecked;
            ClippySettings.Instance.MainWindowAlwaysOnTop = AlwaysOnTopMenuItem.IsChecked;
            ApplySettings();
        }

        private void ClipboardItemView_ClickHandler(object sender, ItemAction action, ClipboardItemEventArgs e)
        {
            IClipboardItem currentItem = ((FrameworkElement)sender).DataContext as IClipboardItem;
            if (currentItem != null)
            {
                switch (action)
                {
                    case ItemAction.ItemCopy:
                        if (!ClipDataManager.Instance.CopyDataToClipboard(currentItem))
                        {
                            MessageBox.Show(ClipDataManager.Instance.Status,
                                $"{Title}: Copy to clipboard failed",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }

                        break;
                    case ItemAction.ItemFileCopy:
                        if (!ClipDataManager.Instance.WriteDataToFile(currentItem.Index))
                        {
                            MessageBox.Show(
                                "Saving failed: " + Environment.NewLine + ClipDataManager.Instance.Status, Title + " - Save content to file...",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                        break;
                    case ItemAction.ItemEdit:
                        ContentViewWindow contentViewWindow = new ContentViewWindow(currentItem);
                        contentViewWindow.ShowDialog();
                        if (contentViewWindow.ContentChanged)
                        {
                            ListBoxClipboardItems.Items.Refresh();
                        }
                        break;
                    case ItemAction.ItemDelete:
                        ClipDataManager.Instance.RemoveItem(currentItem.Index);
                        break;
                    default:
                        break;
                }
            }
        }

        private void ItemsChangedHandler(ItemsChangeType changeType, ClipboardItemEventArgs e)
        {
            UpdateItemsList();
        }

        private void ClippyMainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F2:
                    HandleSelectedItem(ItemAction.ItemEdit);
                    break;

                case Key.F5:
                    UpdateItemsList();
                    break;

                case Key.F6:
                    HandleSelectedItem(ItemAction.ItemCopy);
                    break;

                case Key.F7:
                    HandleSelectedItem(ItemAction.ItemFileCopy);
                    break;

                case Key.Delete:
                    HandleSelectedItem(ItemAction.ItemDelete);
                    break;

                default:
                    break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
            {
                ClipDataManager.Instance.GetDataFromClipboard();
            }

            if (ClippySettings.Instance.UseClipboardFiles && e.Key == Key.F4)
            {
                ClipDataManager.Instance.GetDataFromFile(DataKind.PlainText);
            }
        }

        private void GlobalHotkeyPressed()
        {
            ClipDataManager.Instance.GetDataFromClipboard();
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            Activate();
        }

        private void ClippyMainWindow_StateChanged(object sender, EventArgs e)
        {
            if (ClippySettings.Instance.ShowIconInSystemTray)
            {
                switch (WindowState)
                {
                    case WindowState.Minimized:
                        ShowInTaskbar = false;
                        break;
                    default:
                        ShowInTaskbar = true;
                        break;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (ClippySettings.Instance.AutoSaveState == true)
            {
                ClipDataManager.Instance.AutoSave();
            }

            ClippySettings.Instance.SaveAllSettings();
            TrayIcon.Dispose();
            HotKeyHelper.Instance.Dispose();
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            if (ClippySettings.Instance.SaveWindowLayoutState == true)
            {
                ClippySettings.Instance.SaveWindowLayout(this);
            }

            _pipeServer.MessageRecieved -= new ServerMessage(MessageRecievedHandler);
            _pipeServer.Shutdown();
            _pipeServer = null;
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (ClipDataManager.Instance.Items.Count == 0)
            {
                MessageBox.Show(
                    "Clipboard items list is empty. Saving is not possible",
                    Title + " - Save items",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }
            string title = $"{Title} - Save clipboard list...";
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            string ext = ClipDataManager.Instance.FileExtension;
            saveFileDialog.Filter = $"Clippy list file (*.{ext})|*.{ext})";
            saveFileDialog.Title = title;

            if (saveFileDialog.ShowDialog() == true && !string.IsNullOrEmpty(saveFileDialog.FileName))
            {
                bool result = ClipDataManager.Instance.SaveList(saveFileDialog.FileName);
                if (result == false)
                {
                    MessageBox.Show(
                        "Saving failed: " + Environment.NewLine + ClipDataManager.Instance.Status,
                        title,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void MenuItemOpenList_Click(object sender, RoutedEventArgs e)
        {
            string title = $"{Title} - Open clipboard list...";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = title;
            string ext = ClipDataManager.Instance.FileExtension;
            openFileDialog.Filter = $"Clippy list file (*.{ext})|*.{ext})";

            if (openFileDialog.ShowDialog() == true && !string.IsNullOrEmpty(openFileDialog.FileName))
            {
                bool result = ClipDataManager.Instance.LoadList(openFileDialog.FileName);
                if (result == false)
                {
                    MessageBox.Show(
                        "Open list file failed: " + Environment.NewLine + ClipDataManager.Instance.Status,
                        title,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void TrayIcon_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BringToFront();
        }

        /// <summary>
        /// Resets the main window to its default state
        /// </summary>
        private void MenuItemResetWindow_click(object sender, RoutedEventArgs e)
        {
            ResetWindow();
        }

        private void UpdateItemsList()
        {
            ListBoxClipboardItems.ItemsSource = ClipDataManager.Instance.Items;
            ListBoxClipboardItems.Items.Refresh();
            ItemCountLabel.Content = ClipDataManager.Instance.Items.Count;
        }

        private void HandleSelectedItem(ItemAction action)
        {
            if (ClipDataManager.Instance.Items.Count == 0 || ListBoxClipboardItems.SelectedIndex == -1) return;

            IClipboardItem selectedItem = ListBoxClipboardItems.SelectedItem as IClipboardItem;
            if (selectedItem == null) return;

            switch (action)
            {
                case ItemAction.ItemCopy:
                    if (!ClipDataManager.Instance.CopyDataToClipboard(selectedItem))
                    {
                        MessageBox.Show(ClipDataManager.Instance.Status,
                            $"{Title}: Copy to clipboard failed",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    break;

                case ItemAction.ItemFileCopy:
                    if (selectedItem.Type == DataKind.PlainText && ClippySettings.Instance.UseClipboardFiles)
                    {
                        if (!ClipDataManager.Instance.WriteDataToFile(selectedItem.Index))
                        {
                            MessageBox.Show(
                                $"Saving failed: {Environment.NewLine}{ClipDataManager.Instance.Status}",
                                $"{Title} - Save content to file...",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                    break;

                case ItemAction.ItemEdit:
                    ContentViewWindow contentViewWindow = new ContentViewWindow(selectedItem);
                    contentViewWindow.ShowDialog();
                    if (contentViewWindow.ContentChanged)
                    {
                        ListBoxClipboardItems.Items.Refresh();
                    }
                    break;

                case ItemAction.ItemDelete:
                    ClipDataManager.Instance.RemoveItem(selectedItem.Index);
                    break;
                default:
                    break;
            }
        }

        private void BringToFront()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            Activate();
        }

        private void ResetWindow()
        {
            WindowState = WindowState.Normal;
            Width = 250;
            Height = 387;
            StaticHelper.CenterOnScreen(this);
            Activate();
        }
    }
}