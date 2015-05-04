﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace DocAsCode.EntityModel
{
    using Microsoft.CodeAnalysis.CSharp;
    using System.IO;

    public class CSYamlModelGeneratorVisitor : YamlModelGeneratorVisitor
    {
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

        public CSYamlModelGeneratorVisitor(object context, Compilation compilation) : base(context, compilation, SyntaxLanguage.CSharp)
        {
        }

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

        public override string GetSyntaxContent(MemberType typeKind, SyntaxNode syntaxNode)
        {
            string syntaxStr = null;
            int openBracketIndex = -1;
            switch (typeKind)
            {
                case MemberType.Class:
                    {
                        var syntax = syntaxNode as ClassDeclarationSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        var symbol = Compilation.GetSemanticModel(syntax.SyntaxTree).GetDeclaredSymbol(syntax);
                        syntaxStr = SyntaxFactory.ClassDeclaration(
                            new SyntaxList<AttributeListSyntax>(),
                            SyntaxFactory.TokenList(GetTypeModifiers(symbol)),
                            SyntaxFactory.Identifier(symbol.Name),
                            GetTypeParameters(symbol),
                            GetBaseTypeList(symbol),
                            SyntaxFactory.List(GetTypeParameterConstraints(symbol)),
                            new SyntaxList<MemberDeclarationSyntax>())
                            .NormalizeWhitespace()
                            .ToString();
                        openBracketIndex = syntaxStr.IndexOf(syntax.OpenBraceToken.ValueText);
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
                case MemberType.Enum:
                    {
                        var syntax = syntaxNode as EnumDeclarationSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        var symbol = Compilation.GetSemanticModel(syntax.SyntaxTree).GetDeclaredSymbol(syntax);
                        syntaxStr = SyntaxFactory.EnumDeclaration(
                            new SyntaxList<AttributeListSyntax>(),
                            SyntaxFactory.TokenList(GetTypeModifiers(symbol)),
                            SyntaxFactory.Identifier(symbol.Name),
                            GetEnumBaseTypeList(symbol),
                            new SeparatedSyntaxList<EnumMemberDeclarationSyntax>())
                            .NormalizeWhitespace()
                            .ToString();
                        openBracketIndex = syntaxStr.IndexOf(syntax.OpenBraceToken.ValueText);
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
                        var syntax = syntaxNode as InterfaceDeclarationSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        var symbol = Compilation.GetSemanticModel(syntax.SyntaxTree).GetDeclaredSymbol(syntax);
                        syntaxStr = SyntaxFactory.InterfaceDeclaration(
                            new SyntaxList<AttributeListSyntax>(),
                            SyntaxFactory.TokenList(GetTypeModifiers(symbol)),
                            SyntaxFactory.Identifier(symbol.Name),
                            GetTypeParameters(symbol),
                            GetBaseTypeList(symbol),
                            SyntaxFactory.List(GetTypeParameterConstraints(symbol)),
                            new SyntaxList<MemberDeclarationSyntax>())
                            .NormalizeWhitespace()
                            .ToString();
                        openBracketIndex = syntaxStr.IndexOf(syntax.OpenBraceToken.ValueText);
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
                        var syntax = syntaxNode as StructDeclarationSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        var symbol = Compilation.GetSemanticModel(syntax.SyntaxTree).GetDeclaredSymbol(syntax);
                        syntaxStr = SyntaxFactory.StructDeclaration(
                            new SyntaxList<AttributeListSyntax>(),
                            SyntaxFactory.TokenList(GetTypeModifiers(symbol)),
                            SyntaxFactory.Identifier(symbol.Name),
                            GetTypeParameters(symbol),
                            GetBaseTypeList(symbol),
                            SyntaxFactory.List(GetTypeParameterConstraints(symbol)),
                            new SyntaxList<MemberDeclarationSyntax>())
                            .NormalizeWhitespace()
                            .ToString();
                        openBracketIndex = syntaxStr.IndexOf(syntax.OpenBraceToken.ValueText);
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
                        var syntax = syntaxNode as DelegateDeclarationSyntax;
                        Debug.Assert(syntax != null);
                        if (syntax == null) break;
                        var symbol = Compilation.GetSemanticModel(syntax.SyntaxTree).GetDeclaredSymbol(syntax);
                        syntaxStr = SyntaxFactory.DelegateDeclaration(
                            new SyntaxList<AttributeListSyntax>(),
                            SyntaxFactory.TokenList(GetTypeModifiers(symbol)),
                            SyntaxFactory.ParseTypeName(symbol.DelegateInvokeMethod.ReturnType.ToDisplayString(ShortFormat)),
                            SyntaxFactory.Identifier(symbol.Name),
                            GetTypeParameters(symbol),
                            SyntaxFactory.ParameterList(
                                SyntaxFactory.SeparatedList(
                                    from p in symbol.DelegateInvokeMethod.Parameters
                                    select GetParameter(p))),
                            SyntaxFactory.List(GetTypeParameterConstraints(symbol)))
                            .NormalizeWhitespace()
                            .ToString();
                        break;
                    };
                case MemberType.Method:
                    {
                        var syntax = syntaxNode as MethodDeclarationSyntax;
                        if (syntax != null)
                        {
                            syntaxStr = syntax.WithBody(null)
                                .NormalizeWhitespace()
                                .ToString()
                                .Trim();
                            break;
                        }
                        var opertatorSyntax = syntaxNode as OperatorDeclarationSyntax;
                        if (opertatorSyntax != null)
                        {
                            syntaxStr = opertatorSyntax.WithBody(null)
                                .NormalizeWhitespace()
                                .ToString()
                                .Trim();
                            break;
                        }
                        break;
                    };
                case MemberType.Constructor:
                    {
                        var syntax = syntaxNode as ConstructorDeclarationSyntax;
                        if (syntax != null)
                        {
                            syntaxStr = syntax.WithBody(null)
                            .NormalizeWhitespace()
                            .ToString()
                            .Trim();
                        }

                        break;
                    };
                case MemberType.Field:
                    {
                        var syntax = syntaxNode as VariableDeclaratorSyntax;
                        if (syntax != null)
                        {
                            if (syntax.Parent as VariableDeclarationSyntax != null && syntax.Parent.Parent as FieldDeclarationSyntax != null)
                            {
                                var variableDeclarationSyntax = syntax.Parent.ReplaceNode(syntax, syntax.WithInitializer(null)) as VariableDeclarationSyntax;
                                var fieldDeclarationSyntax = syntax.Parent.Parent.ReplaceNode(syntax.Parent, variableDeclarationSyntax) as FieldDeclarationSyntax;
                                syntaxStr = fieldDeclarationSyntax
                                    .NormalizeWhitespace()
                                    .ToString()
                                    .Trim();
                            }
                        }

                        break;
                    };
                case MemberType.Event:
                    {
                        var syntax = syntaxNode as EventDeclarationSyntax;
                        if (syntax != null)
                        {
                            syntaxStr = syntax.WithoutTrivia().NormalizeWhitespace().ToString().Trim();
                            syntaxStr = Regex.Replace(syntaxStr, @"\s*\{(\S|\s)*", "");
                            break;
                        }
                        var variable = syntaxNode as VariableDeclaratorSyntax;
                        if (variable != null)
                        {
                            syntaxStr = variable.Parent.Parent.NormalizeWhitespace().ToString().Trim();
                            break;
                        }
                        break;
                    };
                case MemberType.Property:
                    {
                        Debug.Assert(syntaxNode is PropertyDeclarationSyntax || syntaxNode is IndexerDeclarationSyntax);

                        var syntax = syntaxNode as PropertyDeclarationSyntax;
                        if (syntax != null)
                        {
                            SyntaxList<AccessorDeclarationSyntax> accessorList;
                            if (syntax.AccessorList != null)
                            {
                                var accessors = syntax.AccessorList.Accessors.Where(x => !x.Modifiers.Any(SyntaxKind.PrivateKeyword) && !x.Modifiers.Any(SyntaxKind.InternalKeyword));
                                accessorList = new SyntaxList<AccessorDeclarationSyntax>().AddRange(accessors);
                            }
                            else if (syntax.ExpressionBody != null)
                            {
                                // If it's an expression bodied property, it should have a getter accessor
                                var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration);
                                accessorList = new SyntaxList<AccessorDeclarationSyntax>().Add(getter);
                            }
                            else
                            {
                                throw new InvalidDataException(string.Format("Property declaration '{0}' does not have any accessor.", syntax));
                            }

                            var simplifiedAccessorList = accessorList.Select(s => s.WithBody(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                            SyntaxList<AccessorDeclarationSyntax> syntaxList = new SyntaxList<AccessorDeclarationSyntax>();
                            syntaxList = syntaxList.AddRange(simplifiedAccessorList);
                            var simplifiedSyntax = syntax.WithExpressionBody(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None)).WithAccessorList(SyntaxFactory.AccessorList(syntaxList));
                            syntaxStr = simplifiedSyntax.NormalizeWhitespace().ToString().Trim();
                        }
                        else
                        {
                            var syntaxIndexer = syntaxNode as IndexerDeclarationSyntax;
                            if (syntaxIndexer != null)
                            {
                                var accessorList = syntaxIndexer.AccessorList.Accessors;
                                var simplifiedAccessorList = accessorList.Select(s => s.WithBody(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                                SyntaxList<AccessorDeclarationSyntax> syntaxList = new SyntaxList<AccessorDeclarationSyntax>();
                                syntaxList = syntaxList.AddRange(simplifiedAccessorList);
                                var simplifiedSyntax = syntaxIndexer.WithAccessorList(SyntaxFactory.AccessorList(syntaxList));
                                syntaxStr = simplifiedSyntax.NormalizeWhitespace().ToString().Trim();
                            }
                        }

                        break;
                    };
            }

            if (string.IsNullOrEmpty(syntaxStr)) syntaxStr = syntaxNode.NormalizeWhitespace().ToString().Trim();
            return syntaxStr;
        }

        private static ParameterSyntax GetParameter(IParameterSymbol p)
        {
            return SyntaxFactory.Parameter(
                new SyntaxList<AttributeListSyntax>(),
                SyntaxFactory.TokenList(GetParameterModifiers(p)),
                SyntaxFactory.ParseTypeName(p.Type.ToDisplayString(ShortFormat)),
                SyntaxFactory.Identifier(p.Name),
                GetDefaultValueClause(p));
        }

        private static IEnumerable<SyntaxToken> GetParameterModifiers(IParameterSymbol parameter)
        {
            switch (parameter.RefKind)
            {
                case RefKind.None:
                    break;
                case RefKind.Ref:
                    yield return SyntaxFactory.Token(SyntaxKind.RefKeyword);
                    break;
                case RefKind.Out:
                    yield return SyntaxFactory.Token(SyntaxKind.OutKeyword);
                    break;
                default:
                    break;
            }
            if (parameter.IsParams)
            {
                yield return SyntaxFactory.Token(SyntaxKind.ParamsKeyword);
            }
        }

        private static EqualsValueClauseSyntax GetDefaultValueClause(IParameterSymbol parameter)
        {
            if (parameter.HasExplicitDefaultValue)
            {
                if (parameter.ExplicitDefaultValue == null)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NullLiteralExpression,
                            SyntaxFactory.Token(SyntaxKind.NullKeyword)));
                }
                else if (parameter.ExplicitDefaultValue is long)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralToken,
                            SyntaxFactory.Literal((long)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is ulong)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralToken,
                            SyntaxFactory.Literal((ulong)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is int)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralToken,
                            SyntaxFactory.Literal((int)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is uint)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralToken,
                            SyntaxFactory.Literal((uint)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is short)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralToken,
                            SyntaxFactory.Literal((short)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is ushort)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralToken,
                            SyntaxFactory.Literal((ushort)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is byte)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralToken,
                            SyntaxFactory.Literal((byte)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is sbyte)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralToken,
                            SyntaxFactory.Literal((sbyte)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is char)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.CharacterLiteralToken,
                            SyntaxFactory.Literal((char)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is string)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralToken,
                            SyntaxFactory.Literal((string)parameter.ExplicitDefaultValue)));
                }
            }
            return null;
        }

        private static IEnumerable<TypeParameterConstraintClauseSyntax> GetTypeParameterConstraints(INamedTypeSymbol symbol)
        {
            if (symbol.TypeArguments.Length == 0)
            {
                yield break;
            }
            foreach (ITypeParameterSymbol ta in symbol.TypeArguments)
            {
                if (ta.HasConstructorConstraint || ta.HasReferenceTypeConstraint || ta.HasValueTypeConstraint || ta.ConstraintTypes.Length > 0)
                {
                    yield return SyntaxFactory.TypeParameterConstraintClause(SyntaxFactory.IdentifierName(ta.Name), SyntaxFactory.SeparatedList(GetTypeParameterConstraint(ta)));
                }
            }
        }

        private static IEnumerable<TypeParameterConstraintSyntax> GetTypeParameterConstraint(ITypeParameterSymbol symbol)
        {
            if (symbol.HasReferenceTypeConstraint)
            {
                yield return SyntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint);
            }
            if (symbol.HasValueTypeConstraint)
            {
                yield return SyntaxFactory.ClassOrStructConstraint(SyntaxKind.StructConstraint);
            }
            if (symbol.ConstraintTypes.Length > 0)
            {
                for (int i = 0; i < symbol.ConstraintTypes.Length; i++)
                {
                    yield return SyntaxFactory.TypeConstraint(SyntaxFactory.ParseTypeName(symbol.ConstraintTypes[i].ToDisplayString(ShortFormat)));
                }
            }
            if (symbol.HasConstructorConstraint)
            {
                yield return SyntaxFactory.ConstructorConstraint();
            }
        }

        private BaseListSyntax GetBaseTypeList(INamedTypeSymbol symbol)
        {
            IReadOnlyList<INamedTypeSymbol> baseTypeList;
            if (symbol.TypeKind != TypeKind.Class || symbol.BaseType.GetDocumentationCommentId() == "T:System.Object")
            {
                baseTypeList = symbol.AllInterfaces;
            }
            else
            {
                baseTypeList = new[] { symbol.BaseType }.Concat(symbol.AllInterfaces).ToList();
            }
            if (baseTypeList.Count == 0)
            {
                return null;
            }
            return SyntaxFactory.BaseList(
                SyntaxFactory.SeparatedList<BaseTypeSyntax>(
                    from t in baseTypeList
                    select SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(t.ToDisplayString(ShortDisplayFormat)))));
        }

        private BaseListSyntax GetEnumBaseTypeList(INamedTypeSymbol symbol)
        {
            var underlyingType = symbol.EnumUnderlyingType;
            if (underlyingType.GetDocumentationCommentId() == "T:System.Int32")
            {
                return null;
            }
            return SyntaxFactory.BaseList(
                SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                    SyntaxFactory.SimpleBaseType(
                        SyntaxFactory.ParseTypeName(
                            underlyingType.ToDisplayString(ShortDisplayFormat)))));
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
                        new SyntaxList<AttributeListSyntax>(),
                        GetVarianceToken(t),
                        SyntaxFactory.Identifier(t.Name))));
        }

        private static SyntaxToken GetVarianceToken(ITypeParameterSymbol t)
        {
            if (t.Variance == VarianceKind.In)
                return SyntaxFactory.Token(SyntaxKind.InKeyword);
            if (t.Variance == VarianceKind.Out)
                return SyntaxFactory.Token(SyntaxKind.OutKeyword);
            return new SyntaxToken();
        }

        private static IEnumerable<SyntaxToken> GetTypeModifiers(INamedTypeSymbol symbol)
        {
            switch (symbol.DeclaredAccessibility)
            {
                case Accessibility.Private:
                    yield return SyntaxFactory.Token(SyntaxKind.PrivateKeyword);
                    break;
                case Accessibility.ProtectedAndInternal:
                    // todo : not supported in c#.
                    break;
                case Accessibility.Protected:
                    yield return SyntaxFactory.Token(SyntaxKind.ProtectedKeyword);
                    break;
                case Accessibility.Internal:
                    yield return SyntaxFactory.Token(SyntaxKind.InternalKeyword);
                    break;
                case Accessibility.ProtectedOrInternal:
                    yield return SyntaxFactory.Token(SyntaxKind.ProtectedKeyword);
                    yield return SyntaxFactory.Token(SyntaxKind.InternalKeyword);
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
                    yield return SyntaxFactory.Token(SyntaxKind.StaticKeyword);
                }
                else
                {
                    if (symbol.IsAbstract)
                    {
                        yield return SyntaxFactory.Token(SyntaxKind.AbstractKeyword);
                    }
                    if (symbol.IsSealed)
                    {
                        yield return SyntaxFactory.Token(SyntaxKind.SealedKeyword);
                    }
                }
            }
        }
    }
}
