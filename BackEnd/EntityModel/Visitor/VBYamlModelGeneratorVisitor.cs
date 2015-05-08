using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using DocAsCode.Utility;

namespace DocAsCode.EntityModel
{
    public class VBYamlModelGeneratorVisitor : YamlModelGeneratorVisitor
    {
        #region Fields
        private static readonly SymbolDisplayFormat ShortFormat = new SymbolDisplayFormat(
            SymbolDisplayGlobalNamespaceStyle.Omitted,
            SymbolDisplayTypeQualificationStyle.NameOnly,
            SymbolDisplayGenericsOptions.IncludeTypeParameters,
            SymbolDisplayMemberOptions.IncludeExplicitInterface | SymbolDisplayMemberOptions.IncludeParameters,
            SymbolDisplayDelegateStyle.NameOnly,
            SymbolDisplayExtensionMethodStyle.Default,
            SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut,
            SymbolDisplayPropertyStyle.NameOnly,
            SymbolDisplayLocalOptions.None,
            SymbolDisplayKindOptions.None,
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseAsterisksInMultiDimensionalArrays | SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName);
        private static readonly Regex EndRegex = new Regex(@"\s+End\s*\w*\s*$", RegexOptions.Compiled);
        #endregion

        public VBYamlModelGeneratorVisitor(object context) : base(context, SyntaxLanguage.VB)
        {
        }

        #region Overrides

        public override SymbolDisplayFormat ShortDisplayFormat
        {
            get
            {
                return ShortFormat;
            }
        }

        public override SymbolDisplayFormat DisplayFormat
        {
            get
            {
                return SymbolDisplayFormat.CSharpErrorMessageFormat;
            }
        }

        public override string GetSyntaxContent(MemberType typeKind, ISymbol symbol, SyntaxNode syntaxNode)
        {
            string syntaxStr = null;
            int openBracketIndex = -1;
            switch (typeKind)
            {
                case MemberType.Class:
                    {
                        var typeSymbol = (INamedTypeSymbol)symbol;
                        syntaxStr = SyntaxFactory.ClassBlock(
                            SyntaxFactory.ClassStatement(
                                new SyntaxList<AttributeListSyntax>(),
                                SyntaxFactory.TokenList(GetTypeModifiers(typeSymbol)),
                                SyntaxFactory.Token(SyntaxKind.ClassKeyword),
                                SyntaxFactory.Identifier(typeSymbol.Name),
                                GetTypeParameters(typeSymbol)),
                            GetInheritsList(typeSymbol),
                            GetImplementsList(typeSymbol),
                            new SyntaxList<StatementSyntax>(),
                            SyntaxFactory.EndClassStatement())
                            .NormalizeWhitespace()
                            .ToString();
                        syntaxStr = RemoveEnd(syntaxStr);
                        break;
                    };
                case MemberType.Enum:
                    {
                        var syntax = syntaxNode as EnumStatementSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        syntaxStr
                            = syntax
                                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                                .NormalizeWhitespace()
                                .ToString();
                        if (openBracketIndex > -1)
                        {
                            syntaxStr = syntaxStr.Substring(0, openBracketIndex).Trim();
                        }
                        else
                        {
                            syntaxStr = syntaxStr.Trim();
                        }
                        break;
                    };
                case MemberType.Interface:
                    {
                        var syntax = syntaxNode as InterfaceStatementSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        syntaxStr =
                            syntax
                                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                                .NormalizeWhitespace()
                                .ToString();

                        if (openBracketIndex > -1)
                        {
                            syntaxStr = syntaxStr.Substring(0, openBracketIndex).Trim();
                        }
                        else
                        {
                            syntaxStr = syntaxStr.Trim();
                        }
                        break;
                    };
                case MemberType.Struct:
                    {
                        var syntax = syntaxNode as StructureStatementSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        syntaxStr = syntax
                                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                                .NormalizeWhitespace()
                                .ToString();
                        if (openBracketIndex > -1)
                        {
                            syntaxStr = syntaxStr.Substring(0, openBracketIndex).Trim();
                        }
                        else
                        {
                            syntaxStr = syntaxStr.Trim();
                        }
                        break;
                    };
                case MemberType.Delegate:
                    {
                        var syntax = syntaxNode as DelegateStatementSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        syntaxStr = syntax
                                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                                .NormalizeWhitespace()
                                .ToString();
                        if (openBracketIndex > -1)
                        {
                            syntaxStr = syntaxStr.Substring(0, openBracketIndex).Trim();
                        }
                        else
                        {
                            syntaxStr = syntaxStr.Trim();
                        }
                        break;
                    };
                case MemberType.Method:
                    {
                        var syntax = syntaxNode as MethodStatementSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        syntaxStr = syntax
                            .ToString()
                            .Trim();
                        break;
                    };
                case MemberType.Operator:
                    {
                        break;
                    };
                case MemberType.Constructor:
                    {
                        var syntax = syntaxNode as ConstructorBlockSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        syntaxStr = syntax.WithBlockStatement(null)
                            .NormalizeWhitespace()
                            .ToString()
                            .Trim();
                        break;
                    };
                case MemberType.Field:
                    {
                        var syntax = syntaxNode as FieldDeclarationSyntax;
                        // TODO: Why is it ModifiedIdentifierSyntax?
                        Debug.Assert(syntaxNode is ModifiedIdentifierSyntax || syntax != null);
                        if (syntax != null)
                        {
                            syntaxStr = syntax
                            .ToString()
                            .Trim();
                        }
                        else
                        {
                            var identifier = syntaxNode as ModifiedIdentifierSyntax;
                            if (identifier != null)
                            {
                                syntaxStr = identifier.Parent.Parent.ToString().Trim();
                            }
                        }

                        break;
                    };
                case MemberType.Event:
                    {
                        var syntax = syntaxNode as VariableDeclaratorSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        syntaxStr = syntax
                            .NormalizeWhitespace()
                            .ToString()
                            .Trim();
                        break;
                    };
                case MemberType.Property:
                    {
                        var syntax = syntaxNode as PropertyStatementSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        syntaxStr = syntax
                                    .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                                    .ToString()
                                    .Trim();
                        break;
                    };
            }

            return syntaxStr;
        }

