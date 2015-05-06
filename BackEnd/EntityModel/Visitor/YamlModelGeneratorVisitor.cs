namespace DocAsCode.EntityModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.CodeAnalysis;

    public abstract class YamlModelGeneratorVisitor : SymbolVisitor<MetadataItem>
    {
        private MetadataItem parent = new MetadataItem();
        private MetadataItem currentNamespace = new MetadataItem();
        private MetadataItem currentAssembly = new MetadataItem();
        private readonly SyntaxLanguage language;

        public YamlModelGeneratorVisitor(object context, SyntaxLanguage language)
        {
            this.language = language;
        }

        public abstract SymbolDisplayFormat ShortDisplayFormat { get; }

        public abstract SymbolDisplayFormat DisplayFormat { get; }

        public abstract string GetSyntaxContent(MemberType typeKind, ISymbol symbol, SyntaxNode syntaxNode);

        public override MetadataItem DefaultVisit(ISymbol symbol)
        {
            if (!VisitorHelper.CanVisit(symbol)) return null;
            var item = new MetadataItem
            {
                Name = VisitorHelper.GetId(symbol),
                RawComment = symbol.GetDocumentationCommentXml(),
                Language = this.language,
            };

            item.DisplayNames = new Dictionary<SyntaxLanguage, string>() { { SyntaxLanguage.CSharp, symbol.ToDisplayString(ShortDisplayFormat) } };
            item.DisplayQualifiedNames = new Dictionary<SyntaxLanguage, string>() { { SyntaxLanguage.CSharp, symbol.ToDisplayString(DisplayFormat) } };
            item.Source = VisitorHelper.GetSourceDetail(symbol);
            VisitorHelper.FeedComments(item);

            return item;
        }

        public override MetadataItem VisitAssembly(IAssemblySymbol symbol)
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

        public override MetadataItem VisitNamespace(INamespaceSymbol symbol)
        {
            var item = this.DefaultVisit(symbol);
            if (item == null) return null;
            item.Type = MemberType.Namespace;

            Debug.Assert(this.parent != null && this.currentAssembly != null);
            if (this.currentAssembly != null)
            {
                if (this.currentAssembly.Items == null)
                {
                    this.currentAssembly.Items = new List<MetadataItem>();
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

        public override MetadataItem VisitNamedType(INamedTypeSymbol symbol)
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
                    SourceDetail link = VisitorHelper.GetLinkDetail(type, ShortDisplayFormat);

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
                    this.currentNamespace.Items = new List<MetadataItem>();
                }

                this.currentNamespace.Items.Add(item);
            }

            item.Type = VisitorHelper.GetMemberTypeFromTypeKind(symbol.TypeKind);
            string syntaxStr = this.GetSyntaxContent(item.Type, symbol, syntaxNode);
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

        public override MetadataItem VisitMethod(IMethodSymbol symbol)
        {
            var item = this.AddYamlItem(symbol);
            if (item == null) return null;

            if (item.Syntax == null)
            {
                item.Syntax = new SyntaxDetail { Content = new Dictionary<SyntaxLanguage, string>() };
            }

            if (!symbol.ReturnsVoid)
            {
                item.Syntax.Return = VisitorHelper.GetParameterDescription(symbol.ReturnType, item, true, ShortDisplayFormat);
            }

            if (item.Syntax.Parameters == null)
            {
                item.Syntax.Parameters = new List<ApiParameter>();
            }

            foreach (var i in symbol.Parameters)
            {
                var param = VisitorHelper.GetParameterDescription(i, item, false, ShortDisplayFormat);
                Debug.Assert(param.Type != null);

                item.Syntax.Parameters.Add(param);
            }

            return item;
        }

        public override MetadataItem VisitField(IFieldSymbol symbol)
        {
            return this.AddYamlItem(symbol);
        }

        public override MetadataItem VisitEvent(IEventSymbol symbol)
        {
            return this.AddYamlItem(symbol);
        }

        public override MetadataItem VisitProperty(IPropertySymbol symbol)
        {
            var item = this.AddYamlItem(symbol);
            if (item == null) return null;

            if (item.Syntax == null)
            {
                item.Syntax = new SyntaxDetail { Content = new Dictionary<SyntaxLanguage, string>() };
            }

            if (item.Syntax.Parameters == null)
            {
                item.Syntax.Parameters = new List<ApiParameter>();
            }

            var param = VisitorHelper.GetParameterDescription(symbol, item, false, ShortDisplayFormat);
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
                            case MethodKind.DelegateInvoke:
                            case MethodKind.Destructor:
                            case MethodKind.ExplicitInterfaceImplementation:
                            case MethodKind.Ordinary:
                            case MethodKind.ReducedExtension:
                            case MethodKind.DeclareMethod:
                                return MemberType.Method;
                            case MethodKind.BuiltinOperator:
                            case MethodKind.UserDefinedOperator:
                            case MethodKind.Conversion:
                                return MemberType.Operator;
                            case MethodKind.Constructor:
                            case MethodKind.StaticConstructor:
                                return MemberType.Constructor;
                            // ignore: Property's get/set, and event's add/remove/raise
                            case MethodKind.PropertyGet:
                            case MethodKind.PropertySet:
                            case MethodKind.EventAdd:
                            case MethodKind.EventRemove:
                            case MethodKind.EventRaise:
                            default:
                                return MemberType.Default;
                        }
                    }
                default:
                    return MemberType.Default;
            }
        }

        private MetadataItem AddYamlItem(ISymbol symbol)
        {
            var item = this.DefaultVisit(symbol);
            if (item == null) return null;
            item.Type = this.GetMemberTypeFromSymbol(symbol);
            if (item.Type == MemberType.Default)
            {
                // If Default, then it is Property get/set or Event add/remove/raise, ignore
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

            string syntaxStr = this.GetSyntaxContent(item.Type, symbol, syntaxNode);

            Debug.Assert(!string.IsNullOrEmpty(syntaxStr));
            if (string.IsNullOrEmpty(syntaxStr)) return null;

            item.Syntax.Content.Add(this.language, syntaxStr);
            Debug.Assert(this.parent != null && this.currentNamespace != null);
            if (this.parent != null)
            {
                if (this.parent.Items == null)
                {
                    this.parent.Items = new List<MetadataItem>();
                }

                this.parent.Items.Add(item);
            }

            return item;
        }
    }
}
