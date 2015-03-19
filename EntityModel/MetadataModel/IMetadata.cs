using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityModel
{
    public static class MetadataConstant
    {
        public const string MemberType = "MemberType";
        public const string SyntaxType = "SyntaxType";
    }

    public interface IMetadata
    {
        Identity Identity { get; set; }

        IMetadata Parent { get; set; }

        IMetadata OwnerNamespace { get; set; }

        MemberType MemberType { get; set; }

        Version MscorlibVersion { get; set; }

        string FilePath { get; set; }

        string AssemblyName { get; set; }

        SyntaxDescriptionGroup SyntaxDescriptionGroup { get; set; }

        Task AcceptAsync<TContext>(IMetadataVisitor<TContext> visitor, TContext context);
    }

    public enum MemberType
    {
        Default,
        Toc,
        Assembly,
        Namespace,
        Class,
        Interface,
        Struct,
        Delegate,
        Enum,
        Field,
        Property,
        Event,
        Constructor,
        Method,
    }
}
