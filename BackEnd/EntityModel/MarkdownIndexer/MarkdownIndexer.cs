using System.Collections.Generic;
using System.Linq;

namespace DocAsCode.EntityModel.MarkdownIndexer
{
    public static class MarkdownIndexer
    {
        // Order matters
        static List<IIndexerPipeline> pipelines = new List<IIndexerPipeline>()
        {
            new MarkdownFileLoader(),
            new ApiReferenceParser(),
            new CodeSnippetParser(),
            new FullTextIndexer(), // TODO: Ignore the text if it contains YAML HEADER?
            new YamlHeaderParser(),
            new IndexFileSaver(),
        };

        /// <summary>
        /// Save to **.md.map
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static MapFileItemViewModel Exec(IndexerContext context)
        {
            MapFileItemViewModel viewModel = new MapFileItemViewModel();
            var result = ExecutePipeline(viewModel, context);

            result.WriteToConsole();
            return viewModel;
        }

        private static ParseResult ExecutePipeline(MapFileItemViewModel index, IndexerContext context)
        {
            ParseResult result = new ParseResult(ResultLevel.Success);
            foreach(var pipeline in pipelines)
            {
                result = pipeline.Run(index, context);
                if (result.ResultLevel == ResultLevel.Error)
                {
                    return result;
                }

                if (!string.IsNullOrEmpty(result.Message))
                {
                    result.WriteToConsole();
                }
            }

            return result;
        }
    }
}
