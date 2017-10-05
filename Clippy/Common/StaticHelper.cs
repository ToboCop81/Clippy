using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Clippy.Common
{
    /// <summary>
    /// Contains static helper methods
    /// </summary>
    public static class StaticHelper
    {
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
    }
}
