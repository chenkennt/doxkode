using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace DocAsCode.EntityModel
{
    /// <summary>
    /// Doc metadata is per project
    /// </summary>
    public class AssemblyDocMetadata : DocMetadata
    {
        private ConcurrentDictionary<Identity, NamespaceDocMetadata> _namespaces = new ConcurrentDictionary<Identity, NamespaceDocMetadata>();

        public IEnumerable<NamespaceDocMetadata> Namespaces
        {
            get
            {
                if(_namespaces == null || _namespaces.Count == 0)
                {
                    return null;
                }
                else
                {
                    return _namespaces.Values.OrderBy(q => q.Id.ToString()).ToList();
                }
            }
            set
            {
                _namespaces = new ConcurrentDictionary<Identity, NamespaceDocMetadata>(value.ToDictionary(s => s.Id, s => s));
            }
        }

        public AssemblyDocMetadata()
        {
        }

        public AssemblyDocMetadata(string name) : base(name)
        {
        }

        public bool TryAddNamespace(NamespaceDocMetadata mta)
        {
            bool flag =  _namespaces.TryAdd(mta.Id, mta);
            if (flag)
            {
                mta.Parent = this.Id;
            }
            return flag;
        }

        /// <summary>
        /// TODO: implement this to return all the supported DocMetadata
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DocMetadata> GetChildrenDocMetadata()
        {
            throw new NotImplementedException();
            foreach(var ns in Namespaces)
            {
                yield return ns;
                // Namespaces' Children
            }
        }
    }

    public class MemberDocMetadata : DocMetadata
    {
        public Identity Namespace { get; set; }

        public Identity Assembly { get; set; }

        public SyntaxDocFragment Syntax { get; set; }

        public MemberDocMetadata() { }
        public MemberDocMetadata(string name) : base(name)
        {
        }

        public MemberDocMetadata(DocMetadata mta) : base(mta.Id)
        {
            this.MscorlibVersion = mta.MscorlibVersion;
            this.XmlDocumentation = mta.XmlDocumentation;
        }
    }

    public class CompositeDocMetadata : MemberDocMetadata
    {
        protected Dictionary<MemberType, ConcurrentDictionary<Identity, DocMetadata>> _members =
            new Dictionary<MemberType, ConcurrentDictionary<Identity, DocMetadata>>();

        public List<MemberType> AllowedMemberTypes { get; set; }

        private Stack<Identity> _inheritancehierarchy;
        public string FilePath { get; set; }

        public Stack<Identity> InheritanceHierarchy
        {
            get
            {
                return _inheritancehierarchy;
            }
            set
            {
                _inheritancehierarchy = new Stack<Identity>(value);
            }
        }


        [JsonIgnore]
        public IEnumerable<MemberDocMetadata> Members
        {
            get
            {
                List<MemberDocMetadata> list = new List<MemberDocMetadata>();
                for (int i = 0; i < AllowedMemberTypes.Count; i++)
                {
                    var type = AllowedMemberTypes[i];
                    if (_members.ContainsKey(type))
                    {
                        foreach (var value in _members[type].Values)
                        {
                            list.Add(value as MemberDocMetadata);
                        }
                    }
                }
                return list;
            }
        }

        [JsonIgnore]
        public Dictionary<MemberType, ConcurrentDictionary<Identity, DocMetadata>> MemberDict
        {
            get { return _members; }
            set
            {
                _members = value;
            }
        }

        protected IEnumerable<T> GetMetadata<T>(MemberType type) where T : DocMetadata
        {
            List<T> list = null;
            if (_members != null && _members.ContainsKey(type))
            {
                foreach (var member in _members[type].Values)
                {
                    if (list == null)
                    {
                        list = new List<T>();
                    }
                    list.Add(member as T);
                }
            }

            if (list != null)
            {
                list = list.OrderBy(q => q.Id.ToString()).ToList();
            }

            return list;
        }

        protected void SetMetadata<T>(MemberType type, IEnumerable<T> value) where T : DocMetadata
        {
            if (_members != null && _members.ContainsKey(type))
            {
                foreach (var member in value)
                {
                    _members[type].TryAdd(member.Id, member);
                }
            }

        }

        public bool TryAdd(MemberDocMetadata metadata, MemberType type)
        {
            if (!AllowedMemberTypes.Contains(type))
            {
                throw new Exception(string.Format("The metadata of type {0} cannot be generated.", type.ToString()));
            }

            bool flag =  _members[type].TryAdd(metadata.Id, metadata);
            if (flag)
            {
                metadata.Parent = this.Id;
            }
            return flag;
        }
         
        public IEnumerable<DocMetadata> GetMemberType(MemberType type)
        {
            if (!AllowedMemberTypes.Contains(type))
            {
                throw new Exception(string.Format("The metadata of type {0} is not allowed.", type.ToString()));
            }
            if (_members.ContainsKey(type))
            {
                return _members[type].Values;
            }
            else
            {
                return new List<DocMetadata>();
            }
        }

        protected void Init()
        {
            if (AllowedMemberTypes != null)
            {
                foreach(var i in AllowedMemberTypes)
                {
                    _members.Add(i, new ConcurrentDictionary<Identity, DocMetadata>());
                }
            }
        }
        public CompositeDocMetadata() {
        }

        public CompositeDocMetadata(string name) : base(name)
        {
        }

        public CompositeDocMetadata(DocMetadata mta) : base(mta)
        {
        }

        public override void WriteMarkdownSkeleton(TextWriter writer)                                    
        {
            base.WriteMarkdownSkeleton(writer);

            foreach (var member in Members)
            {
                member.WriteMarkdownSkeleton(writer);
            }
        }
    }

    /// <summary>
    /// TODO: what if no namespace?
    /// </summary>
    public class NamespaceDocMetadata : CompositeDocMetadata
    {

        public IEnumerable<ClassDocMetadata> Classes
        {
            get
            {
                return GetMetadata<ClassDocMetadata>(MemberType.Class);
            }
            set
            {
                SetMetadata<ClassDocMetadata>(MemberType.Class, value);
            }
        }

        public IEnumerable<InterfaceDocMetadata> Interfaces
        {
            get
            {
                return GetMetadata<InterfaceDocMetadata>(MemberType.Interface);
            }
            set
            {
                SetMetadata<InterfaceDocMetadata>(MemberType.Interface, value);
            }
        }

        public IEnumerable<StructDocMetadata> Structs
        {
            get
            {
                return GetMetadata<StructDocMetadata>(MemberType.Struct);
            }
            set
            {
                SetMetadata<StructDocMetadata>(MemberType.Struct, value);
            }
        }

        public IEnumerable<DelegateDocMetadata> Delegates
        {
            get
            {
                return GetMetadata<DelegateDocMetadata>(MemberType.Delegate);
            }
            set
            {
                SetMetadata<DelegateDocMetadata>(MemberType.Delegate, value);
            }
        }

        public IEnumerable<EnumDocMetadata> Enums
        {
            get
            {
                return GetMetadata<EnumDocMetadata>(MemberType.Enum);
            }
            set
            {
                SetMetadata<EnumDocMetadata>(MemberType.Enum, value);
            }
        }

        public NamespaceDocMetadata()
        {
            AllowedMemberTypes = new List<MemberType>()
            {
                MemberType.Class,
                MemberType.Interface,
                MemberType.Struct,
                MemberType.Delegate,
                MemberType.Enum
            };

           base.Init();
        }

        public NamespaceDocMetadata(string name) : base(name)
        {
            AllowedMemberTypes = new List<MemberType>()
            {
                MemberType.Class,
                MemberType.Interface,
                MemberType.Struct,
                MemberType.Delegate,
                MemberType.Enum
            };
            base.Init();
        }

        public NamespaceDocMetadata(DocMetadata mta) : base(mta)
        {
            AllowedMemberTypes = new List<MemberType>()
            {
                MemberType.Class,
                MemberType.Interface,
                MemberType.Struct,
                MemberType.Delegate,
                MemberType.Enum
            };
            base.Init();
        }
    }

    public class ClassDocMetadata : CompositeDocMetadata
    {
        public IEnumerable<MethodDocMetadata> Methods
        {
            get
            {
                return GetMetadata<MethodDocMetadata>(MemberType.Method);
            }
            set
            {
                SetMetadata<MethodDocMetadata>(MemberType.Method, value);
            }
        }

        public IEnumerable<FieldDocMetadata> Fields
        {
            get
            {
                return GetMetadata<FieldDocMetadata>(MemberType.Field);
            }
            set
            {
                SetMetadata<FieldDocMetadata>(MemberType.Field, value);
            }
        }

        public IEnumerable<PropertyDocMetadata> Properties
        {
            get
            {
                return GetMetadata<PropertyDocMetadata>(MemberType.Property);
            }
            set
            {
                SetMetadata<PropertyDocMetadata>(MemberType.Property, value);
            }
        }

        public IEnumerable<EventDocMetadataDefinition> Events
        {
            get
            {
                return GetMetadata<EventDocMetadataDefinition>(MemberType.Event);
            }
            set
            {
                SetMetadata<EventDocMetadataDefinition>(MemberType.Event, value);
            }
        }

        public IEnumerable<ConstructorDocMetadata> Constructors
        {
            get
            {
                return GetMetadata<ConstructorDocMetadata>(MemberType.Constructor);
            }
            set
            {
                SetMetadata<ConstructorDocMetadata>(MemberType.Constructor, value);
            }
        }

        public ClassDocMetadata()
        {
            AllowedMemberTypes = new List<MemberType>
            {
                MemberType.Field,
                MemberType.Property,
                MemberType.Event,
                MemberType.Constructor,
                MemberType.Method,
            };
            base.Init();
        }

        public ClassDocMetadata(string name) : base(name)
        {
            AllowedMemberTypes = new List<MemberType>
            {
                MemberType.Field,
                MemberType.Property,
                MemberType.Event,
                MemberType.Constructor,
                MemberType.Method,
            };
            base.Init();
        }

        public ClassDocMetadata(DocMetadata mta) : base(mta)
        {
            AllowedMemberTypes = new List<MemberType>
            {
                MemberType.Field,
                MemberType.Property,
                MemberType.Event,
                MemberType.Constructor,
                MemberType.Method,
            };
            base.Init();
        }

        /*public MethodDocMetadata CreateMethod(string name)
        {
            return _members[SubMemberType.Method].GetOrAdd(name, s => new DocMetadata(s)
            {
                MscorlibVersion = this.MscorlibVersion,
                Assembly = this.Assembly,
                Namespace = this.Namespace,
            });
        }*/
    }

    public class MethodDocMetadata : MemberDocMetadata
    {
        public MethodSyntax MethodSyntax
        {
            get
            {
                return this.Syntax as MethodSyntax; 
            }
            set
            {
                this.Syntax = value;
            }
        }

        public MethodDocMetadata() { }

        public MethodDocMetadata(string name) : base(name)
        {
        }

        public MethodDocMetadata(DocMetadata mta) : base(mta)
        {
        }
    }
    public class ConstructorDocMetadata : MemberDocMetadata
    {
        public ConstructorSyntax ConstructorSyntax
        {
            get
            {
                return this.Syntax as ConstructorSyntax;
            }
            set
            {
                this.Syntax = value;
            }
        }
        public ConstructorDocMetadata() { }

        public ConstructorDocMetadata(string name) : base(name)
        {
        }

        public ConstructorDocMetadata(DocMetadata mta) : base(mta)
        {
        }
    }

    public class PropertyDocMetadata : MemberDocMetadata
    {
        public PropertySyntax PropertySyntax
        {
            get
            {
                return this.Syntax as PropertySyntax;
            }
            set
            {
                this.Syntax = value;
            }
        }

        public PropertyDocMetadata() { }
        public PropertyDocMetadata(string name) : base(name)
        {
        }
        public PropertyDocMetadata(DocMetadata mta) : base(mta)
        {
        }
    }

    public class FieldDocMetadata : MemberDocMetadata
    {
        public FieldDocMetadata() { }
        public FieldDocMetadata(string name) : base(name)
        {
        }
        public FieldDocMetadata(DocMetadata mta) : base(mta)
        {
        }
    }

    public class EventDocMetadataDefinition : MemberDocMetadata
    {
        public EventDocMetadataDefinition() { }
        public EventDocMetadataDefinition(string name) : base(name)
        {
        }
        public EventDocMetadataDefinition(DocMetadata mta) : base(mta)
        {
        }
    }

    public class InterfaceDocMetadata : CompositeDocMetadata
    {
        public IEnumerable<MethodDocMetadata> Methods
        {
            get
            {
                return GetMetadata<MethodDocMetadata>(MemberType.Method);
            }
            set
            {
                SetMetadata<MethodDocMetadata>(MemberType.Method, value);
            }
        }

        public IEnumerable<PropertyDocMetadata> Properties
        {
            get
            {
                return GetMetadata<PropertyDocMetadata>(MemberType.Property);
            }
            set
            {
                SetMetadata<PropertyDocMetadata>(MemberType.Property, value);
            }
        }

        public IEnumerable<EventDocMetadataDefinition> Events
        {
            get
            {
                return GetMetadata<EventDocMetadataDefinition>(MemberType.Event);
            }
            set
            {
                SetMetadata<EventDocMetadataDefinition>(MemberType.Event, value);
            }
        }
        public InterfaceDocMetadata()
        {
            AllowedMemberTypes = new List<MemberType>
            {
                MemberType.Property,
                MemberType.Event,
                MemberType.Method,
            };
            base.Init();
        }
        public InterfaceDocMetadata(string name) : base(name)
        {
            AllowedMemberTypes = new List<MemberType>
            {
                MemberType.Property,
                MemberType.Event,
                MemberType.Method,
            };
            base.Init();
        }
        public InterfaceDocMetadata(DocMetadata mta) : base(mta)
        {
            AllowedMemberTypes = new List<MemberType>
            {
                MemberType.Property,
                MemberType.Event,
                MemberType.Method,
            };
            base.Init();
        }
    }

    public class StructDocMetadata : CompositeDocMetadata
    {
        public IEnumerable<MethodDocMetadata> Methods
        {
            get
            {
                return GetMetadata<MethodDocMetadata>(MemberType.Method);
            }
            set
            {
                SetMetadata<MethodDocMetadata>(MemberType.Method, value);
            }
        }

        public IEnumerable<FieldDocMetadata> Fields
        {
            get
            {
                return GetMetadata<FieldDocMetadata>(MemberType.Field);
            }
            set
            {
                SetMetadata<FieldDocMetadata>(MemberType.Field, value);
            }
        }

        public IEnumerable<PropertyDocMetadata> Properties
        {
            get
            {
                return GetMetadata<PropertyDocMetadata>(MemberType.Property);
            }
            set
            {
                SetMetadata<PropertyDocMetadata>(MemberType.Property, value);
            }
        }

        public IEnumerable<EventDocMetadataDefinition> Events
        {
            get
            {
                return GetMetadata<EventDocMetadataDefinition>(MemberType.Event);
            }
            set
            {
                SetMetadata<EventDocMetadataDefinition>(MemberType.Event, value);
            }
        }

        public IEnumerable<ConstructorDocMetadata> Constructors
        {
            get
            {
                return GetMetadata<ConstructorDocMetadata>(MemberType.Constructor);
            }
            set
            {
                SetMetadata<ConstructorDocMetadata>(MemberType.Constructor, value);
            }
        }

        public StructDocMetadata()
        {
            AllowedMemberTypes = new List<MemberType>
            {
                MemberType.Field,
                MemberType.Property,
                MemberType.Event,
                MemberType.Constructor,
                MemberType.Method,
            };
            base.Init();
        }
        public StructDocMetadata(string name) : base(name)
        {
            AllowedMemberTypes = new List<MemberType>
            {
                MemberType.Field,
                MemberType.Property,
                MemberType.Event,
                MemberType.Constructor,
                MemberType.Method,
            };
            base.Init();
        }
        public StructDocMetadata(DocMetadata mta) : base(mta)
        {
            AllowedMemberTypes = new List<MemberType>
            {
                MemberType.Field,
                MemberType.Property,
                MemberType.Event,
                MemberType.Constructor,
                MemberType.Method,
            };
            base.Init();
        }
    }

    public class DelegateDocMetadata : MemberDocMetadata
    {

        public DelegateDocMetadata() { }
        public DelegateDocMetadata(string name) : base(name)
        {
        }
        public DelegateDocMetadata(DocMetadata mta) : base(mta)
        {
        }
    }

    public class EnumDocMetadata : MemberDocMetadata
    {

        public EnumDocMetadata() { }
        public EnumDocMetadata(string name) : base(name)
        {
        }
        public EnumDocMetadata(DocMetadata mta) : base(mta)
        {
        }
    }

    public class DocMetadata
    {
        public Identity Id { get; set; }

        public Identity Parent { get; set; }

        public MemberType MemberType { get; set; }

        public string XmlDocumentation { get; set; }

        public string MarkdownContent { get; set; }

        public Version MscorlibVersion { get; set; }
        public DocMetadata()
        {
        }

        public DocMetadata(string name)
        {
            this.Id = new Identity(name);
            this.Parent = null;
        }

        /// <summary>
        /// TODO: use razor engine to generate html
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="additionalContent"></param>
        public virtual void WriteHtml(TextWriter writer, string additionalContent)
        {
            writer.WriteLine(string.Format("<h>{0}</h>", Id));
            writer.WriteLine(string.Format("<p>{0}<p>"), XmlDocumentation);
            writer.WriteLine(string.Format("<p>{0}<p>"), additionalContent);
        }


        /// <summary>
        /// Export Markdown file
        /// ---
        /// M:SDSearchLib.Managers.ConsoleManager.ShowWindow(System.Boolean)
        /// ---
        /// </summary>
        /// <param name="stream"></param>
        public virtual void WriteMarkdownSkeleton(TextWriter writer)
        {
            writer.WriteLine(ConvertCommentIdToYamlHeader(this.Id));
        }

        private string ConvertCommentIdToYamlHeader(string commendId)
        {
            if (string.IsNullOrEmpty(commendId))
            {
                throw new ArgumentNullException("commendId");
            }

            return (string)new CommentIdToYamlHeaderConverter().ConvertTo(commendId, typeof(string));
        }
    }

    /// <summary>
    /// http://msdn.microsoft.com/zh-cn/library/system.exception.helplink(v=vs.110).aspx
    /// </summary>
    public class PropertySyntax : SyntaxDocFragment
    {
        public Identity PropertyType { get; set; }

        public IReadOnlyList<Identity> Implements { get; set; }
    }

    /// <summary>
    /// http://msdn.microsoft.com/zh-cn/library/tz6bzkbf(v=vs.110).aspx
    /// </summary>
    public class ConstructorSyntax : SyntaxDocFragment
    {
        public SortedDictionary<string, string> Parameters { get; set; }
    }

    /// <summary>
    /// http://msdn.microsoft.com/zh-cn/library/tz6bzkbf(v=vs.110).aspx
    /// </summary>
    public class MethodSyntax : SyntaxDocFragment
    {
        public SortedDictionary<string, string> Parameters { get; set; }
        public Dictionary<string, string> Returns { get; set; }

        public IReadOnlyList<Identity> Implements { get; set; }
    }

    public class SyntaxDocFragment
    {
        public string Content { get; set; }

        /// <summary>
        /// TODO: how to merge comments in different language
        /// </summary>
        public string Language { get; set; }

        public string XmlDocumentation { get; set; }

        public int StartLine { get; set; }
    }

    public class Identity
    {
        string _fullyQualifiedName;

        public Identity(string fullyQualifiedName)
        {
            _fullyQualifiedName = fullyQualifiedName;
        }

        public override string ToString()
        {
            return _fullyQualifiedName;
        }

        public static implicit operator string (Identity memberIdentity)
        {
            return memberIdentity.ToString();
        }

        public static implicit operator Identity(string name)
        {
            return new Identity(name);
        }
    }

    public enum MemberType
    {
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
