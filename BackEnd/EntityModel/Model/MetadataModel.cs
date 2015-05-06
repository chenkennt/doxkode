using System.Collections.Generic;
using System.IO;
using DocAsCode.Utility;

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

    public class MetadataModelUtility
    {
        public static string ResolveApiHrefRelativeToCurrent(Dictionary<string, MetadataItem> index, string name, string currentHref)
        {
            if (string.IsNullOrEmpty(name) || index == null) return name;
            MetadataItem item;
            if (index.TryGetValue(name, out item))
            {
                if (string.IsNullOrEmpty(currentHref)) return item.Href;
                var directoryName = Path.GetDirectoryName(currentHref);
                return FileExtensions.MakeRelativePath(directoryName, item.Href);
            }

            return name;
        }

        public static string ResolveApiHrefRelativeToCurrentApi(Dictionary<string, MetadataItem> index, string name, string currentApiName)
        {
            if (string.IsNullOrEmpty(name) || index == null) return name;
            MetadataItem item;
            if (index.TryGetValue(name, out item))
            {
                MetadataItem currentApi;
                if (!index.TryGetValue(currentApiName, out currentApi)) return item.Href;
                var currentHref = currentApi.Href;
                if (string.IsNullOrEmpty(currentHref)) return item.Href;
                var directoryName = Path.GetDirectoryName(currentHref);
                return FileExtensions.MakeRelativePath(directoryName, item.Href);
            }

            return name;
        }
    }
}
