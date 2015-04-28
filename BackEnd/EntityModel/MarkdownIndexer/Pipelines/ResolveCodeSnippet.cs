﻿namespace DocAsCode.EntityModel.MarkdownIndexer
{
    using System;
    using System.IO;

    using DocAsCode.Utility;

    public class ResolveCodeSnippet : IIndexerPipeline
    {
        public ParseResult Run(MapFileItemViewModel item, IndexerContext context)
        {
            var filePath = context.MarkdownFilePath;
            var content = context.MarkdownContent;
            var codeSnippets = CodeSnippetParser.Select(content);
            if (codeSnippets == null || codeSnippets.Count == 0) return new ParseResult(ResultLevel.Info, "No code snippet reference found for {0}", filePath);
            if (item.References == null) item.References = new ReferencesViewModel();
            ReferencesViewModel references = item.References;
            var defaultReferenceFolder = Environment.CurrentDirectory;
            var referenceFolder = string.IsNullOrEmpty(context.ReferenceOutputFolder)
                                    ? defaultReferenceFolder
                                    : context.ReferenceOutputFolder;
            foreach (var codeSnippet in codeSnippets)
            {
                var referenceId = codeSnippet.Id;
                var codeSnippetPath = FileExtensions.GetFullPath(Path.GetDirectoryName(filePath), codeSnippet.Path);
                // As reference, copy file to local
                var targetFileName = FileExtensions.MakeRelativePath(referenceFolder, codeSnippetPath).ToValidFilePath();
                var targetPath = Path.Combine(referenceFolder, targetFileName);
                MapFileItemViewModel reference;
                if (!File.Exists(codeSnippetPath))
                {
                    reference = new MapFileItemViewModel
                    {
                        Id = referenceId,
                        ReferenceKeys = codeSnippet.MatchedSections,
                        Message = string.Format("{0} does not exist.", Path.GetFullPath(codeSnippetPath))
                    };

                    ParseResult.WriteToConsole(ResultLevel.Warn, reference.Message);
                }
                else
                {
                    FileExtensions.CopyFile(codeSnippetPath, targetPath);
                    reference = new MapFileItemViewModel
                    {
                        Id = referenceId,
                        ReferenceKeys = codeSnippet.MatchedSections,
                        Path = FileExtensions.MakeRelativePath(Path.GetDirectoryName(filePath), targetPath).BackSlashToForwardSlash(),
                        Startline = codeSnippet.StartLine,
                        Endline = codeSnippet.EndLine,
                    };
                }

                // Api Index file only contains Id and Href
                references.AddItem(reference);
            }

            return new ParseResult(ResultLevel.Success);
        }
    }
}
