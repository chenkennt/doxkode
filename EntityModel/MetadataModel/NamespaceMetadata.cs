using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityModel
{
    public class NamespaceMetadata : BaseMetadata
    {
        /// <summary>
        /// Use NamespaceMemberMetadata instead of INamesapceMember otherwise it can not be instantiated in Json Deserialize
        /// </summary>
        private List<NamespaceMemberMetadata> _members = new List<NamespaceMemberMetadata>();

        public List<NamespaceMemberMetadata> Members { get { return _members; } set { _members = value; } }
    }

    public class DocumentationMetadata
    {
        public IdentityMapping<NamespaceMetadata> Namespaces { get; set; }
        public IdentityMapping<IMetadata> AllMembers { get; set; }
    }

    public class ProjectMetadata
    {
        public string ProjectName { get; set; }

        public IdentityMapping<NamespaceMetadata> Namespaces { get; set; }
    }

    public class IdentityMapping<T> : SortedDictionary<Identity, T>
    {
        public T GetOrAdd(Identity key, T value)
        {
            T outValue;
            if (this.TryGetValue(key, out outValue))
            {
                return outValue;
            }
            else
            {
                this.Add(key, value);
                return value;
            }
        }
    }

}
