/// Clippy - File: "ImageViewerPage.xaml.cs"
/// Copyright © 2018 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Clippy.UiElements
{
    /// <summary>
    /// Interaction logic for ImageViewerPage.xaml
    /// </summary>
    public partial class ImageViewerPage : Page
    {
        private double m_zoomValue = 1.0;

        public ImageViewerPage()
        {
            InitializeComponent();
        }

        public ImageViewerPage(BitmapSource bitmapSource) : this()
        {
            MainImage.Source = bitmapSource;
        }

        private void MainImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool zoomAllowed = false;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Delta < 0)
                {
                    if (m_zoomValue >= 0.2)
                    {
                        m_zoomValue -= 0.1;
                        zoomAllowed = true;
                    }
                }

                else if (e.Delta > 0)
                {
                    if (m_zoomValue <= 10)
                    {
                        m_zoomValue += 0.1;
                        zoomAllowed = true;
                    }
                }

                if (zoomAllowed)
                {
                    ScaleTransform scale = new ScaleTransform(m_zoomValue, m_zoomValue);
                    MainImage.LayoutTransform = scale;
                }
                
                e.Handled = true;
            }
        }
    }
}
