namespace DocAsCode.EntityModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.CodeAnalysis;

    public abstract class YamlModelGeneratorVisitor : SymbolVisitor<YamlItemViewModel>
    {
        private YamlItemViewModel parent = new YamlItemViewModel();
        private YamlItemViewModel currentNamespace = new YamlItemViewModel();
        private YamlItemViewModel currentAssembly = new YamlItemViewModel();
        private readonly SyntaxLanguage language;
        public YamlModelGeneratorVisitor(object context, SyntaxLanguage language)
        {
            this.language = language;
        }

        public abstract string GetSyntaxContent(MemberType typeKind, SyntaxNode syntaxNode);

        public override YamlItemViewModel DefaultVisit(ISymbol symbol)
        {
            if (!VisitorHelper.CanVisit(symbol)) return null;
            var item = new YamlItemViewModel
            {
                Name = VisitorHelper.GetId(symbol),
                RawComment = symbol.GetDocumentationCommentXml(),
                Language = this.language,
            };

            item.DisplayNames = new Dictionary<SyntaxLanguage, string>() { { language, symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat) } };
            item.DisplayQualifiedNames = new Dictionary<SyntaxLanguage, string>() { { language, symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) } };

            item.Source = VisitorHelper.GetSourceDetail(symbol);
            VisitorHelper.FeedComments(item);

            return item;
        }

        public override YamlItemViewModel VisitAssembly(IAssemblySymbol symbol)
        {
            var item = this.DefaultVisit(symbol);
            if (item == null) return null;
            item.Type = MemberType.Assembly;

            this.parent = item;
            this.currentAssembly = item;
            foreach (var ns in symbol.GlobalNamespace.GetNamespaceMembers())
            {
                ns.Accept(this);
            }

            return item;
        }

        public override YamlItemViewModel VisitNamespace(INamespaceSymbol symbol)
        {
            var item = this.DefaultVisit(symbol);
            if (item == null) return null;
            item.Type = MemberType.Namespace;

            Debug.Assert(this.parent != null && this.currentAssembly != null);
            if (this.currentAssembly != null)
            {
                if (this.currentAssembly.Items == null)
                {
                    this.currentAssembly.Items = new List<YamlItemViewModel>();
                }

                this.currentAssembly.Items.Add(item);
            }

            var members = symbol.GetMembers().ToList();
            var currentNamespaceSaved = this.currentNamespace;
            this.currentNamespace = item;
            var parentSaved = this.parent;
            this.parent = item;
            foreach (var member in members)
            {
                var nsItem = member.Accept(this);
            }
            this.parent = parentSaved;
            this.currentNamespace = currentNamespaceSaved;
            return item;
        }

        public override YamlItemViewModel VisitNamedType(INamedTypeSymbol symbol)
        {
            var item = this.DefaultVisit(symbol);
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
            Debug.Assert(this.parent != null && this.currentNamespace != null);
            if (this.currentNamespace != null)
            {
                if (this.currentNamespace.Items == null)
                {
                    this.currentNamespace.Items = new List<YamlItemViewModel>();
                }

                this.currentNamespace.Items.Add(item);
            }

            item.Type = VisitorHelper.GetMemberTypeFromTypeKind(symbol.TypeKind);
            string syntaxStr = this.GetSyntaxContent(item.Type, syntaxNode);
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

            item.Syntax.Content.Add(this.language, syntaxStr);

            var parentSaved = this.parent;
            this.parent = item;

            foreach (var member in symbol.GetMembers())
            {
                var nsItem = member.Accept(this);
            }

            this.parent = parentSaved;
            return item;
        }

        public override YamlItemViewModel VisitMethod(IMethodSymbol symbol)
        {
            var item = this.AddYamlItem(symbol);
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

            return item;
        }

        public override YamlItemViewModel VisitField(IFieldSymbol symbol)
        {
            return this.AddYamlItem(symbol);
        }

        public override YamlItemViewModel VisitEvent(IEventSymbol symbol)
        {
            return this.AddYamlItem(symbol);
        }

        public override YamlItemViewModel VisitProperty(IPropertySymbol symbol)
        {
            var item = this.AddYamlItem(symbol);
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
            var item = this.DefaultVisit(symbol);
            if (item == null) return null;
            item.Type = this.GetMemberTypeFromSymbol(symbol);
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

            string syntaxStr = this.GetSyntaxContent(item.Type, syntaxNode);

            Debug.Assert(!string.IsNullOrEmpty(syntaxStr));
            if (string.IsNullOrEmpty(syntaxStr)) return null;

            item.Syntax.Content.Add(this.language, syntaxStr);
            Debug.Assert(this.parent != null && this.currentNamespace != null);
            if (this.parent != null)
            {
                if (this.parent.Items == null)
                {
                    this.parent.Items = new List<YamlItemViewModel>();
                }

                this.parent.Items.Add(item);
            }

            return item;
        }
    }
}
