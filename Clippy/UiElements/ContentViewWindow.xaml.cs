/// Clippy - File: "ContentViewWindow.cs"
/// Copyright © 2017 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Common;
using Clippy.Interfaces;
using Clippy.Resources;
using System;
using System.Windows;
using System.Windows.Input;

namespace Clippy.UiElements
{
    /// <summary>
    /// Interaction logic for ContentViewWindow.xaml
    /// </summary>
    public partial class ContentViewWindow : Window
    {

        private IClipboardItem m_item;

        public ContentViewWindow(IClipboardItem item)
        {
            m_item = item;
            InitializeComponent();
            ContentChanged = false;

            bool saveFlag = ClippySettings.Instance.SaveWindowLayoutState;

            TitleLabel.Text = m_item.Title;

            switch (m_item.Type)
            {
                case DataKind.PlainText:
                    TextEditorPage textEditor = new TextEditorPage(((PlainTextItem)m_item).GetText());
                    textEditor.FontComboBox.IsEnabled = false;
                    textEditor.FontComboBox.Visibility = Visibility.Collapsed;
                    if (saveFlag)
                    {
                        textEditor.SetFontSize(ClippySettings.Instance.FontSize);
                    }

                    ContentFrame.Navigate(textEditor);
                    break;
                case DataKind.Image:
                    ContentFrame.Navigate(new ImageViewerPage(((ImageItem)m_item).GetImage()));
                    break;
                case DataKind.Undefined:
                    throw new InvalidOperationException("Undefined clipboard item type");
                default:
                    throw new InvalidOperationException("Unkown clipboard item type");
            }

            if (saveFlag)
            {
                ClippySettings.Instance.RestoreWindowLayout(this);
            }
        }

        public bool ContentChanged { get; set; }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }

            if (e.Key == Key.F2)
            {
                CommitChanges();
                Close();
            }

            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Key == Key.S)
            {
                CommitChanges();
            }
        }

        private void CommitButton_Click(object sender, RoutedEventArgs e)
        {
            CommitChanges();
        }

        private void CommitAndCloseButton_Click(object sender, RoutedEventArgs e)
        {
            CommitChanges();
            Close();
        }

        private void ContentPreviewWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ClippySettings.Instance.SaveWindowLayoutState == true)
            {
                ClippySettings.Instance.SaveWindowLayout(this);
                switch (m_item.Type)
                {
                    case DataKind.PlainText:
                       ClippySettings.Instance.FontSize = ((TextEditorPage)ContentFrame.Content).MainTextbox.FontSize;
                       break;
                }
            }
        }

        private void CommitChanges()
        {
            m_item.Title = TitleLabel.Text;

            switch (m_item.Type)
            {
                case DataKind.PlainText:
                    ((PlainTextItem)m_item).UpdateText(((TextEditorPage)ContentFrame.Content).GetText());
                    break;
            }

            ContentChanged = true;
        }
    }
}