        #endregion

        #region Private Methods

        private static IEnumerable<SyntaxToken> GetTypeModifiers(INamedTypeSymbol symbol)
        {
            switch (symbol.DeclaredAccessibility)
            {
                case Accessibility.Protected:
                case Accessibility.ProtectedOrFriend:
                    yield return SyntaxFactory.Token(SyntaxKind.ProtectedKeyword);
                    break;
                case Accessibility.Public:
                    yield return SyntaxFactory.Token(SyntaxKind.PublicKeyword);
                    break;
                default:
                    break;
            }
            if (symbol.TypeKind == TypeKind.Class)
            {
                if (symbol.IsAbstract && symbol.IsSealed)
                {
                    yield return SyntaxFactory.Token(SyntaxKind.NotInheritableKeyword);
                }
                else
                {
                    if (symbol.IsAbstract)
                    {
                        yield return SyntaxFactory.Token(SyntaxKind.MustInheritKeyword);
                    }
                    if (symbol.IsSealed)
                    {
                        yield return SyntaxFactory.Token(SyntaxKind.NotInheritableKeyword);
                    }
                }
            }
        }

        private static TypeParameterListSyntax GetTypeParameters(INamedTypeSymbol symbol)
        {
            if (symbol.TypeArguments.Length == 0)
            {
                return null;
            }
            return SyntaxFactory.TypeParameterList(
                SyntaxFactory.SeparatedList(
                    from ITypeParameterSymbol t in symbol.TypeArguments
                    select SyntaxFactory.TypeParameter(
                        GetVarianceToken(t),
                        SyntaxFactory.Identifier(t.Name),
                        GetTypeParameterConstraintClauseSyntax(t))));
        }

        private static SyntaxToken GetVarianceToken(ITypeParameterSymbol t)
        {
            if (t.Variance == VarianceKind.In)
                return SyntaxFactory.Token(SyntaxKind.InKeyword);
            if (t.Variance == VarianceKind.Out)
                return SyntaxFactory.Token(SyntaxKind.OutKeyword);
            return new SyntaxToken();
        }

        private static TypeParameterConstraintClauseSyntax GetTypeParameterConstraintClauseSyntax(ITypeParameterSymbol symbol)
        {
            var contraints = GetConstraintSyntaxes(symbol).ToList();
            if (contraints.Count == 0)
            {
                return null;
            }
            if (contraints.Count == 1)
            {
                return SyntaxFactory.TypeParameterSingleConstraintClause(contraints[0]);
            }
            return SyntaxFactory.TypeParameterMultipleConstraintClause(contraints.ToArray());
        }

        private static IEnumerable<ConstraintSyntax> GetConstraintSyntaxes(ITypeParameterSymbol symbol)
        {
            if (symbol.HasReferenceTypeConstraint)
            {
                yield return SyntaxFactory.ClassConstraint(SyntaxFactory.Token(SyntaxKind.ClassKeyword));
            }
            if (symbol.HasValueTypeConstraint)
            {
                yield return SyntaxFactory.StructureConstraint(SyntaxFactory.Token(SyntaxKind.StructureKeyword));
            }
            if (symbol.ConstraintTypes.Length > 0)
            {
                for (int i = 0; i < symbol.ConstraintTypes.Length; i++)
                {
                    yield return SyntaxFactory.TypeConstraint(GetTypeSyntax(symbol.ConstraintTypes[i]));
                }
            }
            if (symbol.HasConstructorConstraint)
            {
                yield return SyntaxFactory.NewConstraint(SyntaxFactory.Token(SyntaxKind.NewKeyword));
            }
        }

        private SyntaxList<InheritsStatementSyntax> GetInheritsList(INamedTypeSymbol symbol)
        {
            if (symbol.TypeKind == TypeKind.Class && symbol.BaseType.GetDocumentationCommentId() != "T:System.Object")
            {
                return SyntaxFactory.SingletonList(SyntaxFactory.InheritsStatement(GetTypeSyntax(symbol.BaseType)));
            }
            if (symbol.TypeKind == TypeKind.Interface)
            {
                return SyntaxFactory.SingletonList(SyntaxFactory.InheritsStatement(
                    (from t in symbol.Interfaces
                     select GetTypeSyntax(t)).ToArray()));
            }
            return new SyntaxList<InheritsStatementSyntax>();
        }

        private SyntaxList<ImplementsStatementSyntax> GetImplementsList(INamedTypeSymbol symbol)
        {
            if (symbol.TypeKind != TypeKind.Interface && symbol.AllInterfaces.Any())
            {
                return SyntaxFactory.SingletonList(SyntaxFactory.ImplementsStatement(
                    (from t in symbol.AllInterfaces
                     select GetTypeSyntax(t)).ToArray()));
            }
            return new SyntaxList<ImplementsStatementSyntax>();
        }

        private static TypeSyntax GetTypeSyntax(ITypeSymbol type)
        {
            // todo : need to verify it when type.language is not vb
            return SyntaxFactory.ParseTypeName(type.ToDisplayString(ShortFormat));
        }

        private static string RemoveEnd(string code)
        {
            return EndRegex.Replace(code, string.Empty);
        }

        #endregion
    }
}
