using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DocAsCode.Utility;

namespace DocAsCode.EntityModel
{
    public class YamlModelGeneratorVisitor : SymbolVisitor<YamlItemViewModel>
    {
        private YamlItemViewModel parent = new YamlItemViewModel();
        private YamlItemViewModel currentNamespace = new YamlItemViewModel();
        private YamlItemViewModel currentAssembly = new YamlItemViewModel();
        public YamlModelGeneratorVisitor(object context)
        {

        }

        private void FeedComments(YamlItemViewModel item)
        {
            if (string.IsNullOrEmpty(item.RawComment)) return;
            item.Summary = TripleSlashCommentParser.GetSummary(item.RawComment, true);
        }

        private string GetId(ISymbol symbol)
        {
            if (symbol == null)
            {
                return symbol.MetadataName;
            }

            string str = symbol.GetDocumentationCommentId();
            if (string.IsNullOrEmpty(str))
            {
                return symbol.MetadataName;
            }

            return str.ToString().Substring(2);
        }

        private static SourceDetail GetLinkDetail(ISymbol symbol)
        {
            if (symbol == null)
            {
                return null;
            }
            string id = symbol.GetDocumentationCommentId();
            if (string.IsNullOrEmpty(id))
            {
                var typeSymbol = symbol as ITypeSymbol;
                if (typeSymbol != null)
                {
                    id = typeSymbol.BaseType.GetDocumentationCommentId();
                }
            }

            string displayName = string.Empty;
            if (!string.IsNullOrEmpty(id) && id.Length > 2)
            {
                displayName = id.Substring(2);
                id = id.Substring(2);
            }

            var syntaxRef = symbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (symbol.IsExtern || syntaxRef == null)
            {
                // TODO: get external URL
                return new SourceDetail { IsExternalPath = true, Name = id, DisplayName = displayName, Href = null };
            }

            var syntaxNode = syntaxRef.GetSyntax();
            Debug.Assert(syntaxNode != null);
            if (syntaxNode != null)
            {
                return new SourceDetail
                {
                    Name = id,
                    DisplayName = symbol.Name,
                    IsExternalPath = false,
                    Href = syntaxNode.SyntaxTree.FilePath,
                };
            }

            return null;
        }
        private YamlItemParameterViewModel GetParameterDescription(ISymbol symbol, YamlItemViewModel item, bool isReturn)
        {
            SourceDetail id = null;
            string raw = item.RawComment;
            // TODO: GetDocumentationCommentXml for parameters seems not accurate
            string name = symbol.Name;
            var paraSymbol = symbol as IParameterSymbol;
            if (paraSymbol != null)
            {
                // TODO: why BaseType?
                id = GetLinkDetail(paraSymbol.Type);
            }

            // when would it be type symbol?
            var typeSymbol = symbol as ITypeSymbol;
            if (typeSymbol != null)
            {
                Debug.Assert(typeSymbol != null, "Should be TypeSymbol");

                // TODO: check what name is
                // name = DescriptionConstants.ReturnName;
                id = GetLinkDetail(paraSymbol);
            }

            var propertySymbol = symbol as IPropertySymbol;
            if (propertySymbol != null)
            {
                // TODO: check what name is
                // name = DescriptionConstants.ReturnName;
                id = GetLinkDetail(propertySymbol.Type);
            }

            string comment = isReturn ? TripleSlashCommentParser.GetReturns(raw, true) :
                TripleSlashCommentParser.GetParam(raw, name, true);

            return new YamlItemParameterViewModel() { Name = name, Type = id, Description = comment };
        }

        private static bool CanVisit(ISymbol symbol)
        {
            if (symbol.DeclaredAccessibility == Accessibility.NotApplicable)
            {
                return true;
            }

            if (symbol.DeclaredAccessibility != Accessibility.Public)
            {
                return false;
            }

            return true;
        }

        private static SourceDetail GetSourceDetail(ISymbol symbol)
        {
            if (symbol == null)
            {
                return null;
            }

            var syntaxRef = symbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (symbol.IsExtern || syntaxRef == null)
            {
                return new SourceDetail { IsExternalPath = true, Path = symbol.ContainingAssembly != null ? symbol.ContainingAssembly.Name : symbol.Name };
            }

            var syntaxNode = syntaxRef.GetSyntax();
            Debug.Assert(syntaxNode != null);
            if (syntaxNode != null)
            {
                var source = new SourceDetail
                {
                    StartLine = syntaxNode.SyntaxTree.GetLineSpan(syntaxNode.Span).StartLinePosition.Line,
                    Path = syntaxNode.SyntaxTree.FilePath,
                };

                source.Remote = GitUtility.GetGitDetail(source.Path);
                return source;
            }

            return null;
        }

        public override YamlItemViewModel DefaultVisit(ISymbol symbol)
        {
            if (!CanVisit(symbol)) return null;
            var item = new YamlItemViewModel
            {
                Name = GetId(symbol),
                DisplayNames = new Dictionary<SyntaxLanguage, string>() { { SyntaxLanguage.CSharp, symbol.MetadataName } },
                RawComment = symbol.GetDocumentationCommentXml(),
            };

            item.DisplayQualifiedNames = new Dictionary<SyntaxLanguage, string>() { { SyntaxLanguage.CSharp, item.Name } };
            
            item.Source = GetSourceDetail(symbol);
            FeedComments(item);
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
                    SourceDetail link = GetLinkDetail(type);

                    item.Inheritance.Add(link);
                    type = type.BaseType;
                }

