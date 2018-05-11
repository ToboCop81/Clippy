/// Clippy - File: "MainWindow.xaml.cs"
/// Copyright © 2018 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Common;
using Clippy.Functionality;
using Clippy.Interfaces;
using Clippy.UiElements;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;

namespace Clippy
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool m_hasStartupArgs;
        private string[] m_startupArgs;
        private AsyncPipeServer m_pipeServer;

        public delegate void ServerMessageInvoker(string content);

        public MainWindow(bool hasStartupArgs = false, string[] startupArgs = null)
        {
            m_hasStartupArgs = hasStartupArgs;
            m_startupArgs = startupArgs;
            m_pipeServer = new AsyncPipeServer();
            m_pipeServer.MessageRecieved += new ServerMessage(MessageRecievedHandler);
            InitializeComponent();
            ClippySettings.Instance.InitializeSettings();
            ClipDataManager.Instance.ItemsChanged += ItemsChangedHandler;

            SetupWindow();

            LoadItemsList();
            ApplySettings();
            string pipeName = StaticHelper.GetPipeName();
            m_pipeServer.Listen(pipeName);
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
            ButtonGetFromFile.Visibility = ClippySettings.Instance.UseClipboardFiles ? Visibility.Visible : Visibility.Collapsed;
            UpdateItemsList();
        }

        private void LoadItemsList()
        {
            if (m_hasStartupArgs)
            {
                string startupFilename = String.Join(" ", m_startupArgs).Trim();
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
            bool extOk = content.ToLower().EndsWith("." + ClipDataManager.Instance.FileExtension);
            if (!extOk)
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
            TermsOfUseWindow termsOfUseWindow = new TermsOfUseWindow();
            termsOfUseWindow.Show();
        }

        private void MenuItemSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            bool? dialogResult = settingsWindow.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value == true)
            {
                ApplySettings();
            }
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

        private void Window_Closed(object sender, System.EventArgs e)
        {
            if (ClippySettings.Instance.AutoSaveState == true)
            {
                ClipDataManager.Instance.AutoSave();
            }

            ClippySettings.Instance.SaveAllSettings();
        }

        private void Window_Closing(object sender, System.EventArgs e)
        {
            if (ClippySettings.Instance.SaveWindowLayoutState == true)
            {
                ClippySettings.Instance.SaveWindowLayout(this);
            }

            m_pipeServer.MessageRecieved -= new ServerMessage(MessageRecievedHandler);
            m_pipeServer.Shutdown();
            m_pipeServer = null;
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

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            string ext = ClipDataManager.Instance.FileExtension;
            saveFileDialog.Filter = $"Clippy list file (*.{ext})|*.{ext}";
            saveFileDialog.Title = Title + " - Save clipboard list...";

            if (saveFileDialog.ShowDialog() == true && !string.IsNullOrEmpty(saveFileDialog.FileName))
            {
                bool result = ClipDataManager.Instance.SaveList(saveFileDialog.FileName);
                if (result == false)
                {
                    MessageBox.Show(
                        "Saving failed: " + Environment.NewLine + ClipDataManager.Instance.Status,
                        Title + " - Save clipboard list ...",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void MenuItemOpenList_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = Title + " - Open clipboard list...";
            string ext = ClipDataManager.Instance.FileExtension;
            openFileDialog.Filter = $"Clippy list file (*.{ext})|*.{ext}";

            if (openFileDialog.ShowDialog() == true && !string.IsNullOrEmpty(openFileDialog.FileName))
            {
                bool result = ClipDataManager.Instance.LoadList(openFileDialog.FileName);
                if (result == false)
                {
                    MessageBox.Show(
                        "Open list file failed: " + Environment.NewLine + ClipDataManager.Instance.Status,
                        Title + " - Open clipboard list...",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
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
                                "Saving failed: " + Environment.NewLine + ClipDataManager.Instance.Status, Title + " - Save content to file...",
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
    }
}