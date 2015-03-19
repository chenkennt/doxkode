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
