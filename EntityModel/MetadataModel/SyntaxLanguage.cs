using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityModel
{
    public enum SyntaxLanguage
    {
        CSharp,
        CPlusPlus,
        FSharp,
        Javascript,
        VB,
    }

    public class DescriptionConstants
    {
        public const string ReturnName = "ReturnName";
        public const string PropertyName = "PropertyName";
    }
}
