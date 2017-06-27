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
                case Common.DataKind.PlainText:
                    TextEditorPage textEditor = new TextEditorPage(((PlainTextItem)m_item).GetText());
                    textEditor.FontComboBox.IsEnabled = false;
                    textEditor.FontComboBox.Visibility = Visibility.Collapsed;
                    if (saveFlag)
                    {
                        textEditor.SetFontSize(ClippySettings.Instance.FontSize);
                    }

                    ContentFrame.Navigate(textEditor);
                    break;
                case Common.DataKind.Image:
                    ContentFrame.Navigate(new ImageViewerPage(((ImageItem)m_item).GetImage()));
                    break;
                case Common.DataKind.Undefined:
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
        }

        private void CommitButton_Click(object sender, RoutedEventArgs e)
        {
            m_item.Title = TitleLabel.Text;

            switch (m_item.Type)
            {
                case Common.DataKind.PlainText:
                    ((PlainTextItem)m_item).UpdateText(((TextEditorPage)ContentFrame.Content).GetText());
                    break;
            }

            ContentChanged = true;
        }

        private void ContentPreviewWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ClippySettings.Instance.SaveWindowLayoutState == true)
            {
                ClippySettings.Instance.SaveWindowLayout(this);
                switch (m_item.Type)
                {
                    case Common.DataKind.PlainText:
                       ClippySettings.Instance.FontSize = ((TextEditorPage)ContentFrame.Content).MainTextbox.FontSize;
                       break;
                }
            }
        }
    }
}
