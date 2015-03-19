using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityModel
{
    public class CommentVisitor<TContext> : IMetadataVisitor<TContext>
    {
        public Task VisitAsync(INamespaceMembersMember metadata, TContext context)
        {
            throw new NotImplementedException();
        }

        public Task VisitAsync(INamespaceMember metadata, TContext context)
        {
            throw new NotImplementedException();
        }

        public Task VisitAsync(IMetadata metadata, TContext context)
        {
            throw new NotImplementedException();
        }
    }
}
