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
            if (!File.Exists(inputApiIndexFilePath)) return new ParseResult(ResultLevel.Error, "Index file {0} is not found, exiting...", inputApiIndexFilePath);
            using (StreamReader sr = new StreamReader(inputApiIndexFilePath))
            {
                indexViewModel = YamlUtility.Deserialize<Dictionary<string, MetadataItem>>(sr);
            }

            context.ExternalApiIndex = indexViewModel;
            return new ParseResult(ResultLevel.Success);
        }
    }
}
