using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DocAsCode.EntityModel
{
    public abstract class YamlModelGeneratorVisitor : SymbolVisitor<YamlItemViewModel>
    {
        private YamlItemViewModel parent = new YamlItemViewModel();
        private YamlItemViewModel currentNamespace = new YamlItemViewModel();
        private YamlItemViewModel currentAssembly = new YamlItemViewModel();
        private SyntaxLanguage _language;
        public YamlModelGeneratorVisitor(object context, SyntaxLanguage language)
        {
            _language = language;
        }

        public abstract string GetSyntaxContent(MemberType typeKind, SyntaxNode syntaxNode);

        public override YamlItemViewModel DefaultVisit(ISymbol symbol)
        {
            if (!VisitorHelper.CanVisit(symbol)) return null;
            var item = new YamlItemViewModel
            {
                Name = VisitorHelper.GetId(symbol),
                DisplayNames = new Dictionary<SyntaxLanguage, string>() { { _language, symbol.MetadataName } },
                RawComment = symbol.GetDocumentationCommentXml(),
                Language = _language,
            };

            item.DisplayQualifiedNames = new Dictionary<SyntaxLanguage, string>() { { _language, item.Name } };
            
            item.Source = VisitorHelper.GetSourceDetail(symbol);
            VisitorHelper.FeedComments(item);
            return item;
        }

        public override YamlItemViewModel VisitAssembly(IAssemblySymbol symbol)
        {
            var item = DefaultVisit(symbol);
            if (item == null) return null;
            item.Type = MemberType.Assembly;
            
            parent = item;
            currentAssembly = item;
            foreach (var ns in symbol.GlobalNamespace.GetNamespaceMembers())
            {
                ns.Accept(this);
            }

            return item;
        }

        public override YamlItemViewModel VisitNamespace(INamespaceSymbol symbol)
        {
            var item = DefaultVisit(symbol);
            if (item == null) return null;
            item.Type = MemberType.Namespace;

            Debug.Assert(parent != null && currentAssembly != null);
            if (currentAssembly != null)
            {
                if (currentAssembly.Items == null)
                {
                    currentAssembly.Items = new List<YamlItemViewModel>();
                }

                currentAssembly.Items.Add(item);
            }

            var members = symbol.GetMembers().ToList();
            var currentNamespaceSaved = currentNamespace;
            currentNamespace = item;
            var parentSaved = parent;
            parent = item;
            foreach (var member in members)
            {
                var nsItem = member.Accept(this);
            }
            parent = parentSaved;
            currentNamespace = currentNamespaceSaved;
            return item;
        }

        public override YamlItemViewModel VisitNamedType(INamedTypeSymbol symbol)
        {
            var item = DefaultVisit(symbol);
            if (item == null) return null;

            var syntaxRef = symbol.DeclaringSyntaxReferences.FirstOrDefault();
            Debug.Assert(syntaxRef != null);
            if (syntaxRef == null)
            {
                return null;
            }
            var syntaxNode = syntaxRef.GetSyntax();
            Debug.Assert(syntaxNode != null);
            if (syntaxNode == null)
            {
                return null;
            }

            var type = symbol.BaseType;
            if (type != null)
            {
                item.Inheritance = new List<SourceDetail>();
                while (type != null)
                {
                    SourceDetail link = VisitorHelper.GetLinkDetail(type);

                    item.Inheritance.Add(link);
                    type = type.BaseType;
                }

                item.Inheritance.Reverse();
            }
            Debug.Assert(parent != null && currentNamespace != null);
            if (currentNamespace != null)
            {
                if (currentNamespace.Items == null)
                {
                    currentNamespace.Items = new List<YamlItemViewModel>();
                }

                currentNamespace.Items.Add(item);
            }

            item.Type = VisitorHelper.GetMemberTypeFromTypeKind(symbol.TypeKind);
            string syntaxStr = GetSyntaxContent(item.Type, syntaxNode);
            Debug.Assert(!string.IsNullOrEmpty(syntaxStr));
            if (string.IsNullOrEmpty(syntaxStr)) return null;

            if (item.Syntax == null)
            {
                item.Syntax = new SyntaxDetail { Content = new Dictionary<SyntaxLanguage, string>() };
            }

            if (item.Syntax.Content == null)
            {
                item.Syntax.Content = new Dictionary<SyntaxLanguage, string>();
            }

            item.Syntax.Content.Add(_language, syntaxStr);

            var parentSaved = parent;
            parent = item;

            foreach (var member in symbol.GetMembers())
            {
                var nsItem = member.Accept(this);
            }

            parent = parentSaved;
            return item;
        }

        public override YamlItemViewModel VisitMethod(IMethodSymbol symbol)
        {
            var item = AddYamlItem(symbol);
            if (item == null) return null;

            if (item.Syntax == null)
            {
                item.Syntax = new SyntaxDetail { Content = new Dictionary<SyntaxLanguage, string>() };
            }

            item.Syntax.Return = VisitorHelper.GetParameterDescription(symbol.ReturnType, item, true);

            if (item.Syntax.Parameters == null)
            {
                item.Syntax.Parameters = new List<YamlItemParameterViewModel>();
            }

            foreach (var i in symbol.Parameters)
            {
                var param = VisitorHelper.GetParameterDescription(i, item, false);
                Debug.Assert(param.Type != null);

                item.Syntax.Parameters.Add(param);
            }

            if (item.Type == MemberType.Constructor)
            {
                string parentName = parent.DisplayNames[_language];
                string name = item.DisplayQualifiedNames[_language];
                int index = name.IndexOf("#ctor");
                Debug.Assert(index > -1);
                if (index > -1)
                {
                    item.DisplayQualifiedNames[_language] = name.Replace("#ctor", parentName);

                    // Special handles for constructor's displayName as .ctor is internal
                    item.DisplayNames[_language] = item.DisplayQualifiedNames[_language].Substring(index);
                }
            }

            return item;
        }

        public override YamlItemViewModel VisitField(IFieldSymbol symbol)
        {
            return AddYamlItem(symbol);
        }

        public override YamlItemViewModel VisitEvent(IEventSymbol symbol)
        {
            return AddYamlItem(symbol);
        }

        public override YamlItemViewModel VisitProperty(IPropertySymbol symbol)
        {
            var item = AddYamlItem(symbol);
            if (item == null) return null;

            if (item.Syntax == null)
            {
                item.Syntax = new SyntaxDetail { Content = new Dictionary<SyntaxLanguage, string>() };
            }

            if (item.Syntax.Parameters == null)
            {
                item.Syntax.Parameters = new List<YamlItemParameterViewModel>();
            }

            var param = VisitorHelper.GetParameterDescription(symbol, item, false);
            Debug.Assert(param.Type != null);

            item.Syntax.Parameters.Add(param);

            return item;
        }

        private MemberType GetMemberTypeFromSymbol(ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Event:
                    return MemberType.Event;
                case SymbolKind.Field:
                    return MemberType.Field;
                case SymbolKind.Property:
                    return MemberType.Property;
                case SymbolKind.Method:
                    {
                        var methodSymbol = symbol as IMethodSymbol;
                        Debug.Assert(methodSymbol != null);
                        if (methodSymbol == null) return MemberType.Default;
                        switch (methodSymbol.MethodKind)
                        {
                            case MethodKind.AnonymousFunction:
                            case MethodKind.Conversion:
                            case MethodKind.DelegateInvoke:
                            case MethodKind.Destructor:
                            case MethodKind.EventAdd:
                            case MethodKind.EventRaise:
                            case MethodKind.EventRemove:
                            case MethodKind.ExplicitInterfaceImplementation:
                            case MethodKind.UserDefinedOperator:
                            case MethodKind.Ordinary:
                            case MethodKind.ReducedExtension:
                            case MethodKind.BuiltinOperator:
                            case MethodKind.DeclareMethod:
                                return MemberType.Method;
                            case MethodKind.Constructor:
                            case MethodKind.StaticConstructor:
                                return MemberType.Constructor;
                            // Property's get and set, ignore
                            case MethodKind.PropertyGet:
                            case MethodKind.PropertySet:
                            default:
                                return MemberType.Default;
                        }
                    }
                default:
                    return MemberType.Default;
            }
        }

        private YamlItemViewModel AddYamlItem(ISymbol symbol)
        {
            var item = DefaultVisit(symbol);
            if (item == null) return null;
            item.Type = GetMemberTypeFromSymbol(symbol);
            if (item.Type == MemberType.Default)
            {
                // If Default, then it is PropertyGet or PropertySet, ignore
                return null;
            }

            var syntaxRef = symbol.DeclaringSyntaxReferences.FirstOrDefault();
            Debug.Assert(syntaxRef != null || item.Type == MemberType.Constructor);
            if (syntaxRef == null)
            {
                return null;
            }

            var syntaxNode = syntaxRef.GetSyntax();
            Debug.Assert(syntaxNode != null);
            if (syntaxNode == null)
            {
                return null;
            }

            if (item.Syntax == null)
            {
                item.Syntax = new SyntaxDetail { Content = new Dictionary<SyntaxLanguage, string>() };
            }

            if (item.Syntax.Content == null)
            {
                item.Syntax.Content = new Dictionary<SyntaxLanguage, string>();
            }

            string syntaxStr = GetSyntaxContent(item.Type, syntaxNode);

            Debug.Assert(!string.IsNullOrEmpty(syntaxStr));
            if (string.IsNullOrEmpty(syntaxStr)) return null;

            item.Syntax.Content.Add(_language, syntaxStr);
            Debug.Assert(parent != null && currentNamespace != null);
            if (parent != null)
            {
                if (parent.Items == null)
                {
                    parent.Items = new List<YamlItemViewModel>();
                }

                parent.Items.Add(item);
            }

            return item;
        }
    }
}
