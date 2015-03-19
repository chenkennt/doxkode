using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityModel
{
    public interface INamespaceMembersMember
    {
        SyntaxDescriptionGroup SyntaxDescriptionGroup { get; set; }
    }

    public enum NamespaceMembersMemberType
    {
        Method,
        Field,
        Property,
        Event,
        Ctor,
    }
}
