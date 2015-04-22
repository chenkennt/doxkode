namespace DocAsCode.EntityModel.MarkdownIndexer
{
    public class ApiReferenceParser : IIndexerPipeline
    {
        public ParseResult Run(MapFileItemViewModel item, IndexerContext context)
        {
            var content = context.MarkdownContent;
            var links = LinkParser.Select(content);
            // TODO: add to references
            return new ParseResult(ResultLevel.Success);
        }
    }
}