                item.Inheritance.Reverse();
            }
            string syntaxStr = string.Empty;
            int openBracketIndex = -1;
            Debug.Assert(parent != null && currentNamespace != null);
            if (currentNamespace != null)
            {
                if (currentNamespace.Items == null)
                {
                    currentNamespace.Items = new List<YamlItemViewModel>();
                }

                currentNamespace.Items.Add(item);
            }

            switch (symbol.TypeKind)
            {
                case TypeKind.Class:
                    {
                        item.Type = MemberType.Class;
                        var syntax = syntaxNode as ClassDeclarationSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        syntaxStr
                            = syntax
                            .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                            .WithBaseList(null)
                            .WithMembers(new SyntaxList<MemberDeclarationSyntax>())
                            .NormalizeWhitespace()
                            .ToString();
                        openBracketIndex = syntaxStr.IndexOf(syntax.OpenBraceToken.ValueText);
                        break;
                    };
                case TypeKind.Enum:
                    {
                        item.Type = MemberType.Enum;
                        var syntax = syntaxNode as EnumDeclarationSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        syntaxStr
                            = syntax
                                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                                .WithBaseList(null)
                                .WithMembers(new SeparatedSyntaxList<EnumMemberDeclarationSyntax>())
                                .NormalizeWhitespace()
                                .ToString();
                        openBracketIndex = syntaxStr.IndexOf(syntax.OpenBraceToken.ValueText);
                        break;
                    };
                case TypeKind.Interface:
                    {
                        item.Type = MemberType.Interface;
                        var syntax = syntaxNode as InterfaceDeclarationSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        syntaxStr =
                            syntax
                                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                                .WithBaseList(null)
                                .WithMembers(new SyntaxList<MemberDeclarationSyntax>())
                                .NormalizeWhitespace()
                                .ToString();

                        openBracketIndex = syntaxStr.IndexOf(syntax.OpenBraceToken.ValueText);
                        break;
                    };
                case TypeKind.Struct:
                    {
                        item.Type = MemberType.Struct;
                        var syntax = syntaxNode as StructDeclarationSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        syntaxStr = syntax
                                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                                .WithBaseList(null)
                                .WithMembers(new SyntaxList<MemberDeclarationSyntax>())
                                .NormalizeWhitespace()
                                .ToString();
                        openBracketIndex = syntaxStr.IndexOf(syntax.OpenBraceToken.ValueText);
                        break;
                    };
                case TypeKind.Delegate:
                    {
                        item.Type = MemberType.Delegate;
                        var syntax = syntaxNode as DelegateDeclarationSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        syntaxStr = syntax
                                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                                .NormalizeWhitespace()
                                .ToString();
                        break;
                    };
            }
            if (item.Syntax == null)
            {
                item.Syntax = new SyntaxDetail { Content = new Dictionary<SyntaxLanguage, string>() };
            }

            if (item.Syntax.Content == null)
            {
                item.Syntax.Content = new Dictionary<SyntaxLanguage, string>();
            }

            if (openBracketIndex > -1)
            {
                item.Syntax.Content.Add(SyntaxLanguage.CSharp, syntaxStr.Substring(0, openBracketIndex).Trim());
            }
            else
            {
                item.Syntax.Content.Add(SyntaxLanguage.CSharp, syntaxStr.Trim());
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
                    item.Syntax.Parameters.Add(GetParameterDescription(p, item, false));
                }

                item.Syntax.Return = GetParameterDescription(symbol.ReturnType, item, true);

                var syntaxStr = syntax.WithBody(null)
                        .NormalizeWhitespace()
                        .ToString()
                        .Trim();
                
                item.Syntax.Content.Add(SyntaxLanguage.CSharp, syntaxStr);
                return item;
            }

            var constructorSyntax = syntaxNode as ConstructorDeclarationSyntax;

            if (constructorSyntax != null)
            {
                item.Type = MemberType.Constructor;
                string parentName = parent.DisplayNames[SyntaxLanguage.CSharp];
                string name = item.DisplayQualifiedNames[SyntaxLanguage.CSharp];
                int index = name.IndexOf("#ctor");
                Debug.Assert(index > -1);
                if (index > -1)
                {
                    item.DisplayQualifiedNames[SyntaxLanguage.CSharp] = name.Replace("#ctor", parentName);

                    // Special handles for constructor's displayName as .ctor is internal
                    item.DisplayNames[SyntaxLanguage.CSharp] = item.DisplayQualifiedNames[SyntaxLanguage.CSharp].Substring(index);
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
                    item.Syntax.Parameters.Add(GetParameterDescription(p, item, false));
                }

                string syntaxStr = constructorSyntax.WithBody(null)
                        .NormalizeWhitespace()
                        .ToString()
                        .Trim();

                item.Syntax.Content.Add(SyntaxLanguage.CSharp, syntaxStr);
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
                    item.Syntax.Content.Add(SyntaxLanguage.CSharp, syntaxStr);
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
                    item.Syntax.Content.Add(SyntaxLanguage.CSharp, syntaxStr);
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
                    item.Syntax.Content.Add(SyntaxLanguage.CSharp, syntaxStr);
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

                item.Syntax.Parameters.Add(GetParameterDescription(symbol, item, false));

                var syntaxStr = varSyntax
                        .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                        .WithAccessorList(null)
                        .NormalizeWhitespace()
                        .ToString()
                        .Trim();
                item.Syntax.Content.Add(SyntaxLanguage.CSharp, syntaxStr);
                return item;
            }

            return null;
        }
    }
}
