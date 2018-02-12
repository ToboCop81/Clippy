/// Clippy - File: "ClipboardItemBase.cs"
/// Copyright © 2018 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Common;
using Clippy.Interfaces;
using System;

namespace Clippy.Resources
{
    [Serializable]
    public abstract class ClipboardItemBase : IClipboardItem
    {
        protected DateTime m_timeStamp;
        protected DataKind m_type;
        protected object m_data;

        public ClipboardItemBase(long index)
        {
            Index = index;
            Title = null;
            Selected = false;
            Favorite = false;
            m_type = DataKind.Undefined;
            m_timeStamp = DateTime.Now;
            m_data = null;
        }

        public string Title { get; set; }
        public long Index { get; set; }
        public bool Selected { get; set; }
        public bool Favorite { get; set; }

        public DateTime TimeStamp => m_timeStamp;
        public DataKind Type => m_type;
        public object Data => m_data;

        public abstract void CopyToClipboard();
    }
}
