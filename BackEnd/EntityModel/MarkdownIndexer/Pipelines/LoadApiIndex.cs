namespace DocAsCode.EntityModel.MarkdownIndexer
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using DocAsCode.Utility;

    public class LoadApiIndex : IIndexerPipeline
    {
        public ParseResult Run(MapFileItemViewModel item, IndexerContext context)
        {
            Dictionary<string, MetadataItem> indexViewModel;
            var inputApiIndexFilePath = context.ApiIndexFilePath;

            // Read index
            if (string.IsNullOrEmpty(inputApiIndexFilePath) || !File.Exists(inputApiIndexFilePath))
            {
                context.ExternalApiIndex = new Dictionary<string, MetadataItem>();
                if (string.IsNullOrEmpty(inputApiIndexFilePath)) return new ParseResult(ResultLevel.Success);
                return new ParseResult(ResultLevel.Warn, "Index file {0} for API is not found", inputApiIndexFilePath);
            }

            using (StreamReader sr = new StreamReader(inputApiIndexFilePath))
            {
                indexViewModel = YamlUtility.Deserialize<Dictionary<string, MetadataItem>>(sr);
            }

            context.ExternalApiIndex = indexViewModel;
            return new ParseResult(ResultLevel.Success);
        }
    }
}
