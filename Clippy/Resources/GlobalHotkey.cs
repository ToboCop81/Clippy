/// Clippy - File: "GlobalHotkey.cs"
/// Copyright © 2021 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Clippy.Resources
{
    /// <summary>
    /// Represents a key kombination for a global hotkey
    /// </summary>
    [Serializable]
    public class GlobalHotkey
    {
        public GlobalHotkey()
        {
            Key = Key.None;
            Modifiers = ModifierKeys.None;
            IsActive = false;
        }

        public GlobalHotkey(Key key, ModifierKeys modifierKeys, bool isActive)
        {
            Key = key;
            Modifiers = modifierKeys;
            IsActive = isActive;
        }

        public Key Key { get; set; }

        public ModifierKeys Modifiers { get; set; }

        public bool IsActive { get; set; }

        public override string ToString()
        {
            return (Modifiers != ModifierKeys.None) ? $"{Modifiers} + {Key}" : $"{Key}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GlobalHotkey);
        }

        public bool Equals(GlobalHotkey ghk)
        {
            if (ReferenceEquals(ghk, null)) return false;

            if (ReferenceEquals(this, ghk)) return true;

            if (GetType() != ghk.GetType()) return false;

            return (Key == ghk.Key) && (Modifiers == ghk.Modifiers) && (IsActive == ghk.IsActive);
        }

        public override int GetHashCode()
        {
            var result = 0;
            result = (result * 397) ^ Key.GetHashCode();
            result = (result * 397) ^ Modifiers.GetHashCode();
            result = (result * 397) ^ IsActive.GetHashCode();
            return result;
        }
    }
}
