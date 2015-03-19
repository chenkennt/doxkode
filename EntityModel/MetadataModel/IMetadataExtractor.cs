using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using DocAsCode.EntityModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EntityModel
{
    public static class MetadataExtractorManager
    {
        /// <summary>
        /// Order matters
        /// </summary>
        private static IList<IMetadataExtractor> _supportedExtractors = new List<IMetadataExtractor>
        {
            new NamespaceSymbolExtractor(),
            new NamespaceMemberSymbolExtractor(),
            new NamespaceMembersMemberSymbolExtractor(),
            new DefaultExtractor()
        };

        public static bool CanExtract(ISymbol symbol)
        {
            foreach (var converter in _supportedExtractors)
            {
                if (converter.CanExtract(symbol))
                {
                    return true;
                }
            }

            return false;
        }

        public static Task<IMetadata> ExtractAsync(ISymbol symbol, IMetadataExtractContext context)
        {
            foreach (var converter in _supportedExtractors)
            {
                if (converter.CanExtract(symbol))
                {
                    return converter.ExtractAsync(symbol, context);
                }
            }

            return Task.FromResult<IMetadata>(null);
        }
    }

    public class MetadataExtractContext : IMetadataExtractContext
    {
        public string ProjectName { get; set; }

        public IMetadata OwnerNamespace { get; set; }
    }

    public interface IMetadataExtractContext
    {
        string ProjectName { get; set; }

        IMetadata OwnerNamespace { get; set; }
    }

    public interface IMetadataExtractor
    {
        bool CanExtract(ISymbol symbol);

        Task<IMetadata> ExtractAsync(ISymbol symbol, IMetadataExtractContext context);
    }

    public class DefaultExtractor : IMetadataExtractor
    {
        public virtual bool CanExtract(ISymbol symbol)
        {
            if (symbol.DeclaredAccessibility.HasFlag(Accessibility.Public))
            {
                return true;
            }

            if (symbol.DeclaredAccessibility.HasFlag(Accessibility.Protected))
            {
                return false;
            }

            return false;
        }

        public virtual Task<IMetadata> ExtractAsync(ISymbol symbol, IMetadataExtractContext context)
        {
            return null;
        }

        /// <summary>
        /// The basic settings for all the metadata
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        protected static async Task<SyntaxNode> SetCommonMetadataAndGetSyntaxNodeAsync(IMetadata metadata, ISymbol symbol, IMetadataExtractContext context)
        {
            // Consider CommentId as the glable unique Identity in one documentation project
            metadata.Identity = new Identity(symbol.GetDocumentationCommentId());
            metadata.OwnerNamespace = context.OwnerNamespace;

            // TODO: how about version?
            metadata.AssemblyName = symbol.ContainingAssembly.ToDisplayString();
            
            var tripleSlashComment = new TripleSlashComment() { Raw = symbol.GetDocumentationCommentXml() };

            // Set CSharp syntax
            var syntaxDescription = new SyntaxDescription
            {
                Comments = new List<CommentBase> { tripleSlashComment }
            };

            metadata.SyntaxDescriptionGroup = new SyntaxDescriptionGroup() { { SyntaxLanguage.CSharp, syntaxDescription } };
            
            if (!symbol.DeclaringSyntaxReferences.Any())
            {
                return null;
            }

            var syntaxNode = await symbol.DeclaringSyntaxReferences.First().GetSyntaxAsync();

            if (syntaxNode == null)
            {
                return null;
            }

            tripleSlashComment.StartLine = syntaxNode.SyntaxTree.GetLineSpan(syntaxNode.Span).StartLinePosition.Line;
            metadata.FilePath = syntaxNode.SyntaxTree.FilePath;
            return syntaxNode;
        }
    }

    public class NamespaceSymbolExtractor : DefaultExtractor
    {
        public override bool CanExtract(ISymbol symbol)
        {
            var namespaceSymbol = symbol as INamespaceSymbol;
            if (namespaceSymbol == null)
            {
                return false;
            }

            return base.CanExtract(symbol);
        }

        public override async Task<IMetadata> ExtractAsync(ISymbol symbol, IMetadataExtractContext context)
        {
            NamespaceMetadata namespaceMetadata = new NamespaceMetadata();
            var syntax = await SetCommonMetadataAndGetSyntaxNodeAsync(namespaceMetadata, symbol, context) as NamespaceDeclarationSyntax;
            namespaceMetadata.MemberType = MemberType.Namespace;
            return syntax == null ? null : namespaceMetadata;
        }
    }

    public class NamespaceMemberSymbolExtractor : DefaultExtractor
    {
        public override bool CanExtract(ISymbol symbol)
        {
            var nameTypedSymbol = symbol as INamedTypeSymbol;
            if (nameTypedSymbol == null)
            {
                return false;
            }

            if ((nameTypedSymbol.TypeKind != TypeKind.Class) && (nameTypedSymbol.TypeKind != TypeKind.Enum)
                && (nameTypedSymbol.TypeKind != TypeKind.Interface) && (nameTypedSymbol.TypeKind != TypeKind.Struct)
                && (nameTypedSymbol.TypeKind != TypeKind.Delegate))
            {
                return false;
            }

            return base.CanExtract(symbol);
        }

        public override async Task<IMetadata> ExtractAsync(ISymbol symbol, IMetadataExtractContext context)
        {
            NamespaceMemberMetadata namespaceMemberMetadata = new NamespaceMemberMetadata();

            var syntaxNode = await SetCommonMetadataAndGetSyntaxNodeAsync(namespaceMemberMetadata, symbol, context);
            namespaceMemberMetadata.ProjectName = context.ProjectName;
            if (syntaxNode == null)
            {
                return null;
            }

            var nameTypedSymbol = symbol as INamedTypeSymbol;
            Debug.Assert(nameTypedSymbol != null);

            namespaceMemberMetadata.FilePath = syntaxNode.SyntaxTree.FilePath;

            var type = nameTypedSymbol.BaseType;
            if (type != null)
            {
                namespaceMemberMetadata.InheritanceHierarchy = new List<Identity>();

                while (type != null)
                {
                    namespaceMemberMetadata.InheritanceHierarchy.Add(new Identity(type));
                    type = type.BaseType;
                }
            }
            string syntaxStr = string.Empty;
            int openBracketIndex = -1;
            switch (nameTypedSymbol.TypeKind)
            {
                case TypeKind.Class:
                    {
                        var syntax = syntaxNode as ClassDeclarationSyntax;
                        Debug.Assert(syntax != null);

                        namespaceMemberMetadata.MemberType = MemberType.Class;
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
                        var syntax = syntaxNode as EnumDeclarationSyntax;
                        Debug.Assert(syntax != null);

                        namespaceMemberMetadata.MemberType = MemberType.Enum;
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
                        var syntax = syntaxNode as InterfaceDeclarationSyntax;
                        Debug.Assert(syntax != null);

                        namespaceMemberMetadata.MemberType = MemberType.Interface;

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
                        var syntax = syntaxNode as StructDeclarationSyntax;
                        Debug.Assert(syntax != null);

                        namespaceMemberMetadata.MemberType = MemberType.Struct;
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
                        var syntax = syntaxNode as DelegateDeclarationSyntax;
                        Debug.Assert(syntax != null);

                        namespaceMemberMetadata.MemberType = MemberType.Delegate;
                        syntaxStr = syntax
                                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                                .NormalizeWhitespace()
                                .ToString();
                        break;
                    };

            }

            if (openBracketIndex > -1)
            {
                namespaceMemberMetadata.SyntaxDescriptionGroup[SyntaxLanguage.CSharp].Syntax = syntaxStr.Substring(0, openBracketIndex).Trim();
            }
            else
            {
                namespaceMemberMetadata.SyntaxDescriptionGroup[SyntaxLanguage.CSharp].Syntax = syntaxStr.Trim();
            }
            return namespaceMemberMetadata;
        }
    }

    public class NamespaceMembersMemberSymbolExtractor : DefaultExtractor
    {
        public override bool CanExtract(ISymbol symbol)
        {
            // Currently support Methods, property, field and event.
            if ((symbol.Kind != SymbolKind.Method) && (symbol.Kind != SymbolKind.Property) 
                && (symbol.Kind != SymbolKind.Field) && (symbol.Kind != SymbolKind.Event))
            {
                return false;
            }

            return base.CanExtract(symbol);
        }

        public override async Task<IMetadata> ExtractAsync(ISymbol symbol, IMetadataExtractContext context)
        {
            NamespaceMembersMemberMetadata membersMemberMetadata = new NamespaceMembersMemberMetadata();

            var syntaxNode = await SetCommonMetadataAndGetSyntaxNodeAsync(membersMemberMetadata, symbol, context);

            if (syntaxNode == null)
            {
                return null;
            }
            
            switch (symbol.Kind)
            {
                case SymbolKind.Method:
                    {
                        var syntax = syntaxNode as MethodDeclarationSyntax;

                        if (syntax != null)
                        {
                            var baseSyntax = membersMemberMetadata.SyntaxDescriptionGroup[SyntaxLanguage.CSharp];
                            membersMemberMetadata.MemberType = MemberType.Method;
                            var syntaxDescription = new MethodSyntaxDescription();
                            syntaxDescription.Syntax = baseSyntax.Syntax;
                            syntaxDescription.Comments = baseSyntax.Comments;

                            var methodSymbol = symbol as IMethodSymbol;
                            Debug.Assert(methodSymbol != null);

                            foreach (var p in methodSymbol.Parameters)
                            {
                                syntaxDescription.Parameters.Add(GetParameterDescription(p));
                            }

                            syntaxDescription.Return = GetParameterDescription(methodSymbol.ReturnType);

                            syntaxDescription.Syntax = syntax.WithBody(null)
                                    .NormalizeWhitespace()
                                    .ToString()
                                    .Trim();

                            membersMemberMetadata.SyntaxDescriptionGroup[SyntaxLanguage.CSharp] = syntaxDescription;
                            return membersMemberMetadata;
                        }

                        var constructorSyntax = syntaxNode as ConstructorDeclarationSyntax;

                        if (constructorSyntax != null)
                        {
                            var baseSyntax = membersMemberMetadata.SyntaxDescriptionGroup[SyntaxLanguage.CSharp];
                            membersMemberMetadata.MemberType = MemberType.Constructor;
                            var syntaxDescription = new ConstructorSyntaxDescription();
                            syntaxDescription.Syntax = baseSyntax.Syntax;
                            syntaxDescription.Comments = baseSyntax.Comments;
                            
                            var methodSymbol = symbol as IMethodSymbol;
                            Debug.Assert(methodSymbol != null);

                            foreach (var p in methodSymbol.Parameters)
                            {
                                syntaxDescription.Parameters.Add(GetParameterDescription(p));
                            }

                            syntaxDescription.Syntax = constructorSyntax.WithBody(null)
                                    .NormalizeWhitespace()
                                    .ToString()
                                    .Trim();

                            membersMemberMetadata.SyntaxDescriptionGroup[SyntaxLanguage.CSharp] = syntaxDescription;
                            return membersMemberMetadata;
                        }

                        return null;
                    };
                case SymbolKind.Property:
                    {
                        var syntax = syntaxNode as PropertyDeclarationSyntax;

                        if (syntax == null)
                        {
                            return null;
                        }
                        var baseSyntax = membersMemberMetadata.SyntaxDescriptionGroup[SyntaxLanguage.CSharp];
                        membersMemberMetadata.MemberType = MemberType.Property;
                        var syntaxDescription = new PropertySyntaxDescription();
                        syntaxDescription.Syntax = baseSyntax.Syntax;
                        syntaxDescription.Comments = baseSyntax.Comments;

                        var propertySymbol = symbol as IPropertySymbol;
                        Debug.Assert(propertySymbol != null);

                        syntaxDescription.Property = GetParameterDescription(propertySymbol);

                        syntaxDescription.Syntax = syntax
                                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                                .WithAccessorList(null)
                                .NormalizeWhitespace()
                                .ToString()
                                .Trim();

                        membersMemberMetadata.SyntaxDescriptionGroup[SyntaxLanguage.CSharp] = syntaxDescription;
                        return membersMemberMetadata;
                    };
                case SymbolKind.Field:
                    {
                        if (syntaxNode is VariableDeclarationSyntax || syntaxNode is MemberDeclarationSyntax)
                        {
                            var baseSyntax = membersMemberMetadata.SyntaxDescriptionGroup[SyntaxLanguage.CSharp];
                            membersMemberMetadata.MemberType = MemberType.Field;

                            var varSyntax = syntaxNode as VariableDeclaratorSyntax;
                            if (varSyntax != null)
                            {
                                baseSyntax.Syntax = varSyntax
                                        .WithInitializer(null)
                                        .NormalizeWhitespace()
                                        .ToString()
                                        .Trim();
                            }

                            // For Enum's member
                            var memberSyntax = syntaxNode as MemberDeclarationSyntax;

                            if (memberSyntax != null)
                            {
                                baseSyntax.Syntax = memberSyntax
                                        .NormalizeWhitespace()
                                        .ToString()
                                        .Trim();
                            }

                            return membersMemberMetadata;
                        }
                        else
                        {
                            return null;
                        }
                    };
                case SymbolKind.Event:
                    {
                        var syntax = syntaxNode as VariableDeclaratorSyntax;

                        if (syntax == null)
                        {
                            return null;
                        }
                        var baseSyntax = membersMemberMetadata.SyntaxDescriptionGroup[SyntaxLanguage.CSharp];
                        membersMemberMetadata.MemberType = MemberType.Event;

                        baseSyntax.Syntax = syntax
                                .NormalizeWhitespace()
                                .ToString()
                                .Trim();
                        return membersMemberMetadata;
                    };
                default: return null;
            }
        }

        private ParameterDescription GetParameterDescription(ISymbol symbol)
        {
            string id = null;

            // TODO: GetDocumentationCommentXml for parameters seems not accurate
            string comment = symbol.GetDocumentationCommentXml();
            string name = symbol.Name;
            var paraSymbol = symbol as IParameterSymbol;
            if (paraSymbol != null)
            {
                // TODO: why BaseType?
                id = paraSymbol.Type.GetDocumentationCommentId() ?? paraSymbol.Type.BaseType.GetDocumentationCommentId();
            }

            var typeSymbol = symbol as ITypeSymbol;
            if (typeSymbol != null)
            {
                // TODO: check what name is
                // name = DescriptionConstants.ReturnName;
                id = typeSymbol.GetDocumentationCommentId() ?? typeSymbol.BaseType.GetDocumentationCommentId();
            }

            var propertySymbol = symbol as IPropertySymbol;
            if (propertySymbol != null)
            {
                // TODO: check what name is
                // name = DescriptionConstants.ReturnName;
                id = propertySymbol.Type.GetDocumentationCommentId() ?? propertySymbol.Type.BaseType.GetDocumentationCommentId();
            }

            return new ParameterDescription() { Name = name, Type = new Identity(id), Comments = new List<CommentBase> { new TripleSlashComment { Raw = comment } } };
        }
    }
}
