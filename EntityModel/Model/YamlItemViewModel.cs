using System;
using System.Collections.Generic;

namespace DocAsCode.EntityModel
{
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

        [YamlDotNet.Serialization.YamlMember(Alias = "language")]
        public SyntaxLanguage Language { get; set; }

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
}
