﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityModel
{
    public interface IDescription
    {
        List<IComment> Comments { get; set; }
    }

    public interface ISyntaxDescription : IDescription
    {
        string Syntax { get; set; }
    }

    public interface ISyntaxDescriptionGroup : IDictionary<SyntaxLanguage, SyntaxDescription>
    {
    }

    public class SyntaxDescriptionGroup : Dictionary<SyntaxLanguage, SyntaxDescription>, ISyntaxDescriptionGroup
    {
    }

    public class SyntaxDescription : ISyntaxDescription
    {
        /// <summary>
        /// e.g. public class A : B
        /// </summary>
        public string Syntax { get; set; }

        public List<IComment> Comments { get; set; }
    }

    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/system.exception.helplink(v=vs.110).aspx
    /// </summary>
    public class PropertySyntaxDescription : SyntaxDescription
    {
        public ParameterDescription Property { get; set; }
    }

    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/tz6bzkbf(v=vs.110).aspx
    /// </summary>
    public class ConstructorSyntaxDescription : SyntaxDescription
    {
        private List<ParameterDescription> _parameters = new List<ParameterDescription>();
        public List<ParameterDescription> Parameters { get { return _parameters; } set { _parameters = value; } }
    }

    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/tz6bzkbf(v=vs.110).aspx
    /// </summary>
    public class MethodSyntaxDescription : SyntaxDescription
    {
        private List<ParameterDescription> _parameters = new List<ParameterDescription>();
        public List<ParameterDescription> Parameters { get { return _parameters; } set { _parameters = value; } }

        public ParameterDescription Return { get; set; }
    }

    public class ParameterDescription : IDescription
    {
        public string Name { get; set; }

        public Identity Type { get; set; }

        public List<IComment> Comments { get; set; }
    }

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
