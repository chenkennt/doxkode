﻿namespace DocAsCode.EntityModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;

    using DocAsCode.Utility;

    /// <summary>
    /// Match code snippet from markdown files.
    /// Code snippet syntax: {{"relativePath/sourceFilename"}} or {{"relativePath/sourceFilename"[startline-endline]}}
    /// where starline and endline should be integers and are both optional,
    /// the default values for which are 1 and the fileLength.
    /// </summary>
    public static class CodeSnippetParser
    {
        // Use backreference, support "" and '' wrapper, or no wrapper
        public static readonly Regex CodeSnippetRegex = new Regex(@"{{\s*([""']?)(?<source>\S*?)\1\s*(\[(?<line>\d*-\d*?)\])?\s*}}", RegexOptions.Compiled);

        public static IList<MatchDetail> Select(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            var codeSnippet = CodeSnippetRegex.Matches(input);
            if (codeSnippet.Count == 0) return null;

            // For code snippet, id is the file path, should be case insensitive
            var details = new MatchDetailCollection(StringComparer.OrdinalIgnoreCase);
            var singles = (from Match item in codeSnippet select SelectSingle(item, input));
            details.Merge(singles);
            return details.Values.ToList();
        }

        private static MatchSingleDetail SelectSingle(Match match, string input)
        {
            var wholeMatch = match.Groups[0];

            string id = match.Groups["source"].Value.Trim();
            string rangeStr = match.Groups["line"].Value.Trim();
            var range = ExtractRange(rangeStr);
            var idWithRange = string.Format("{0}[{1}-{2}]", id, range.Item1, range.Item2 >= 0 ? range.Item2.ToString() : string.Empty);
            var location = Location.GetLocation(input, wholeMatch.Index, wholeMatch.Length);

            // As code snippet is case insensitive, change the Id to lower case for further references convenience. e.g. selection in javascript
            return new MatchSingleDetail
                       {
                           Id = idWithRange.BackSlashToForwardSlash().ToLowerInvariant(),
                           Path = id,
                           MatchedSection =
                               new Section { Key = wholeMatch.Value, Locations = new List<Location> { location } },
                           StartLine = range.Item1,
                           EndLine = range.Item2
                       };
        }

        /// <summary>
        /// Extrat range
        /// Lines start from 0
        /// use -1 to stand for max length
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        private static Tuple<int, int> ExtractRange(string range)
        {
            int startline = 0, endLine = -1;
            if (!string.IsNullOrEmpty(range))
            {
                string[] lines = range.Split('-');

                if (!string.IsNullOrEmpty(lines[0]))
                {
                    int.TryParse(lines[0], out startline);
                }

                if (!string.IsNullOrEmpty(lines[1]))
                {
                    int.TryParse(lines[1], out endLine);
                }
            }
            return Tuple.Create(startline, endLine);
        }
    }
}
