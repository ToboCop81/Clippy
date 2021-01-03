/// Clippy - File: "StaticHelper.cs"
/// Copyright © 2021 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Input;

namespace Clippy.Common
{
    /// <summary>
    /// Contains static helper methods
    /// </summary>
    public static class StaticHelper
    {

        [DllImport("user32.dll")]
        static extern IntPtr GetOpenClipboardWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(int hwnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(int hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public static string GetVersionInfoString()
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            return $"{currentAssembly.FullName}_V{currentAssembly.GetName().Version.ToString()}";
        }

        /// <summary>
        /// Gets the pipe name for sending messages between instances
        /// </summary>
        public static string GetPipeName()
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            return "##ClippyPipe##";
        }

        /// <summary>
        /// When the clipboard is blocked: Get the title of the window which is respoinsible
        /// </summary>
        public static string GetOpenClipboardWindowInfo()
        {
            var hwnd = GetOpenClipboardWindow();
            if (hwnd == IntPtr.Zero)
            {
                return "<unknown>";
            }

            int handle = hwnd.ToInt32();
            int titleLength = GetWindowTextLength(handle);
            StringBuilder titleBuilder = new StringBuilder(titleLength);
            GetWindowText(handle, titleBuilder, titleLength);

            uint processId;
            GetWindowThreadProcessId(hwnd, out processId);

            string title = titleBuilder.ToString();
            title = title == string.Empty ? "<no title>" : title;

            return $"PID: {processId} Title: {title}";
        }

        /// <summary>
        /// Checks if given expression is a numeric value
        /// </summary>
        /// <param name="Expression">Expression to check</param>
        /// <returns>TRUE if expression is numeric</returns>
        public static bool IsNumeric(object Expression)
        {
            bool isNum;
            double retNum;

            isNum = double.TryParse(
                Convert.ToString(Expression), 
                System.Globalization.NumberStyles.Any, 
                System.Globalization.NumberFormatInfo.InvariantInfo,
                out retNum);

            return isNum;
        }

        public static bool ValidateFileNameAndPath(string file)
        {
            string trimmedName = file.Trim();
            if (string.IsNullOrEmpty(trimmedName)) { return false; }

            Regex forbiddenFileChars = new Regex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]");
            // We only have a filename - no path
            if (!trimmedName.Contains("\\") || trimmedName.Length < 4)
            {          
                if (forbiddenFileChars.IsMatch(trimmedName, 0))
                {
                    return false;
                }
            }

            //Now check path and filename
            FileInfo fi = new FileInfo(trimmedName);
            Regex forbiddenPathChars = new Regex("[" + Regex.Escape(new string(Path.GetInvalidPathChars())) + "]");
            if (forbiddenPathChars.IsMatch(fi.DirectoryName))
            {
                return false;
            }

            if (forbiddenFileChars.IsMatch(fi.Name))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Convert given plaintext to base64 encoded string
        /// </summary>
        public static string Base64Encode(string plainText)
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Convert base64 encoded string to plain text
        /// </summary>
        public static string Base64Decode(string base64string)
        {
            var bytes = Convert.FromBase64String(base64string);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Detect encoding of textfile by BOM
        /// </summary>
        public static Encoding GetFileEncoding(string srcFile)
        {
            Encoding encoding = Encoding.Default;

            byte[] bom = new byte[4];
            using (FileStream fileStream = new FileStream(srcFile, FileMode.Open))
            {
                fileStream.Read(bom, 0, 4);
                fileStream.Close();
            }

            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) { encoding = Encoding.UTF8; }
            else if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) { encoding = Encoding.UTF7; }
            else if (bom[0] == 0xfe && bom[1] == 0xff) { encoding = Encoding.Unicode; }
            else if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) { encoding = Encoding.UTF32; }
                      
            return encoding;
        }

        /// <summary>
        /// Get the current pressed key modifiers
        /// </summary>
        public static KeyModifiers GetCurrentKeyModifiers()
        {
            if ((Keyboard.Modifiers & ModifierKeys.Alt) > 0) return KeyModifiers.Alt;
            if ((Keyboard.Modifiers & ModifierKeys.Control) > 0) return KeyModifiers.Control;
            if ((Keyboard.Modifiers & ModifierKeys.Shift) > 0) return KeyModifiers.Shift;
            if ((Keyboard.Modifiers & ModifierKeys.Windows) > 0) return KeyModifiers.Windows;

            return KeyModifiers.None;
        }
    }
}
