﻿using System.Threading.Tasks;
using DocAsCode.Utility;

namespace DocAsCode.EntityModel
{
    public class ResolveRelativePath : IResolverPipeline
    {
        public ParseResult Run(YamlViewModel yaml, ResolverContext context)
        {
            TreeIterator.PreorderAsync<YamlItemViewModel>(yaml.TocYamlViewModel, null,
                (s) =>
                {
                    if (s.IsInvalid) return null;
                    else return s.Items;
                }, (current, parent) =>
                {
                    if (current.Type != MemberType.Toc)
                    {
                        if (current.Type.IsPageLevel())
                        {
                            current.YamlPath = context.ApiFolder.ForwardSlashCombine(current.Name + Constants.YamlExtension);
                            current.Href = context.ApiFolder.ForwardSlashCombine(current.Name);
                        }
                        else
                        {
                            current.YamlPath = parent.YamlPath;
                            current.Href = parent.Href;
                        }
                    }

                    return Task.FromResult(true);
                }
                ).Wait();

            return new ParseResult(ResultLevel.Success);
        }
    }
}