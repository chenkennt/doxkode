using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DocAsCode.Utility;

namespace DocAsCode.EntityModel
{
    public class ResolvePath : IResolverPipeline
    {
        public ParseResult Run(MetadataModel yaml, ResolverContext context)
        {
            TreeIterator.PreorderAsync<MetadataItem>(yaml.TocYamlViewModel, null,
                (s) =>
                {
                    if (s.IsInvalid) return null;
                    else return s.Items;
                }, (current, parent) =>
                {
                    if (current.Inheritance != null && current.Inheritance.Count > 0)
                    {
                        current.Inheritance.ForEach(s =>
                        {
                            SetHref(s, yaml.Indexer, current.Name);
                        });
                    }

                    if (current.Syntax != null && current.Syntax.Parameters != null)
                    {
                        current.Syntax.Parameters.ForEach(s =>
                        {
                            Debug.Assert(s.Type != null);
                            SetHref(s.Type, yaml.Indexer, current.Name);
                        });
                    }

                    if (current.Syntax != null && current.Syntax.Return != null && current.Syntax.Return.Type != null)
                    {
                        SetHref(current.Syntax.Return.Type, yaml.Indexer, current.Name);
                    }

                    return Task.FromResult(true);
                }
                ).Wait();

            return new ParseResult(ResultLevel.Success);
        }

        private static void SetHref(SourceDetail s, Dictionary<string, MetadataItem> index, string currentName)
        {
            if (s == null) return;
            if (!s.IsExternalPath)
            {
                s.Href = ResolveInternalLink(index, s.Name, currentName);
            }
            else
            {
                // Set ExternalPath to null;
                s.Href = null;
            }
        }

        private static string ResolveInternalLink(Dictionary<string, MetadataItem> index, string name, string currentName)
        {
            return MetadataModelUtility.ResolveApiHrefRelativeToCurrentApi(index, name, currentName);
        }
    }
}
