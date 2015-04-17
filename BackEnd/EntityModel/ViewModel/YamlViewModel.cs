using System;
using System.Collections.Generic;
using System.Linq;

namespace DocAsCode.EntityModel
{
    public class YamlViewModel
    {
        public List<OnePageViewModel> Pages { get; set; } 
        public OnePageViewModel Toc { get; set; }
    }
}
