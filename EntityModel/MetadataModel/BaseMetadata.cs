using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityModel
{
    public class BaseMetadata : IMetadata
    {
        public Identity Identity { get; set; }

        [JsonIgnore]
        public IMetadata Parent { get; set; }

        [JsonIgnore]
        public IMetadata OwnerNamespace { get; set; }

        public MemberType MemberType { get; set; }

        public Version MscorlibVersion { get; set; }

        public string AssemblyName { get; set; }

        public string FilePath { get; set; }

        /// <summary>
        /// Use SyntaxDescriptionGroup instead of ISyntaxDescriptionGroup for instantiation in JsonDeserialization
        /// </summary>
        public SyntaxDescriptionGroup SyntaxDescriptionGroup { get; set; }

        public Task AcceptAsync<TContext>(IMetadataVisitor<TContext> visitor, TContext context)
        {
            return visitor.VisitAsync(this, context);
        }
    }
}
