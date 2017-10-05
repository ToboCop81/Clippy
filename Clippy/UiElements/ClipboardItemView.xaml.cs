/// Clippy - File: "ClipboardItemView.cs"
/// Copyright © 2017 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Clippy.Interfaces;
using Clippy.Common;

namespace Clippy.UiElements
{
    /// <summary>
    /// Interaktionslogik für ClipboardItemView.xaml
    /// </summary>
    public partial class ClipboardItemView : UserControl
    {
        private bool m_dependencyPropertyUpdate;

        public event ItemActionEventHandler CopyClicked;

        public event ItemActionEventHandler EditClicked;

        public event ItemActionEventHandler DeleteClicked;

        public IClipboardItem ClipboardItem
        {
            get { return (IClipboardItem)GetValue(ClipboardItemProperty); }
            set { SetValue(ClipboardItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClipboardItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClipboardItemProperty =
            DependencyProperty.Register("ClipboardItem", typeof(IClipboardItem), typeof(ClipboardItemView), new PropertyMetadata(null));


        public ClipboardItemView()
        {
            InitializeComponent();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            CopyClicked(this, ItemAction.ItemCopy, new ClipboardItemEventArgs(ClipboardItem));
        }

        private void CopyFileButton_Click(object sender, RoutedEventArgs e)
        {
            CopyClicked(this, ItemAction.ItemFileCopy, new ClipboardItemEventArgs(ClipboardItem));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditClicked(this, ItemAction.ItemEdit, new ClipboardItemEventArgs(ClipboardItem));
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteClicked(this, ItemAction.ItemDelete, new ClipboardItemEventArgs(ClipboardItem));
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e != null && e.Property.PropertyType == typeof(IClipboardItem))
            {
                UpdateDependencyProperty();
            }

            UpdateCopyFileButtonVisibility();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ClipboardItem = DataContext as IClipboardItem;
            UpdateCopyFileButtonVisibility();
        }

        private void SelectBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!m_dependencyPropertyUpdate)
            {
                ClipboardItem.Selected = SelectBox.IsChecked.Value;
            }
        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!m_dependencyPropertyUpdate)
            {
                ClipboardItem.Title = NameBox.Text;
            }
        }

        private void UpdateDependencyProperty()
        {
            m_dependencyPropertyUpdate = true;
            SelectBox.IsChecked = ClipboardItem.Selected;
            ItemTypeLabel.Content = ClipboardItem.Type.ToString();
            NameBox.Text = ClipboardItem.Title;
            m_dependencyPropertyUpdate = false;
        }

        private void UpdateCopyFileButtonVisibility()
        {
            if (CopyFileButton == null || ClipboardItem == null)
            {
                return;
            }

            // Copy to file only implemented for plain text at the moment - so hide the button in all other cases
            if (ClipboardItem.Type != DataKind.PlainText)
            {
                if (CopyFileButton.Visibility == Visibility.Visible)
                {
                    CopyFileButton.Visibility = Visibility.Hidden;
                }

                return;
            }

            if (ClippySettings.Instance.UseClipboardFiles && CopyFileButton.Visibility == Visibility.Hidden)
            {
                CopyFileButton.Visibility = Visibility.Visible;
            }

            else if (!ClippySettings.Instance.UseClipboardFiles && CopyFileButton.Visibility == Visibility.Visible)
            {
                CopyFileButton.Visibility = Visibility.Hidden;
            }
        }
    }
}
