using System.Collections.Generic;
namespace DocAsCode.EntityModel
{
    public class MetadataModel
    {
        public Dictionary<string, MetadataItem> Indexer { get; set; }
        public MetadataItem TocYamlViewModel { get; set; }
        public List<MetadataItem> Members { get; set; }
    }
    public class YamlViewModel1
    {
        public List<ItemViewModel> TocYamlListViewModel { get; set; }
        public ItemViewModel TocYamlViewModel { get; set; }
        public List<ItemViewModel> MemberYamlViewModelList { get; set; }
    }
}
