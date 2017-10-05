/// Clippy - File: "Events.cs"
/// Copyright © 2017 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Interfaces;
using System;

namespace Clippy.Common
{
    public delegate void ItemsChangedEventHandler(ItemsChangeType changeType, ClipboardItemEventArgs e);
    public delegate void ItemActionEventHandler(object sender, ItemAction action, ClipboardItemEventArgs e);

    public class ClipboardItemEventArgs : EventArgs
    {
        public ClipboardItemEventArgs(IClipboardItem item)
        {
            ItemIndex = item.Index;
            ItemName = item.Title;
            ItemType = item.Type;
        }

        public long ItemIndex { get; set; }
        public string ItemName { get; set; }
        public DataKind ItemType { get; set; }
    }
}
