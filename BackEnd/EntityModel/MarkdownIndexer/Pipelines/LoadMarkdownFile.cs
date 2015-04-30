namespace DocAsCode.EntityModel.MarkdownIndexer
{
    using System;
    using System.IO;

    using DocAsCode.Utility;

    public class LoadMarkdownFile : IIndexerPipeline
    {
        public ParseResult Run(MapFileItemViewModel item, IndexerContext context)
        {
            if (string.IsNullOrEmpty(context.MarkdownContent) && string.IsNullOrEmpty(context.MarkdownFilePath))
            {
                throw new ArgumentException("Neither Markdown file content nor file path is specified!");
            }

            if (string.IsNullOrEmpty(context.MarkdownContent))
            {
                context.MarkdownContent = File.ReadAllText(context.MarkdownFilePath);
            }

            if (!string.IsNullOrEmpty(context.MarkdownFilePath))
            {
                item.Remote = GitUtility.GetGitDetail(context.MarkdownFilePath);
                if (item.Remote != null)
                {
                    if (string.IsNullOrEmpty(context.RootDirectory)) context.RootDirectory = item.Remote.LocalWorkingDirectory;
                }

                item.Path = context.MarkdownFilePath.FormatPath(UriKind.Relative, context.RootDirectory);
                item.Href = item.Path;
            }

            return new ParseResult(ResultLevel.Success);
        }
    }
}
