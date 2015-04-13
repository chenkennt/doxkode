using System.Collections.Generic;
namespace DocAsCode.EntityModel
{
    public class YamlViewModel
    {
        public List<YamlItemViewModel> TocYamlListViewModel { get; set; }
        public YamlItemViewModel TocYamlViewModel { get; set; }
        public Dictionary<string, IndexYamlItemViewModel> IndexYamlViewModel { get; set; }
        public List<YamlItemViewModel> MemberYamlViewModelList { get; set; }
    }
}
