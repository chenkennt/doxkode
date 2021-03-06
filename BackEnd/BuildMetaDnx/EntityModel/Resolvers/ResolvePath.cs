﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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
                            if (!s.IsExternalPath)
                            {
                                s.Href = ResolveInternalLink(yaml.Indexer, s.Name);
                            }
                        });
                    }

                    if (current.Syntax != null && current.Syntax.Parameters != null)
                    {
                        current.Syntax.Parameters.ForEach(s =>
                        {
                            Debug.Assert(s.Type != null);
                            if (s.Type != null && !s.Type.IsExternalPath)
                            {
                                s.Type.Href = ResolveInternalLink(yaml.Indexer, s.Type.Name);
                            }
                        });
                    }

                    if (current.Syntax != null && current.Syntax.Return != null && current.Syntax.Return.Type != null)
                    {
                        current.Syntax.Return.Type.Href = ResolveInternalLink(yaml.Indexer, current.Syntax.Return.Type.Name);
                    }

                    return Task.FromResult(true);
                }
                ).Wait();

            return new ParseResult(ResultLevel.Success);
        }

        private static string ResolveInternalLink(Dictionary<string, MetadataItem> index, string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            if (string.IsNullOrEmpty(name)) return name;

            MetadataItem item;
            if (index.TryGetValue(name, out item))
            {
                return item.Href;
            }

            return name;
        }
    }
}
