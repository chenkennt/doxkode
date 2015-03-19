using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityModel
{
    public interface INamespaceMember : IHierarchy
    {
        string ProjectName { get; set; }
    }

    public enum NamespaceMemberType
    {
        Class,
        Structure,
        Interface,
        Enumeration,
        Delegate,
    }
}
