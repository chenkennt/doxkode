using System;
using System.Collections.Generic;
using System.Linq;

namespace DocAsCode.EntityModel
{
    using System.Data;
    using System.Diagnostics;

    public class TocViewModel : List<ItemViewModel>
    {
        private static List<string> TocMetadataList = new List<string>
                                                           {
                                                               "uid",
                                                               "name",
                                                               "href",
                                                           };
        public TocViewModel() : base() { }
        public TocViewModel(IEnumerable<ItemViewModel> list)
            : base(list)
        {
        }

        public static TocViewModel Convert(MetadataItem item)
        {
            Debug.Assert(item.Type == MemberType.Toc);
            if (item.Type != MemberType.Toc) return null;
            var tocList = new TocViewModel();

            foreach (var namespaceItem in item.Items)
            {
                var namespaceTocViewModel = ItemViewModel.Convert(namespaceItem, TocMetadataList);
                if (namespaceItem.Items != null)
                {
                    var tocSubList = new TocViewModel();
                    foreach (var classItem in namespaceItem.Items)
                    {
                        var classTocViewModel = ItemViewModel.Convert(classItem, TocMetadataList);
                        tocSubList.Add(classTocViewModel);
                    }
                    namespaceTocViewModel["items"] = tocSubList;
                }

                tocList.Add(namespaceTocViewModel);
            }

            return tocList;
        }     
    }

    public class OnePageViewModel
    {
        private static List<string> FullMetadataList = new List<string>
                                                           {
                                                               "uid",
                                                               "href",
                                                               "name",
                                                               "fullName",
                                                               "type",
                                                               "source",
                                                               "summary",
                                                               "syntax",
                                                               "inheritance",
                                                               "parent",
                                                               "id",
                                                               "children",
                                                           };

        private static List<string> LiteMetadataList = new List<string>
                                                           {
                                                               "uid", "href", "name", "type", "summary"
                                                           };
        [YamlDotNet.Serialization.YamlMember(Alias = "items")]
        public List<ItemViewModel> Items { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "references")]
        public List<ItemViewModel> References { get; set; }

        public static OnePageViewModel Convert(MetadataItem item)
        {
            List<ItemViewModel> items = new List<ItemViewModel>();
            List<ItemViewModel> refs = new List<ItemViewModel>();
            var mainItem = ItemViewModel.Convert(item, FullMetadataList);
            items.Add(mainItem);
            if ( item.Items != null)
            {
                if (item.Type.AllowMultipleItems())
                {
                    items.AddRange(item.Items.Select(s => ItemViewModel.Convert(s, FullMetadataList)));
                }
                else
                {
                    refs.AddRange(item.Items.Select(s => ItemViewModel.Convert(s, LiteMetadataList)));
                }
            }
            
            return new OnePageViewModel
                       {
                           Items = items,
                           References = refs.Count > 0 ? refs : null,
                       };
        }
    }
}
