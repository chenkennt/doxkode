using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityModel
{
    public class NamespaceMemberMetadata : BaseMetadata, INamespaceMember
    {
        private List<NamespaceMembersMemberMetadata> _members = new List<NamespaceMembersMemberMetadata>();

        public List<NamespaceMembersMemberMetadata> Members { get { return _members; } set { _members = value; } }

        public List<Identity> InheritanceHierarchy { get; set; }

        /// <summary>
        /// There could be classes with the same name in diffrent projects within one solution, so DocumentationId is not unique cross projects
        /// In other words, there could be same DocumentationId inside one namespace, adding projectName to make it unique
        /// </summary>
        public string ProjectName { get; set; }
    }
}
