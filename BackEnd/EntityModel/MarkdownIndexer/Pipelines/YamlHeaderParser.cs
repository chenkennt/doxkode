namespace DocAsCode.EntityModel.MarkdownIndexer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using DocAsCode.Utility;

    public class YamlHeaderParser : IIndexerPipeline
    {
        public ParseResult Run(MapFileItemViewModel item, IndexerContext context)
        {
            var filePath = context.MarkdownFilePath;
            var content = context.MarkdownContent;
            var apiList = context.ExternalApiIndex;
            List<MapFileItemViewModel> indics;
            var result = BuildMarkdownIndexHelper.TryParseCustomizedMarkdown(
                filePath,
                content,
                s =>
                    {
                        if (apiList.TryGetValue(s.Name, out item))
                        {
                            return new ParseResult(ResultLevel.Success);
                        }
                        else
                        {
                            return new ParseResult(ResultLevel.Error, "Cannot find {0} in the documentation", s.Name);
                        }
                    },
                out indics);

            // Select references to the indices where DefinedLine is between startline and endline
            return new ParseResult(ResultLevel.Success);
        }
    }
}
