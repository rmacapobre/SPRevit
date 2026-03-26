using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    public static class StringUtilities
    {
        /// <summary>
        /// Converts the specified section number to the specified format.
        /// </summary>
        /// <param name="currentnumber">The current number.</param>
        /// <param name="sectionnumber">Set to the newly-formatted section number.</param>
        /// <param name="format">The format to convert the section number to (0 = ######, 1 = ## ####, 2 = ## ## ##).</param>
        /// <returns>Whether specified section number format was valid.</returns>
        public static bool ConfirmSectionNumberFormat(String currentnumber, out String sectionnumber, int format)//0=######, 1=## ####, 2=## ## ##
        {
            sectionnumber = currentnumber = currentnumber.Trim().Replace(" ", "");
            bool ret = false;
            if (format > 0 && sectionnumber.Length >= 6)
            {
                switch (format)
                {
                    case 1:
                        sectionnumber = " " + currentnumber.Substring(0, 2) + " " + currentnumber.Substring(2);
                        ret = true;
                        break;

                    case 2:
                        sectionnumber = " " + currentnumber.Substring(0, 2) + " ";//they seem to come in with a space
                        if (currentnumber.Length <= 9)
                            sectionnumber += currentnumber.Substring(2, 2) + " " + currentnumber.Substring(4);
                        else      // formatting for long UFGS section numbers (e.g., xxxxxx.0020) 
                            sectionnumber += currentnumber.Substring(2, 2) + " " + currentnumber.Substring(4, 5) + " " + currentnumber.Substring(9, 2);
                        sectionnumber.Trim();
                        ret = true;
                        break;

                    default:
                        break;
                }
            }
            else
            {
                sectionnumber = " " + currentnumber.Replace(" ", "");
                ret = true;
            }

            return ret;
        }

        /// <summary>
        /// Converts the specified title to the specified format.
        /// </summary>
        /// <param name="currenttitle">The title to convert.</param>
        /// <param name="sectiontitle">Set to the newly-formatted section title.</param>
        /// <param name="format">The format to convert the title to (0 = ######, 1 = ## ####, 2 = ## ## ##).</param>
        public static void ConfirmTitleFormat(String currenttitle, out String sectiontitle, int format)//0=######, 1=## ####, 2=## ## ##
        {
            String sNumber, sName;
            String sTitle = currenttitle;
            int firstSpace = sTitle.IndexOf(" ");
            int nHyphen = sTitle.IndexOf(" -");
            sName = sTitle.Substring(nHyphen);
            sNumber = sTitle.Substring(0, firstSpace);

            String currentnumber = sNumber;
            String sectionnumber = "";
            ConfirmSectionNumberFormat(currentnumber, out sectionnumber, format);
            sectiontitle = sectionnumber + sName;
        }

        /// <summary>
        /// Converts the specified section title to the specified format.
        /// </summary>
        /// <param name="currenttitle">The title to convert.</param>
        /// <param name="sectiontitle">Set to the newly-formatted section title.</param>
        /// <param name="format">The format to convert the title to (0 = ######, 1 = ## ####, 2 = ## ## ##).</param>
        public static void ConfirmSectionTitleFormat(String currenttitle, out String sectiontitle, int format)//0=######, 1=## ####, 2=## ## ##
        {
            String sIntro, sNumber, sName;
            String sTitle = currenttitle;
            int firstSpace = sTitle.IndexOf(" ");
            sIntro = sTitle.Substring(0, firstSpace);
            int nHyphen = sTitle.IndexOf(" -");
            sName = sTitle.Substring(nHyphen);
            sNumber = sTitle.Substring(firstSpace, nHyphen - firstSpace);

            String currentnumber = sNumber;
            String sectionnumber = "";
            ConfirmSectionNumberFormat(currentnumber, out sectionnumber, format);
            sectiontitle = sIntro + sectionnumber + sName;
        }

        /// <summary>
        /// Determines whether the specified string contains any alphanumeric characters.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>
        ///   <c>true</c> if string contains at least one alphanumeric character; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsAlphaNumericCharacters(string str)
        {
            return str.FirstOrDefault(c => char.IsLetterOrDigit(c)) != default(char); // Less concise than Regex, but faster
        }

        /// <summary>
        /// Returns the string surrounded by double quotes ("").
        /// </summary>
        /// <param name="str">String to quote.</param>
        /// <returns>Double-quoted string.</returns>
        public static string DoubleQuote(string str)
        {
            return "\"" + str + "\"";
        }

        /// <summary>
        /// Fixes the specified string's line endings so they are compatible
        /// with Windows (replaces LF with CRLF).
        /// </summary>
        /// <param name="str">The string to process.</param>
        /// <returns>String with line endings converted to CRLF.</returns>
        public static string FixLineEndings(string str)
        {
            return Regex.Replace(str, @"(?<!\r)\n", "\r\n");
        }

        /// <summary>
        /// Gets a comma-separated list of the given numbers as a string.
        /// </summary>
        /// <param name="list">The objects.</param>
        /// <returns>String list (e.g., "1,2,3").</returns>
        public static string GetCommaSeparatedList(IEnumerable<int> list)
        {
            return string.Join(",", list.Select(x => x.ToString()));
        }

        /// <summary>
        /// Gets a comma-separated list of the given numbers as a string.
        /// </summary>
        /// <param name="list">The objects.</param>
        /// <returns>String list (e.g., "1,2,3").</returns>
        public static string GetCommaSeparatedList(IEnumerable<long> list)
        {
            return string.Join(",", list.Select(x => x.ToString()));
        }

        /// <summary>
        /// Makes the given filename valid by removing colons and other path-invalid characters.
        /// Source: http://stackoverflow.com/questions/309485/c-sanitize-file-name
        /// </summary>
        /// <param name="name">filename to clean up.</param>
        /// <returns>Version of filename suitable for saving.</returns>
        public static string MakeValidFileName(string name)
        {
            return MakeValidFileName(name, false);
        }

        /// <summary>
        /// Makes the given filename valid by removing colons and other path-invalid characters.
        /// Source: http://stackoverflow.com/questions/309485/c-sanitize-file-name
        /// </summary>
        /// <param name="name">filename to clean up.</param>
        /// <param name="replaceSpaces">Whether to replace spaces in name with underscores.</param>
        /// <returns>Version of filename suitable for saving.</returns>
        public static string MakeValidFileName(string name, bool replaceSpaces)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            if (replaceSpaces && !(invalidChars.Contains(" ")))
            {
                invalidChars += " ";
            }

            string invalidReStr = string.Format(@"[{0}]", invalidChars);
            return Regex.Replace(name, invalidReStr, "_");
        }

        /// <summary>
        /// Replaces all occurrences of one or more whitespace in the given
        /// string with a single space character, returning the result.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The string with all whitespace sequences converted to
        /// a single space character.</returns>
        public static string NormalizeWhitespace(string str)
        {
            return Regex.Replace(str, @"\s+", " ");
        }

        /// <summary>
        /// Returns the string with any non-digit characters removed.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>String with non-digit characters removed.</returns>
        public static string RemoveNonDigitCharacters(string str)
        {
            return Regex.Replace(str, @"\D", string.Empty);
        }

        /// <summary>
        /// Returns the string with any punctuation characters removed.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>String with punctuation removed.</returns>
        public static string RemovePunctuation(string str)
        {
            return RemovePunctuation(str, string.Empty);
        }

        /// <summary>
        /// Returns the string with any punctuation characters removed except those specified.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="exceptions">String containing any whitespace characters to retain.</param>
        /// <returns>
        /// String with punctuation removed.
        /// </returns>
        public static string RemovePunctuation(string str, string exceptions)
        {
            string pattern = string.IsNullOrEmpty(exceptions)
                ? @"\p{P}"
                : @"[\p{P}-[" + exceptions + "]]";
            return Regex.Replace(str, pattern, string.Empty);
        }

        /// <summary>
        /// Removes all whitespace from the specified string and returns
        /// the result.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>String with all whitespace removed.</returns>
        public static string RemoveWhitespace(string str)
        {
            return Regex.Replace(str, @"\s+", string.Empty);
        }

        /// <summary>
        /// Validates the name of the project in the specified textbox.
        /// </summary>
        /// <param name="textbox">The textbox containing the project name.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        public static void ValidateProjectName(TextBox textbox, KeyEventArgs e)
        {
            if (e.KeyValue == 189) return;//allow '_'

            if (e.KeyData == Keys.Multiply || e.Alt || e.Control || e.Shift || e.KeyData == Keys.Add || e.KeyData == Keys.Divide || e.KeyData == Keys.Tab
                || e.KeyValue == 191 || e.KeyValue == 220 || e.KeyValue == 187 || e.KeyValue == 188 || e.KeyValue == 190)//don't allow '/','\','=',',','.'
            {
                textbox.Text = textbox.Text.Substring(0, textbox.Text.Length - 1);
                textbox.SelectionStart = textbox.Text.Length;
            }
        }

        /// <summary>
        /// Converts the given wildcard expression into a Regex pattern.
        /// </summary>
        /// <param name="wildcardExpression">A wildcard expression.</param>
        /// <returns>Wildcard pattern as a regular expression.</returns>
        public static string WildcardToRegex(string wildcardExpression)
        {
            return "^" + Regex.Escape(wildcardExpression)
                              .Replace(@"\*", ".*")
                              .Replace(@"\?", ".")
                       + "$";
        }

        /// <summary>
        /// Replaces in a string all occurrences of keys in the given dictionary
        /// with their associated values.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="substititions">Lookup of keys to substitution values.</param>
        /// <returns>String with all occurrences of keys replaced with their values, processed in order.</returns>
        public static string ReplaceAll(string str, IDictionary<string, string> substititions)
        {
            string result = str;
            foreach (var pair in substititions)
            {
                result = result.Replace(pair.Key, pair.Value);
            }

            return result;
        }
    }
}
