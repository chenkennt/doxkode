using System.Collections.Generic;
using System.Linq;

namespace DocAsCode.EntityModel.MarkdownIndexer
{
    public static class MarkdownIndexer
    {
        // Order matters
        static List<IIndexerPipeline> pipelines = new List<IIndexerPipeline>()
        {
        };

        /// <summary>
        /// Save to **.md.map
        /// </summary>
        /// <param name="allMembers"></param>
        /// <param name="apiFolder"></param>
        /// <returns></returns>
        public static MarkdownIndex ResolveMetadata(Dictionary<string, IIndexerPipeline> allMembers, string apiFolder)
        {
            MarkdownIndex viewModel = new MarkdownIndex();
            IndexerContext context = new IndexerContext();
            var result = ExecutePipeline(viewModel, context);

            result.WriteToConsole();
            return viewModel;
        }

        private static ParseResult ExecutePipeline(MarkdownIndex index, IndexerContext context)
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
