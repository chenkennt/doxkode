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
        public static Dictionary<string, MarkdownIndex> MergeMarkdownResults(List<string> markdownFilePathList, Dictionary<string, IndexYamlItemViewModel> apiList, string workingDirectory, string mdFolderName, string referenceFolderName)
        {
            Dictionary<string, MarkdownIndex> table = new Dictionary<string, MarkdownIndex>();

            foreach(var file in markdownFilePathList.Distinct())
            {
                List<MarkdownIndex> indics;
                IndexYamlItemViewModel item;
                string apiFolder = Path.Combine(workingDirectory, mdFolderName);
                string referenceFolder = Path.Combine(workingDirectory, referenceFolderName);
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

                var result = TryParseCustomizedMarkdown(file, resolvedContent,referenceFolder, s =>
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
                        if(key.Items != null)
                        {
                            foreach(var keyItem in key.Items)
                            {
                                keyItem.Href = Path.Combine(referenceFolder.FormatPath(UriKind.Relative, workingDirectory), keyItem.Path.ToValidFilePath());
                            }
                        }
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
        public static ParseResult TryParseCustomizedMarkdown(string markdownFilePath, string resolvedContent, string referenceFolder, Func<YamlItemViewModel, ParseResult> yamlHandler, out List<MarkdownIndex> markdown)
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
                        viewModel = YamlUtility.Deserialize<YamlItemViewModel>(reader);

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
                        TryExtractReference(ref lastSection, referenceFolder);
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
                TryExtractReference(ref lastSection, referenceFolder);
                sections.Add(lastSection);
            }

            markdown = sections;
            if (error.Length > 0)
            {
                return new ParseResult(ResultLevel.Warn, error.ToString());
            }

            return new ParseResult(ResultLevel.Success);
        }

        private static Dictionary<string, int> referencedFileLengthCache = new Dictionary<string, int>();

        private static void TryExtractReference(ref MarkdownIndex section, string referenceFolder)
        {
            // Code snippet syntax: {{"relativePath/sourceFilename"}} or {{"relativePath/sourceFilename"[startline-endline]}}
            // where starline and endline should be integers and are both optional, the default values for which are 1 and the fileLength.
            var ReferenceRegex = new Regex(@"{{\s*""(?<source>\S*?)""\s*(\[(?<line>\d*-\d*?)\])?\s*}}", RegexOptions.Multiline);
            MatchCollection matches = ReferenceRegex.Matches(section.MarkdownContent);

            if(matches.Count > 0)
            {
                if (Directory.Exists(referenceFolder))
                {
                    ParseResult.WriteToConsole(ResultLevel.Warn, "Folder {0} already exists!", referenceFolder);
                }
                else
                {
                    Directory.CreateDirectory(referenceFolder);
                }

                for (int i = 0; i < matches.Count; i++)
                {
                    var match = matches[i];
                    string source = match.Groups["source"].Value.Trim();
                    string line = match.Groups["line"].Value.Trim();

                    MarkdownIndex item = new MarkdownIndex();
                    item.ApiName = source;
                    string sourceFile = Path.Combine(Path.GetDirectoryName(section.Path), source.Replace('/', '\\'));
                    item.Path = sourceFile;
                    string destFile = Path.Combine(referenceFolder, sourceFile.ToValidFilePath());

                    if (!referencedFileLengthCache.ContainsKey(destFile))
                    {
                        try
                        {
                            File.Copy(sourceFile, destFile, true);
                        }
                        catch (Exception e)
                        {
                            ParseResult.WriteToConsole(ResultLevel.Error, "Unable to copy referenced file {0} to output directory {1}: {2}", sourceFile, referenceFolder, e.Message);
                            continue;
                        }
                    }

                    item.ContentStartIndex = section.ContentStartIndex + match.Index;
                    item.ContentEndIndex = item.ContentStartIndex + match.Length - 1;

                    if (!string.IsNullOrEmpty(line))
                    {
                        try
                        {
                            string[] lines = line.Split('-');
                            int startline = 0, endLine = 0;

                            if (string.IsNullOrEmpty(lines[0]))
                            {
                                startline = 1;
                            }
                            else
                            {
                                startline = Convert.ToInt32(lines[0]);
                            }

                            int fileLength;
                            if(!referencedFileLengthCache.TryGetValue(destFile, out fileLength))
                            {
                                fileLength = File.ReadLines(destFile).Count();
                                referencedFileLengthCache.Add(destFile, fileLength);
                            }

                            if (string.IsNullOrEmpty(lines[1]))
                            {
                                endLine = fileLength;
                            }
                            else
                            {
                                endLine = Convert.ToInt32(lines[1]);
                            }

                            if(endLine < startline || startline < 1 || endLine > fileLength)
                            {
                                throw new Exception(string.Format("Span infromation about referenced file {0} form line {1} to {2} is invalid", sourceFile, startline.ToString(), endLine.ToString()));
                            }
                            else
                            {
                                item.ReferenceStartLine = startline;
                                item.ReferenceEndLine = endLine;
                                item.ApiName += string.Format("[{0}-{1}]", startline, endLine);
                            }
                        }
                        catch (Exception e)
                        {
                            ParseResult.WriteToConsole(ResultLevel.Error, "Cannot extract span infromation about referenced file {0}: {1}", sourceFile, e.Message);
                            continue;
                        }
                    }

                    if (section.Items == null)
                    {
                        section.Items = new List<MarkdownIndex>();
                    }
                    section.Items.Add(item);
                }
            }
        }
    }
}
