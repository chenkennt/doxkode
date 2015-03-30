using DocAsCode.Utility;
using System.Collections.Generic;
using System.IO;

namespace DocAsCode.EntityModel
{
    public class MarkdownIndex
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "id")]
        public string ApiName { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "path")]
        public string Path { get; set; }

        /// <summary>
        /// Similar to yaml, href is the location of md in the generated website, currently it is copied to md folder
        /// </summary>
        [YamlDotNet.Serialization.YamlMember(Alias = "href")]
        public string Href { get; set; }

        [YamlDotNet.Serialization.YamlIgnore]
        public int Startline { get; set; }

        [YamlDotNet.Serialization.YamlIgnore]
        public string MarkdownContent { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "startLine")]
        public int ContentStartIndex { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "endLine")]
        public int ContentEndIndex { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "remote")]
        public GitDetail Remote { get; set; }

        /// <summary>
        /// To override yaml settings
        /// </summary>
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

            var other = obj as MarkdownIndex;
            if (other == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(this.ApiName) && string.IsNullOrEmpty(other.ApiName))
            {
                return true;
            }
            else if (string.IsNullOrEmpty(this.ApiName) || string.IsNullOrEmpty(other.ApiName))
            {
                return false;
            }

            return this.ApiName.Equals(other.ApiName);
        }

        public override int GetHashCode()
        {
            if (this.ApiName == null)
            {
                return string.Empty.GetHashCode();
            }

            return this.ApiName.GetHashCode();
        }

        public override string ToString()
        {
            using (StringWriter writer = new StringWriter())
            {
                YamlUtility.Serializer.Serialize(writer, this);
                return writer.ToString();
            }
        }
    }
}
