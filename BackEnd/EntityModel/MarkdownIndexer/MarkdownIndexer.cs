using System.Collections.Generic;
using System.Linq;

namespace DocAsCode.EntityModel.MarkdownIndexer
{
    public static class MarkdownIndexer
    {
        // Order matters
        static List<IIndexerPipeline> pipelines = new List<IIndexerPipeline>()
        {
            new LoadMarkdownFile(),
            new ResolveApiReference(),
            new ResolveCodeSnippet(),
            new GenerateFullTextIndex(), // TODO: Ignore the text if it contains YAML HEADER?
            new ResolveYamlHeader(),
            new Save(),
        };

        /// <summary>
        /// Save to **.md.map
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ParseResult Exec(IndexerContext context)
        {
            MapFileItemViewModel viewModel = new MapFileItemViewModel();
            return ExecutePipeline(viewModel, context);
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
