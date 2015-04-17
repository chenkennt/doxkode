using System.Collections.Generic;

namespace DocAsCode.EntityModel
{
    public class SyntaxDetail
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "content")]
        public Dictionary<SyntaxLanguage, string> Content { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "parameters")]
        public List<ApiParameter> Parameters { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "return")]
        public ApiParameter Return { get; set; }
    }
}
