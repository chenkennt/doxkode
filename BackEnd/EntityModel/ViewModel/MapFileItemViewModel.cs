using DocAsCode.Utility;
using System.Collections.Generic;
using System.IO;

namespace DocAsCode.EntityModel
{
    public class MapFileViewModel : Dictionary<string, MapFileItemViewModel>
    {
    }

    public class MapFileItemViewModel
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "id")]
        public string Id { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "path")]
        public string Path { get; set; }

        /// <summary>
        /// Similar to yaml, href is the location of md in the generated website, currently it is copied to md folder
        /// </summary>
        [YamlDotNet.Serialization.YamlMember(Alias = "href")]
        public string Href { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "startLine")]
        public int Startline { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "endLine")]
        public int Endline { get; set; }

        /// <summary>
        /// The start line in the referencing file that define the reference
        /// </summary>
        [YamlDotNet.Serialization.YamlIgnore]
        public int DefinedLine { get; set; }

        [YamlDotNet.Serialization.YamlIgnore]
        public string MarkdownContent { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "startLine")]
        public int ContentStartIndex { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "endLine")]
        public int ContentEndIndex { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "referenceStartLine")]
        public int ReferenceStartLine { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "referenceEndLine")]
        public int ReferenceEndLine { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "remote")]
        public GitDetail Remote { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "references")]
        public List<MapFileItemViewModel> References { get; set; }

        /// <summary>
        /// To override yaml settings
        /// </summary>
        [YamlDotNet.Serialization.YamlMember(Alias = "override")]
        public Dictionary<string, object> CustomProperties { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as MapFileItemViewModel;
            if (other == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(this.Id) && string.IsNullOrEmpty(other.Id))
            {
                return true;
            }
            else if (string.IsNullOrEmpty(this.Id) || string.IsNullOrEmpty(other.Id))
            {
                return false;
            }

            return this.Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            if (this.Id == null)
            {
                return string.Empty.GetHashCode();
            }

            return this.Id.GetHashCode();
        }

        public override string ToString()
        {
            using (StringWriter writer = new StringWriter())
            {
                YamlUtility.Serialize(writer, this);
                return writer.ToString();
            }
        }
    }
}
