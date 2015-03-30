using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DocAsCode.Utility;

namespace DocAsCode.EntityModel
{
    public class ResolveGitPath : IResolverPipeline
    {
        public ParseResult Run(YamlViewModel yaml, ResolverContext context)
        {
            // Moved to Metadata Visitor
            return new ParseResult(ResultLevel.Success);
        }
    }
}
