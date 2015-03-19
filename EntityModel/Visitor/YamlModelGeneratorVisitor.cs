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

        public abstract string GetSyntaxContent(TypeKind typeKind, SyntaxNode syntaxNode);

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
            string syntaxStr = GetSyntaxContent(symbol.TypeKind, syntaxNode);

            if (item.Syntax == null)
            {
                item.Syntax = new SyntaxDetail { Content = new Dictionary<SyntaxLanguage, string>() };
            }

            if (item.Syntax.Content == null)
            {
                item.Syntax.Content = new Dictionary<SyntaxLanguage, string> { { _language, syntaxStr } };
            }

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
            var item = DefaultVisit(symbol);
            if (item == null) return null;

            // e.g. default .ctor
            var syntaxRef = symbol.DeclaringSyntaxReferences.FirstOrDefault();
            // Debug.Assert(syntaxRef != null); Could be default constructor
            if (syntaxRef == null)
            {
                return null;
            }

            Debug.Assert(parent != null && currentNamespace != null);
            if (parent != null)
            {
                if (parent.Items == null)
                {
                    parent.Items = new List<YamlItemViewModel>();
                }

                parent.Items.Add(item);
            }

            var syntaxNode = syntaxRef.GetSyntax();
            Debug.Assert(syntaxNode != null);
            if (syntaxNode == null)
            {
                return null;
            }
            var syntax = syntaxNode as MethodDeclarationSyntax;

            if (syntax != null)
            {
                item.Type = MemberType.Method;

                if (item.Syntax == null)
                {
                    item.Syntax = new SyntaxDetail { Content = new Dictionary<SyntaxLanguage, string>() };
                }

                if (item.Syntax.Content == null)
                {
                    item.Syntax.Content = new Dictionary<SyntaxLanguage, string>();
                }

                if (item.Syntax.Parameters == null)
                {
                    item.Syntax.Parameters = new List<YamlItemParameterViewModel>();
                }

                foreach (var p in symbol.Parameters)
                {
                    item.Syntax.Parameters.Add(VisitorHelper.GetParameterDescription(p, item, false));
                }

                item.Syntax.Return = VisitorHelper.GetParameterDescription(symbol.ReturnType, item, true);

                var syntaxStr = syntax.WithBody(null)
                        .NormalizeWhitespace()
                        .ToString()
                        .Trim();
                
                item.Syntax.Content.Add(_language, syntaxStr);
                return item;
            }

            var constructorSyntax = syntaxNode as ConstructorDeclarationSyntax;

            if (constructorSyntax != null)
            {
                item.Type = MemberType.Constructor;
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

                if (item.Syntax == null)
                {
                    item.Syntax = new SyntaxDetail { Content = new Dictionary<SyntaxLanguage, string>() };
                }

                if (item.Syntax.Content == null)
                {
                    item.Syntax.Content = new Dictionary<SyntaxLanguage, string>();
                }

                if (item.Syntax.Parameters == null)
                {
                    item.Syntax.Parameters = new List<YamlItemParameterViewModel>();
                }

                foreach (var p in symbol.Parameters)
                {
                    item.Syntax.Parameters.Add(VisitorHelper.GetParameterDescription(p, item, false));
                }

                string syntaxStr = constructorSyntax.WithBody(null)
                        .NormalizeWhitespace()
                        .ToString()
                        .Trim();

                item.Syntax.Content.Add(_language, syntaxStr);
                return item;
            }

            return null;
        }
        public override YamlItemViewModel VisitField(IFieldSymbol symbol)
        {
            var item = DefaultVisit(symbol);
            if (item == null) return null;
            item.Type = MemberType.Field;

            var syntaxRef = symbol.DeclaringSyntaxReferences.FirstOrDefault();
            Debug.Assert(syntaxRef != null);
            if (syntaxRef == null)
            {
                return null;
            }

            Debug.Assert(parent != null && currentNamespace != null);
            if (parent != null)
            {
                if (parent.Items == null)
                {
                    parent.Items = new List<YamlItemViewModel>();
                }

                parent.Items.Add(item);
            }
            var syntaxNode = syntaxRef.GetSyntax();
            Debug.Assert(syntaxNode != null);
            if (syntaxNode == null)
            {
                return null;
            }
            if (syntaxNode is VariableDeclarationSyntax || syntaxNode is MemberDeclarationSyntax)
            {
                var varSyntax = syntaxNode as VariableDeclaratorSyntax;
                if (varSyntax != null)
                {
                    if (item.Syntax == null)
                    {
                        item.Syntax = new SyntaxDetail { Content = new Dictionary<SyntaxLanguage, string>() };
                    }

                    if (item.Syntax.Content == null)
                    {
                        item.Syntax.Content = new Dictionary<SyntaxLanguage, string>();
                    }

                    var syntaxStr = varSyntax
                            .WithInitializer(null)
                            .NormalizeWhitespace()
                            .ToString()
                            .Trim();
                    item.Syntax.Content.Add(_language, syntaxStr);
                    return item;
                }

                // For Enum's member
                var memberSyntax = syntaxNode as MemberDeclarationSyntax;

                if (memberSyntax != null)
                {
                    if (item.Syntax == null)
                    {
                        item.Syntax = new SyntaxDetail { Content = new Dictionary<SyntaxLanguage, string>() };
                    }

                    if (item.Syntax.Content == null)
                    {
                        item.Syntax.Content = new Dictionary<SyntaxLanguage, string>();
                    }

                    var syntaxStr = memberSyntax
                            .NormalizeWhitespace()
                            .ToString()
                            .Trim();
                    item.Syntax.Content.Add(_language, syntaxStr);
                    return item;
                }
            }

            return null;
        }

        public override YamlItemViewModel VisitEvent(IEventSymbol symbol)
        {
            var item = DefaultVisit(symbol);
            if (item == null) return null;
            item.Type = MemberType.Event;

            var syntaxRef = symbol.DeclaringSyntaxReferences.FirstOrDefault();
            Debug.Assert(syntaxRef != null);
            if (syntaxRef == null)
            {
                return null;
            }
            Debug.Assert(parent != null && currentNamespace != null);
            if (parent != null)
            {
                if (parent.Items == null)
                {
                    parent.Items = new List<YamlItemViewModel>();
                }

                parent.Items.Add(item);
            }

            var syntaxNode = syntaxRef.GetSyntax();
            Debug.Assert(syntaxNode != null);
            if (syntaxNode == null)
            {
                return null;
            }
            if (syntaxNode is VariableDeclaratorSyntax)
            {
                var varSyntax = syntaxNode as VariableDeclaratorSyntax;
                if (varSyntax != null)
                {
                    if (item.Syntax == null)
                    {
                        item.Syntax = new SyntaxDetail { Content = new Dictionary<SyntaxLanguage, string>() };
                    }

                    if (item.Syntax.Content == null)
                    {
                        item.Syntax.Content = new Dictionary<SyntaxLanguage, string>();
                    }

                    var syntaxStr = varSyntax
                                .NormalizeWhitespace()
                                .ToString()
                                .Trim();
                    item.Syntax.Content.Add(_language, syntaxStr);
                    return item;
                }
            }

            return null;

        }

        public override YamlItemViewModel VisitProperty(IPropertySymbol symbol)
        {
            var item = DefaultVisit(symbol);
            if (item == null) return null;
            item.Type = MemberType.Property;

            var syntaxRef = symbol.DeclaringSyntaxReferences.FirstOrDefault();
            Debug.Assert(syntaxRef != null);
            if (syntaxRef == null)
            {
                return null;
            }
            Debug.Assert(parent != null && currentNamespace != null);
            if (parent != null)
            {
                if (parent.Items == null)
                {
                    parent.Items = new List<YamlItemViewModel>();
                }

                parent.Items.Add(item);
            }

            var syntaxNode = syntaxRef.GetSyntax();
            Debug.Assert(syntaxNode != null);
            if (syntaxNode == null)
            {
                return null;
            }
            var varSyntax = syntaxNode as PropertyDeclarationSyntax;
            if (varSyntax != null)
            {
                if (item.Syntax == null)
                {
                    item.Syntax = new SyntaxDetail { Content = new Dictionary<SyntaxLanguage, string>() };
                }

                if (item.Syntax.Parameters == null)
                {
                    item.Syntax.Parameters = new List<YamlItemParameterViewModel>();
                }
                if (item.Syntax.Content == null)
                {
                    item.Syntax.Content = new Dictionary<SyntaxLanguage, string>();
                }

                item.Syntax.Parameters.Add(VisitorHelper.GetParameterDescription(symbol, item, false));

                var syntaxStr = varSyntax
                        .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                        .WithAccessorList(null)
                        .NormalizeWhitespace()
                        .ToString()
                        .Trim();
                item.Syntax.Content.Add(_language, syntaxStr);
                return item;
            }

            return null;
        }
    }
}
