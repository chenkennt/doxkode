﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityModel
{
    public interface IMetadataVisitor<TContext>
    {
        Task VisitAsync(IMetadata metadata, TContext context);

        Task VisitAsync(INamespaceMember metadata, TContext context);

        Task VisitAsync(INamespaceMembersMember metadata, TContext context);
    }
}