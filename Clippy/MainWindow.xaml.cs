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

        public MainWindow(bool hasStartupArgs = false, string[] startupArgs = null)
        {
            m_hasStartupArgs = hasStartupArgs;
            m_startupArgs = startupArgs;

            InitializeComponent();
            ClippySettings.Instance.InitializeSettings();
            ClipDataManager.Instance.ItemsChanged += ItemsChangedHandler;

            SetupWindow();

            LoadItemsList();         
        }

        private void SetupWindow()
        {
            MenuItemAutosaveWindowLayout.IsChecked = ClippySettings.Instance.SaveWindowLayoutState;
            MenuItemAutosaveList.IsChecked = ClippySettings.Instance.AutoSaveState;
            MenuItemTextItemNameFromContent.IsChecked = ClippySettings.Instance.TextItemNameFromContent;
            ListBoxClipboardItems.ItemsSource = ClipDataManager.Instance.Items;

            if (ClippySettings.Instance.SaveWindowLayoutState)
            {
                ClippySettings.Instance.RestoreWindowLayout(this);
            }
        }

        private void LoadItemsList()
        {
            if (m_hasStartupArgs)
            {
                bool extOk = m_startupArgs[0].ToLower().EndsWith("." + ClipDataManager.Instance.FileExtension);
                if (extOk)
                {
                    ClipDataManager.Instance.LoadList(m_startupArgs[0]);
                }
            }

            else if (ClippySettings.Instance.AutoSaveState == true)
            {
                ClipDataManager.Instance.AutoLoad();
            }
        }

        private void ShutdownProgram()
        {
            Application.Current.Shutdown();
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

        private void MenuItemClear_Click(object sender, RoutedEventArgs e)
        {
            if (ClipDataManager.Instance.Items.Count == 0)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Do you really want to remove all items from the list?",
                $"{this.Title}: Confirm Clearing",
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
                $"{this.Title}: Confirm Removing",
                MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);

            if (result == MessageBoxResult.Yes)
            {
                ClipDataManager.Instance.RemoveSelectedItems();
            }
        }

        private void MenuItemGetData_Click(object sender, RoutedEventArgs e)
        {
            ClipDataManager.Instance.GetDataFromClipboard();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            IClipboardItem currentItem = ((FrameworkElement)sender).DataContext as IClipboardItem;
            if (currentItem != null)
            {
                currentItem.CopyToClipboard();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            IClipboardItem currentItem = ((FrameworkElement)sender).DataContext as IClipboardItem;
            if (currentItem != null)
            {
                ClipDataManager.Instance.RemoveItem(currentItem.Index);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            IClipboardItem currentItem = ((FrameworkElement)sender).DataContext as IClipboardItem;
            if (currentItem != null)
            {
                ContentViewWindow contentViewWindow = new ContentViewWindow(currentItem);
                contentViewWindow.ShowDialog();
                if (contentViewWindow.ContentChanged)
                {
                    ListBoxClipboardItems.Items.Refresh();
                }
            }
        }

        private void ItemsChangedHandler(ItemsChangeType changeType, ItemsChangedEventArgs e)
        {
            ListBoxClipboardItems.ItemsSource = ClipDataManager.Instance.Items;
            ListBoxClipboardItems.Items.Refresh();
        }

        private void ClippyMainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                ListBoxClipboardItems.ItemsSource = ClipDataManager.Instance.Items;
                ListBoxClipboardItems.Items.Refresh();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.V)
            {
                ClipDataManager.Instance.GetDataFromClipboard();
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

            ClippySettings.Instance.SaveWindowLayoutState = MenuItemAutosaveWindowLayout.IsChecked;
            ClippySettings.Instance.AutoSaveState = MenuItemAutosaveList.IsChecked;
            ClippySettings.Instance.TextItemNameFromContent = MenuItemTextItemNameFromContent.IsChecked;
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

            if (saveFileDialog.ShowDialog() == true  && !string.IsNullOrEmpty(saveFileDialog.FileName))
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

        private void MenuItemTextItemNameFromContent_Click(object sender, RoutedEventArgs e)
        {
            ClippySettings.Instance.TextItemNameFromContent = MenuItemTextItemNameFromContent.IsChecked;
        }
    }
}