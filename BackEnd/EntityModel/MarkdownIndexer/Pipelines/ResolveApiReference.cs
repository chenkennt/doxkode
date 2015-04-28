﻿namespace DocAsCode.EntityModel.MarkdownIndexer
{
    using System.IO;
    using System.Linq;

    using DocAsCode.Utility;

    public class ResolveApiReference : IIndexerPipeline
    {
        public ParseResult Run(MapFileItemViewModel item, IndexerContext context)
        {
            var filePath = context.MarkdownFilePath;
            var apis = context.ExternalApiIndex;
            var content = context.MarkdownContent;
            var links = LinkParser.Select(content);
            if (links == null || links.Count == 0) return new ParseResult(ResultLevel.Info, "No Api reference found for {0}", filePath);
            if (item.References == null) item.References = new ReferencesViewModel();
            ReferencesViewModel references = item.References;
           
            foreach (var matchDetail in links)
            {
                var referenceId = matchDetail.Id;
                var apiId = matchDetail.Id;
                MetadataItem api;
                if (apis.TryGetValue(apiId, out api))
                {
                    var reference = new MapFileItemViewModel
                                        {
                                            Id = referenceId,
                                            ReferenceKeys = matchDetail.MatchedSections,
                                            Href = api.Href.BackSlashToForwardSlash()
                                        };

                    // Api Index file only contains Id and Href
                    references.AddItem(reference);
                }
            }

            return new ParseResult(ResultLevel.Success);
        }
    }
}
