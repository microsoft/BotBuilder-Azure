using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Azure
{
    internal static class StringExtensions
    {
        private static readonly Dictionary<string, string> _DefaultReplacementsForCharactersDisallowedByAzure = new Dictionary<string, string>() { { "/", "|s|" }, { @"\", "|b|" }, { "#", "|h|" }, { "?", "|q|" } };

        /// <summary>
        /// Replaces the four characters disallowed in Azure keys with something more palatable.  You can provide your own mapping if you don't like the defaults.
        /// </summary>
        internal static string SanitizeForAzureKeys(this string input, Dictionary<string, string> replacements = null)
        {
            var repmap = replacements ?? _DefaultReplacementsForCharactersDisallowedByAzure;
            return input.Trim().Replace("/", repmap["/"]).Replace(@"\", repmap[@"\"]).Replace("#", repmap["#"]).Replace("?", repmap["?"]);
        }
        
        internal static string SanitizeTableName(this string input)
        {
            if (input.Length > 63)
            {
                input = input.Substring(0, 63);
            }
            input = Regex.Replace(input, @"[^a-zA-Z0-9]+", "");
            return input;
        }
    }
}
