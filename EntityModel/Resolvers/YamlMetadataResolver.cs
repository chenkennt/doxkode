using System.Collections.Generic;
using System.Linq;

namespace DocAsCode.EntityModel
{
    public static class YamlMetadataResolver
    {
        // Order matters
        static List<IResolverPipeline> pipelines = new List<IResolverPipeline>()
        {
            new LayoutCheckAndCleanup(),
            new ResolveRelativePath(),
            new BuildIndex(),
            new ResolveLink(),
            new ResolveGitPath(),
            new ResolvePath(),
            new BuildMembers(),
            new BuildToc(),
        };

        // Folder structure
        // toc.yaml # toc containing all the namespaces
        // api.yaml # id-yaml mapping
        // api/{id}.yaml # items
        public const string YamlExtension = ".yaml";

        /// <summary>
        /// TODO: input Namespace list instead
        /// </summary>
        /// <param name="allMembers"></param>
        /// <returns></returns>
        public static YamlViewModel ResolveMetadata(Dictionary<string, YamlItemViewModel> allMembers, string apiFolder)
        {
            YamlViewModel viewModel = new YamlViewModel();
            viewModel.IndexYamlViewModel = new Dictionary<string, IndexYamlItemViewModel>();
            viewModel.TocYamlViewModel = new YamlItemViewModel()
            {
                Type = MemberType.Toc,
                Items = allMembers.Where(s => s.Value.Type == MemberType.Namespace).Select(s => s.Value).ToList(),
            };
            viewModel.MemberYamlViewModelList = new List<YamlItemViewModel>();
            ResolverContext context = new ResolverContext { ApiFolder = apiFolder };
            var result = ExecutePipeline(viewModel, context);

            result.WriteToConsole();
            return viewModel;
        }

        public static ParseResult ExecutePipeline(YamlViewModel yaml, ResolverContext context)
        {
            ParseResult result = new ParseResult(ResultLevel.Success);
            foreach(var pipeline in pipelines)
            {
                result = pipeline.Run(yaml, context);
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
