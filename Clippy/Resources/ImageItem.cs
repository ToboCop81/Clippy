/// Clippy - File: "ImageItem.cs"
/// Copyright © 2018 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Common;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Clippy.Resources
{
    [Serializable]
    public class ImageItem : ClipboardItemBase
    {
        private const string s_titleTemplate = "Image";

        public ImageItem(long index, BitmapSource image) : base(index)
        {
            Title = $"{s_titleTemplate}_{index}";
            m_type = DataKind.Image;
            m_data = ClipboardImageHelper.BitmapSourceToByteArray(new PngBitmapEncoder(), image);
        }

        public void UpdateImage(BitmapSource image)
        {
            m_data = ClipboardImageHelper.BitmapSourceToByteArray(new PngBitmapEncoder(), image);
        }

        public BitmapSource GetImage()
        {
           return ClipboardImageHelper.ByteArrayToBitmapSource(Data as byte[]);
        }
    }
}
