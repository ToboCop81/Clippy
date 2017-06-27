using System.Windows;
using System.Windows.Controls;

namespace Clippy.UiElements
{
    /// <summary>
    /// Interaktionslogik für ClipboardItemView.xaml
    /// </summary>
    public partial class ClipboardItemView : UserControl
    {
        public bool Selected
        {
            get { return (bool)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Selected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.Register("Selected", typeof(bool), typeof(ClipboardItemView), new PropertyMetadata(false));


        public string ItemTitle
        {
            get { return (string)GetValue(ItemTitleProperty); }
            set { SetValue(ItemTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTitleProperty =
            DependencyProperty.Register("ItemTitle", typeof(string), typeof(ClipboardItemView), new PropertyMetadata(string.Empty));


        public string ItemType
        {
            get { return (string)GetValue(ItemTypeProperty); }
            set { SetValue(ItemTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTypeProperty =
            DependencyProperty.Register("ItemType", typeof(string), typeof(ClipboardItemView), new PropertyMetadata(string.Empty));


        public ClipboardItemView()
        {
            InitializeComponent();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
