﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityModel
{
    public class Identity
    {
        Tuple<string, string[]> _fullyQualifiedName;

        public Identity(string fullyQualifiedName, params string[] suffix)
        {
            _fullyQualifiedName = Tuple.Create(fullyQualifiedName, suffix);
        }

        public Identity(ISymbol symbol)
        {
            if (symbol == null)
            {
                _fullyQualifiedName = Tuple.Create(string.Empty, (string[])null);
            }
            else
            {
                _fullyQualifiedName = Tuple.Create(symbol.GetDocumentationCommentId(), (string[])null);
            }
        }

        public override string ToString()
        {
            if (_fullyQualifiedName.Item2 == null)
            {
                return _fullyQualifiedName.Item1;
            }

            return "(" + _fullyQualifiedName.Item1 + ", " + string.Join(",", _fullyQualifiedName.Item2) + ")";
        }

        public static Identity Empty
        {
            get
            {
                return new Identity(string.Empty);
            }
        }
    }
}
