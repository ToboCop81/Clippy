/// Clippy - File: "Setting.cs"
/// Copyright © 2017 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using System;

namespace Clippy.Resources
{
    [Serializable]
    public class Setting
    {
        /// <summary>
        /// Creates a new instance of the setting with the given values
        /// </summary>
        public Setting(int id, string name, object value, bool isActive = true)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            Id = id;
            Name = name;
            Value = value ?? throw new ArgumentNullException("value");
            IsActive = isActive;
        }

        public int Id { get; }

        public string Name { get; }

        public object Value { get; set; }

        public bool IsActive { get; set; }
    }
}
