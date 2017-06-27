using System;
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

        /// <summary>
        /// Checks if given filename contains forbidden characters ( \ / : * ? ...)
        ///</summary>
        /// <param name="filename">The filename to check. Do only give the filename - not a path!</param>
        /// <returns>bool</returns>
        public static bool CheckForbiddenFilenameChars(string filename)
        {
            Regex forbidden = new Regex(@"[\\/:*?""<>|]");
            if (forbidden.IsMatch(filename, 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
