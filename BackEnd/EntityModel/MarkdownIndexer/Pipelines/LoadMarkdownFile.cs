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
                item.Path = context.MarkdownFilePath.BackSlashToForwardSlash();
                item.Href = context.MarkdownFilePath.BackSlashToForwardSlash();
            }

            return new ParseResult(ResultLevel.Success);
        }
    }
}
