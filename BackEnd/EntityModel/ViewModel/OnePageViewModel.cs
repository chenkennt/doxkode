using System;
using System.Collections.Generic;
using System.Linq;

namespace DocAsCode.EntityModel
{
    public class OnePageViewModel
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "items")]
        public List<ItemViewModel> Items { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "references")]
        public List<ItemViewModel> References { get; set; }

        public static OnePageViewModel Convert(MetadataItem item)
        {
            List<ItemViewModel> items = new List<ItemViewModel>();
            List<ItemViewModel> refs = new List<ItemViewModel>();
            var mainItem = ItemViewModel.Convert(item);
            items.Add(mainItem);
            if ( item.Items != null)
            {
                if (item.Type.AllowMultipleItems())
                {
                    items.AddRange(item.Items.Select(ItemViewModel.Convert));
                }
                else
                {
                    refs.AddRange(item.Items.Select(ItemViewModel.LiteConvert));
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
