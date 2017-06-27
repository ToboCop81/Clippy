using Clippy.Common;
using Clippy.Interfaces;
using System;

namespace Clippy.Functionality
{
    public delegate void ItemsChangedEventHandler(ItemsChangeType changeType, ItemsChangedEventArgs e);

    public class ItemsChangedEventArgs : EventArgs
    {
        public ItemsChangedEventArgs(IClipboardItem changedItem)
        {
            ItemIndex = changedItem.Index;
            ItemName = changedItem.Title;
            ItemType = changedItem.Type;
        }

        public long ItemIndex { get; set; }
        public string ItemName { get; set; }
        public DataKind ItemType { get; set; }
    }

}
