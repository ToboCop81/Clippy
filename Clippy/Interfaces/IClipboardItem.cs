using Clippy.Common;
using System;

namespace Clippy.Interfaces
{
    public interface IClipboardItem
    {
        /// <summary>
        /// The name of the clipboard item
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// The unique index
        /// </summary>
        long Index { get; set; }

        /// <summary>
        /// A flag that shows if the clipboard item is selected
        /// </summary>
        bool Selected { get; set; }

        /// <summary>
        /// A flag that shows if the clipboard item is a favorite
        /// </summary>
        bool Favorite { get; set; }

        /// <summary>
        /// The creation time stamp
        /// </summary>
        DateTime TimeStamp { get; }

        /// <summary>
        /// Information about the kind of contained data
        /// </summary>
        DataKind Type { get; }

        /// <summary>
        /// The data object
        /// </summary>
        object Data { get; }

        /// <summary>
        /// Copies the content of Data to the clipboard
        /// </summary>
        void CopyToClipboard();
    }
}
