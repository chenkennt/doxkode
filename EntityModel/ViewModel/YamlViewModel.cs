using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DocAsCode.Utility;

namespace EntityModel.ViewModel
{
    /// <summary>
    /// Defines the layout for an API page
    /// </summary>
    public enum LayoutItem
    {
        Title,
        Hierarchy,
        Exception,
        Syntax,
        InlineComments,
        ExternalComments,
        SeeAlso,
        MemberTable,
    }

    public class SourceDetail
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "remote")]
        public GitDetail Remote { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "base")]
        public string BasePath { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "id")]
        public string Name { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "name")]
        public string DisplayName { get; set; }

        /// <summary>
        /// The url path for current source, should be resolved at some late stage
        /// </summary>
        [YamlDotNet.Serialization.YamlMember(Alias = "href")]
        public string Href { get; set; }

        /// <summary>
        /// The local path for current source, should be resolved to be relative path at some late stage
        /// </summary>
        [YamlDotNet.Serialization.YamlMember(Alias = "path")]
        public string Path { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "startLine")]
        public int StartLine { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "endLine")]
        public int EndLine { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "content")]
        public string Content { get; set; }

        /// <summary>
        /// The external path for current source if it is not locally available
        /// </summary>
        [YamlDotNet.Serialization.YamlMember(Alias = "isExternal")]
        public bool IsExternalPath { get; set; }
    }

    public class SyntaxDetail
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "content")]
        public Dictionary<SyntaxLanguage, string> Content { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "parameters")]
        public List<YamlItemParameterViewModel> Parameters { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "return")]
        public YamlItemParameterViewModel Return { get; set; }
    }

    public class ItemType
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "name")]
        public string Name { get; set; }
        [YamlDotNet.Serialization.YamlMember(Alias = "description")]
        public string Description { get; set; }
    }

    public class YamlItemViewModel : ICloneable
    {
        [YamlDotNet.Serialization.YamlIgnore]
        public bool IsInvalid { get; set; }

        [YamlDotNet.Serialization.YamlIgnore]
        public string RawComment { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "id")]
        public string Name { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "yaml")]
        public string YamlPath { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "href")]
        public string Href { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "name")]
        public Dictionary<SyntaxLanguage, string> DisplayNames { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "qualifiedName")]
        public Dictionary<SyntaxLanguage, string> DisplayQualifiedNames { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "parent")]
        public string ParentName { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "type")]
        public MemberType Type { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "source")]
        public SourceDetail Source { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "documentation")]
        public SourceDetail Documentation { get; set; }

        public List<LayoutItem> Layout { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "summary")]
        public string Summary { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "remarks")]
        public string Remarks { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "syntax")]
        public SyntaxDetail Syntax { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "inheritance")]
        public List<SourceDetail> Inheritance { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "itemTypes")]
        public List<ItemType> ItemTypes { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "items")]
        public List<YamlItemViewModel> Items { get; set; }

        public override string ToString()
        {
            return Type + ": " + Name;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class YamlViewModel
    {
        public YamlItemViewModel TocYamlViewModel { get; set; }
        public Dictionary<string, IndexYamlItemViewModel> IndexYamlViewModel { get; set; }
        public List<YamlItemViewModel> MemberYamlViewModelList { get; set; }
    }

    public class IndexYamlItemViewModel
    {
        public string Name { get; set; }
        public string YamlPath { get; set; }
        public string Href { get; set; }
    }

    public class YamlItemParameterViewModel
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "id")]
        public string Name { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "type")]
        public SourceDetail Type { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "description")]
        public string Description { get; set; }
    }
}
