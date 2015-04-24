namespace DocAsCode.EntityModel.MarkdownIndexer
{
    using System.Linq;

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
            foreach (var codeSnippet in codeSnippets)
            {
                var referenceId = codeSnippet.Id;
                var path = codeSnippet.Path;
                var reference = new MapFileItemViewModel
                                    {
                                        Id = referenceId,
                                        ReferenceKeys = codeSnippet.MatchedSections,
                                        Href = path,
                                        Startline = codeSnippet.StartLine,
                                        Endline = codeSnippet.EndLine,
                                    };

                // Api Index file only contains Id and Href
                references.AddItem(reference);
            }

            return new ParseResult(ResultLevel.Success);
        }
    }
}
