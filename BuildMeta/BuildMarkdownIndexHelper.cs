using DocAsCode.EntityModel;
using DocAsCode.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DocAsCode.BuildMeta
{
    public class BuildMarkdownIndexHelper
    {
        public static Dictionary<string, MarkdownIndex> MergeMarkdownResults(List<string> markdownFilePathList, Dictionary<string, IndexYamlItemViewModel> apiList, string workingDirectory, string mdFolderName)
        {
            Dictionary<string, MarkdownIndex> table = new Dictionary<string, MarkdownIndex>();

            foreach(var file in markdownFilePathList.Distinct())
            {
                List<MarkdownIndex> indics;
                IndexYamlItemViewModel item;
                string apiFolder = Path.Combine(workingDirectory, mdFolderName);
                if (Directory.Exists(apiFolder))
                {
                    ParseResult.WriteToConsole(ResultLevel.Warn, "Folder {0} already exists!", apiFolder);
                }
                else
                {
                    Directory.CreateDirectory(apiFolder);
                }

                string destFileName = Path.Combine(apiFolder, file.ToValidFilePath());
                string resolvedContent = string.Empty;
                try
                {
                    string input = File.ReadAllText(file);
                    resolvedContent = LinkParser.ResolveToMarkdownLink(apiList, input);
                    File.WriteAllText(destFileName, resolvedContent);
                }
                catch (Exception e)
                {
                    ParseResult.WriteToConsole(ResultLevel.Error, "Unable to copy markdown file {0} to output directory {1}: {2}", file, apiFolder, e.Message);
                    continue;
                }

                var result = TryParseCustomizedMarkdown(file, resolvedContent, s =>
                {
                    if (apiList.TryGetValue(s.Name, out item))
                    {
                        return new ParseResult(ResultLevel.Success);
                    }
                    else
                    {
                        return new ParseResult(ResultLevel.Error, "Cannot find {0} in the documentation", s.Name);
                    }
                }, out indics);

                if (result.ResultLevel != ResultLevel.Success)
                {
                    Console.Error.WriteLine(result);
                }

                foreach (var key in indics)
                {
                    MarkdownIndex saved;
                    if (table.TryGetValue(key.ApiName, out saved))
                    {
                        ParseResult.WriteToConsole(ResultLevel.Error, "Already contains {0} in file {1}, current one {2} will be ignored.", key.ApiName, saved.Path, key.Path);
                    }
                    else
                    {
                        key.Href = destFileName.FormatPath(UriKind.Relative, workingDirectory);
                        table.Add(key.ApiName, key);
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// Not doing duplication check here, do it outside
        /// </summary>
        /// <param name="markdownFilePath"></param>
        /// <param name="markdown"></param>
        /// <returns></returns>
        public static ParseResult TryParseCustomizedMarkdown(string markdownFilePath, string resolvedContent, Func<YamlItemViewModel, ParseResult> yamlHandler, out List<MarkdownIndex> markdown)
        {
            var gitDetail = GitUtility.GetGitDetail(markdownFilePath);
            if (string.IsNullOrEmpty(resolvedContent)) resolvedContent = File.ReadAllText(markdownFilePath);
            string markdownFile = resolvedContent;
            int length = markdownFile.Length;
            var yamlRegex = new Regex(@"\-\-\-((?!\n)\s)*\n((?!\n)\s)*(?<content>.*)((?!\n)\s)*\n\-\-\-((?!\n)\s)*\n", RegexOptions.Compiled | RegexOptions.Multiline);
            MatchCollection matches = yamlRegex.Matches(markdownFile);
            if (matches.Count == 0)
            {
                markdown = new List<MarkdownIndex>();
                return new ParseResult(ResultLevel.Warn, "no valid yaml header is found in {0}", markdownFilePath);
            }

            int startIndex = 0;
            MarkdownIndex lastSection = null;
            List<MarkdownIndex> sections = new List<MarkdownIndex>();

            StringBuilder error = new StringBuilder();

            for(int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                string content = match.Groups["content"].Value;

                YamlItemViewModel viewModel;
                // Content to yaml
                try
                {
                    using (StringReader reader = new StringReader(content))
                    {
                        viewModel = YamlUtility.Deserializer.Deserialize<YamlItemViewModel>(reader);

                        if (string.IsNullOrEmpty(viewModel.Name))
                        {
                            throw new ArgumentException("Name for yaml header is required");
                        }

                        // TODO: override metadata, merge viewmodel?

                        if (yamlHandler != null)
                        {
                            ParseResult result = yamlHandler(viewModel);
                            if (result.ResultLevel != ResultLevel.Success)
                            {
                                throw new ArgumentException(result.Message);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    error.AppendFormat("{0} in {1} line {2} is not in a valid yaml format {3}", match.Value, markdownFilePath, markdownFile.Substring(0, startIndex).Split('\n').Length + 2, e.Message);
                    continue;
                }

                startIndex = match.Index + match.Length;
                if (lastSection != null)
                {

                    lastSection.ContentEndIndex = match.Index - 1;

                    if (lastSection.ContentEndIndex > lastSection.ContentStartIndex)
                    {
                        lastSection.MarkdownContent = markdownFile.Substring(lastSection.ContentStartIndex, lastSection.ContentEndIndex - lastSection.ContentStartIndex + 1);
                        sections.Add(lastSection);
                    }
                }

                lastSection = new MarkdownIndex { ApiName = viewModel.Name, ContentStartIndex = startIndex, ContentEndIndex = length - 1, Remote = gitDetail, Path = markdownFilePath }; // endIndex should be set from next match if there is next match
                if (lastSection.ContentEndIndex > lastSection.ContentStartIndex)
                {
                    lastSection.MarkdownContent = markdownFile.Substring(lastSection.ContentStartIndex);
                }
            }

            if (lastSection != null)
            {
                if (lastSection.Remote != null && !string.IsNullOrEmpty(lastSection.Remote.LocalWorkingDirectory))
                {
                    lastSection.Path = lastSection.Path.FormatPath(UriKind.Relative, lastSection.Remote.LocalWorkingDirectory);
                }

                sections.Add(lastSection);
            }

            markdown = sections;
            if (error.Length > 0)
            {
                return new ParseResult(ResultLevel.Warn, error.ToString());
            }

            return new ParseResult(ResultLevel.Success);
        }
    }
}
