namespace DocAsCode.EntityModel
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using DocAsCode.Utility;

    using Microsoft.CodeAnalysis;

    public static class VisitorHelper
    {
        public static bool CanVisit(ISymbol symbol)
        {
            if (symbol.DeclaredAccessibility == Accessibility.NotApplicable)
            {
                return true;
            }

            if (symbol.IsImplicitlyDeclared)
            {
                return false;
            }

            if (symbol.DeclaredAccessibility != Accessibility.Public)
            {
                return false;
            }

            return true;
        }

        public static void FeedComments(MetadataItem item)
        {
            if (string.IsNullOrEmpty(item.RawComment)) return;
            item.Summary = TripleSlashCommentParser.GetSummary(item.RawComment, true);
        }

        public static string GetId(ISymbol symbol)
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

        public static SourceDetail GetLinkDetail(ISymbol symbol)
        {
            if (symbol == null)
            {
                return null;
            }
            string id = symbol.GetDocumentationCommentId();
            if (string.IsNullOrEmpty(id))
            {
                var typeSymbol = symbol as ITypeSymbol;
                if (typeSymbol?.BaseType != null)
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
        public static ApiParameter GetParameterDescription(ISymbol symbol, MetadataItem item, bool isReturn)
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

            return new ApiParameter() { Name = name, Type = id, Description = comment };
        }

        public static SourceDetail GetSourceDetail(ISymbol symbol)
        {
            if (symbol == null)
            {
                return null;
            }

            var syntaxRef = symbol.DeclaringSyntaxReferences.LastOrDefault();
            if (symbol.IsExtern || syntaxRef == null)
            {
                return new SourceDetail { IsExternalPath = true, Path = symbol.ContainingAssembly?.Name };
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
                if (source.Remote != null) source.Path = source.Path.FormatPath(UriKind.Relative, source.Remote.LocalWorkingDirectory);
                return source;
            }

            return null;
        }

        public static MemberType GetMemberTypeFromTypeKind(TypeKind typeKind)
        {
            switch (typeKind)
            {
                case TypeKind.Class:
                    return MemberType.Class;
                case TypeKind.Enum:
                    return MemberType.Enum;
                case TypeKind.Interface:
                    return MemberType.Interface;
                case TypeKind.Struct:
                    return MemberType.Struct;
                case TypeKind.Delegate:
                    return MemberType.Delegate;
                default:
                    return MemberType.Default;
            }
        }
    }
}
