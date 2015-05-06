using Microsoft.CodeAnalysis;
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
        private static readonly SymbolDisplayFormat EiiMethodFormat = new SymbolDisplayFormat();
        private static readonly SymbolDisplayFormat EiiContainerTypeFormat = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseAsterisksInMultiDimensionalArrays | SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName);
        private static readonly Regex BracesRegex = new Regex(@"\s*\{(\S|\s)*", RegexOptions.Compiled);
        #endregion

        public CSYamlModelGeneratorVisitor(object context) : base(context, SyntaxLanguage.CSharp)
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
            switch (typeKind)
            {
                case MemberType.Class:
                    {
                        var typeSymbol = (INamedTypeSymbol)symbol;
                        syntaxStr = SyntaxFactory.ClassDeclaration(
                            new SyntaxList<AttributeListSyntax>(),
                            SyntaxFactory.TokenList(GetTypeModifiers(typeSymbol)),
                            SyntaxFactory.Identifier(typeSymbol.Name),
                            GetTypeParameters(typeSymbol),
                            GetBaseTypeList(typeSymbol),
                            SyntaxFactory.List(GetTypeParameterConstraints(typeSymbol)),
                            new SyntaxList<MemberDeclarationSyntax>())
                            .NormalizeWhitespace()
                            .ToString();
                        syntaxStr = RemoveBraces(syntaxStr);
                        break;
                    }
                case MemberType.Enum:
                    {
                        var typeSymbol = (INamedTypeSymbol)symbol;
                        syntaxStr = SyntaxFactory.EnumDeclaration(
                            new SyntaxList<AttributeListSyntax>(),
                            SyntaxFactory.TokenList(GetTypeModifiers(typeSymbol)),
                            SyntaxFactory.Identifier(typeSymbol.Name),
                            GetEnumBaseTypeList(typeSymbol),
                            new SeparatedSyntaxList<EnumMemberDeclarationSyntax>())
                            .NormalizeWhitespace()
                            .ToString();
                        syntaxStr = RemoveBraces(syntaxStr);
                        break;
                    }
                case MemberType.Interface:
                    {
                        var typeSymbol = (INamedTypeSymbol)symbol;
                        syntaxStr = SyntaxFactory.InterfaceDeclaration(
                            new SyntaxList<AttributeListSyntax>(),
                            SyntaxFactory.TokenList(GetTypeModifiers(typeSymbol)),
                            SyntaxFactory.Identifier(typeSymbol.Name),
                            GetTypeParameters(typeSymbol),
                            GetBaseTypeList(typeSymbol),
                            SyntaxFactory.List(GetTypeParameterConstraints(typeSymbol)),
                            new SyntaxList<MemberDeclarationSyntax>())
                            .NormalizeWhitespace()
                            .ToString();
                        syntaxStr = RemoveBraces(syntaxStr);
                        break;
                    }
                case MemberType.Struct:
                    {
                        var typeSymbol = (INamedTypeSymbol)symbol;
                        syntaxStr = SyntaxFactory.StructDeclaration(
                            new SyntaxList<AttributeListSyntax>(),
                            SyntaxFactory.TokenList(GetTypeModifiers(typeSymbol)),
                            SyntaxFactory.Identifier(typeSymbol.Name),
                            GetTypeParameters(typeSymbol),
                            GetBaseTypeList(typeSymbol),
                            SyntaxFactory.List(GetTypeParameterConstraints(typeSymbol)),
                            new SyntaxList<MemberDeclarationSyntax>())
                            .NormalizeWhitespace()
                            .ToString();
                        syntaxStr = RemoveBraces(syntaxStr);
                        break;
                    }
                case MemberType.Delegate:
                    {
                        var typeSymbol = (INamedTypeSymbol)symbol;
                        syntaxStr = SyntaxFactory.DelegateDeclaration(
                            new SyntaxList<AttributeListSyntax>(),
                            SyntaxFactory.TokenList(GetTypeModifiers(typeSymbol)),
                            GetTypeSyntax(typeSymbol.DelegateInvokeMethod.ReturnType),
                            SyntaxFactory.Identifier(typeSymbol.Name),
                            GetTypeParameters(typeSymbol),
                            SyntaxFactory.ParameterList(
                                SyntaxFactory.SeparatedList(
                                    from p in typeSymbol.DelegateInvokeMethod.Parameters
                                    select GetParameter(p))),
                            SyntaxFactory.List(GetTypeParameterConstraints(typeSymbol)))
                            .NormalizeWhitespace()
                            .ToString();
                        break;
                    }
                case MemberType.Method:
                    {
                        var methodSymbol = (IMethodSymbol)symbol;
                        ExplicitInterfaceSpecifierSyntax eii = null;
                        if (methodSymbol.ExplicitInterfaceImplementations.Length > 0)
                        {
                            eii = SyntaxFactory.ExplicitInterfaceSpecifier(SyntaxFactory.ParseName(GetEiiContainerTypeName(methodSymbol)));
                        }
                        syntaxStr = SyntaxFactory.MethodDeclaration(
                            new SyntaxList<AttributeListSyntax>(),
                            SyntaxFactory.TokenList(GetMemberModifiers(methodSymbol)),
                            GetTypeSyntax(methodSymbol.ReturnType),
                            eii,
                            SyntaxFactory.Identifier(GetMemberName(methodSymbol)),
                            GetTypeParameters(methodSymbol),
                            SyntaxFactory.ParameterList(
                                SyntaxFactory.SeparatedList(
                                    from p in methodSymbol.Parameters
                                    select GetParameter(p))),
                            SyntaxFactory.List(GetTypeParameterConstraints(methodSymbol)),
                            null,
                            null)
                            .NormalizeWhitespace()
                            .ToString();
                        break;
                    }
                case MemberType.Operator:
                    {
                        var methodSymbol = (IMethodSymbol)symbol;
                        var operatorToken = GetOperatorToken(methodSymbol);
                        if (operatorToken == null)
                        {
                            syntaxStr = "Not supported in c#";
                        }
                        else if (operatorToken.Value.Kind() == SyntaxKind.ImplicitKeyword || operatorToken.Value.Kind() == SyntaxKind.ExplicitKeyword)
                        {
                            syntaxStr = SyntaxFactory.ConversionOperatorDeclaration(
                                new SyntaxList<AttributeListSyntax>(),
                                SyntaxFactory.TokenList(GetMemberModifiers(methodSymbol)),
                                operatorToken.Value,
                                GetTypeSyntax(methodSymbol.ReturnType),
                                SyntaxFactory.ParameterList(
                                    SyntaxFactory.SeparatedList(
                                        from p in methodSymbol.Parameters
                                        select GetParameter(p))),
                                null,
                                null)
                                .NormalizeWhitespace()
                                .ToString();
                        }
                        else
                        {
                            syntaxStr = SyntaxFactory.OperatorDeclaration(
                                new SyntaxList<AttributeListSyntax>(),
                                SyntaxFactory.TokenList(GetMemberModifiers(methodSymbol)),
                                GetTypeSyntax(methodSymbol.ReturnType),
                                operatorToken.Value,
                                SyntaxFactory.ParameterList(
                                    SyntaxFactory.SeparatedList(
                                        from p in methodSymbol.Parameters
                                        select GetParameter(p))),
                                null,
                                null)
                                .NormalizeWhitespace()
                                .ToString();
                        }
                        break;
                    }
                case MemberType.Constructor:
                    {
                        var methodSymbol = (IMethodSymbol)symbol;
                        syntaxStr = SyntaxFactory.ConstructorDeclaration(
                            new SyntaxList<AttributeListSyntax>(),
                            SyntaxFactory.TokenList(GetMemberModifiers(methodSymbol)),
                            SyntaxFactory.Identifier(methodSymbol.ContainingType.Name),
                            SyntaxFactory.ParameterList(
                                SyntaxFactory.SeparatedList(
                                    from p in methodSymbol.Parameters
                                    select GetParameter(p))),
                            null,
                            null)
                            .NormalizeWhitespace()
                            .ToString();
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
                            syntaxStr = RemoveBraces(syntaxStr);
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

        #endregion

        #region Private methods

        private static string GetMemberName(IMethodSymbol symbol)
        {
            string name = symbol.Name;
            if (symbol.ExplicitInterfaceImplementations.Length == 0)
            {
                return symbol.Name;
            }
            for (int i = 0; i < symbol.ExplicitInterfaceImplementations.Length; i++)
            {
                if (VisitorHelper.CanVisit(symbol.ExplicitInterfaceImplementations[i]))
                {
                    return symbol.ExplicitInterfaceImplementations[i].ToDisplayString(EiiMethodFormat);
                }
            }
            Debug.Fail("Should not be here!");
            return symbol.Name;
        }

        private static string GetEiiContainerTypeName(IMethodSymbol symbol)
        {
            if (symbol.ExplicitInterfaceImplementations.Length == 0)
            {
                return null;
            }
            for (int i = 0; i < symbol.ExplicitInterfaceImplementations.Length; i++)
            {
                if (VisitorHelper.CanVisit(symbol.ExplicitInterfaceImplementations[i]))
                {
                    return symbol.ExplicitInterfaceImplementations[i].ContainingType.ToDisplayString(EiiContainerTypeFormat);
                }
            }
            Debug.Fail("Should not be here!");
            return null;
        }

        private static ParameterSyntax GetParameter(IParameterSymbol p)
        {
            return SyntaxFactory.Parameter(
                new SyntaxList<AttributeListSyntax>(),
                SyntaxFactory.TokenList(GetParameterModifiers(p)),
                GetTypeSyntax(p.Type),
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
                else if (parameter.ExplicitDefaultValue is bool)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            (bool)parameter.ExplicitDefaultValue ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression));
                }
                else if (parameter.ExplicitDefaultValue is long)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal((long)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is ulong)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal((ulong)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is int)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal((int)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is uint)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal((uint)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is short)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal((short)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is ushort)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal((ushort)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is byte)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal((byte)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is sbyte)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal((sbyte)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is char)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.CharacterLiteralExpression,
                            SyntaxFactory.Literal((char)parameter.ExplicitDefaultValue)));
                }
                else if (parameter.ExplicitDefaultValue is string)
                {
                    return SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
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

        private static IEnumerable<TypeParameterConstraintClauseSyntax> GetTypeParameterConstraints(IMethodSymbol symbol)
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
                    yield return SyntaxFactory.TypeConstraint(GetTypeSyntax(symbol.ConstraintTypes[i]));
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
                    select SyntaxFactory.SimpleBaseType(GetTypeSyntax(t))));
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
                        GetTypeSyntax(underlyingType))));
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

        private static TypeParameterListSyntax GetTypeParameters(IMethodSymbol symbol)
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
                case Accessibility.Protected:
                case Accessibility.ProtectedOrInternal:
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

        private static IEnumerable<SyntaxToken> GetMemberModifiers(IMethodSymbol symbol)
        {
            switch (symbol.DeclaredAccessibility)
            {
                case Accessibility.Protected:
                case Accessibility.ProtectedOrInternal:
                    yield return SyntaxFactory.Token(SyntaxKind.ProtectedKeyword);
                    break;
                case Accessibility.Public:
                    yield return SyntaxFactory.Token(SyntaxKind.PublicKeyword);
                    break;
                case Accessibility.ProtectedAndInternal:
                case Accessibility.Internal:
                case Accessibility.Private:
                default:
                    break;
            }
            if (symbol.IsStatic)
            {
                yield return SyntaxFactory.Token(SyntaxKind.StaticKeyword);
            }
            if (symbol.IsAbstract && symbol.ContainingType.TypeKind != TypeKind.Interface)
            {
                yield return SyntaxFactory.Token(SyntaxKind.AbstractKeyword);
            }
            if (symbol.IsVirtual)
            {
                yield return SyntaxFactory.Token(SyntaxKind.VirtualKeyword);
            }
            if (symbol.IsOverride)
            {
                yield return SyntaxFactory.Token(SyntaxKind.OverrideKeyword);
            }
            if (symbol.IsSealed)
            {
                yield return SyntaxFactory.Token(SyntaxKind.SealedKeyword);
            }
        }

        private static SyntaxToken? GetOperatorToken(IMethodSymbol symbol)
        {
            switch (symbol.Name)
            {
                // unary
                case "op_UnaryPlus": return SyntaxFactory.Token(SyntaxKind.PlusToken);
                case "op_UnaryNegation": return SyntaxFactory.Token(SyntaxKind.MinusToken);
                case "op_LogicalNot": return SyntaxFactory.Token(SyntaxKind.ExclamationToken);
                case "op_OnesComplement": return SyntaxFactory.Token(SyntaxKind.TildeToken);
                case "op_Increment": return SyntaxFactory.Token(SyntaxKind.PlusPlusToken);
                case "op_Decrement": return SyntaxFactory.Token(SyntaxKind.MinusMinusToken);
                case "op_True": return SyntaxFactory.Token(SyntaxKind.TrueKeyword);
                case "op_False": return SyntaxFactory.Token(SyntaxKind.FalseKeyword);
                // binary
                case "op_Addition": return SyntaxFactory.Token(SyntaxKind.PlusToken);
                case "op_Subtraction": return SyntaxFactory.Token(SyntaxKind.MinusToken);
                case "op_Multiply": return SyntaxFactory.Token(SyntaxKind.AsteriskToken);
                case "op_Division": return SyntaxFactory.Token(SyntaxKind.SlashToken);
                case "op_Modulus": return SyntaxFactory.Token(SyntaxKind.PercentToken);
                case "op_BitwiseAnd": return SyntaxFactory.Token(SyntaxKind.AmpersandToken);
                case "op_BitwiseOr": return SyntaxFactory.Token(SyntaxKind.BarToken);
                case "op_ExclusiveOr": return SyntaxFactory.Token(SyntaxKind.CaretToken);
                case "op_RightShift": return SyntaxFactory.Token(SyntaxKind.GreaterThanGreaterThanToken);
                case "op_LeftShift": return SyntaxFactory.Token(SyntaxKind.LessThanLessThanToken);
                // comparision
                case "op_Equality": return SyntaxFactory.Token(SyntaxKind.EqualsEqualsToken);
                case "op_Inequality": return SyntaxFactory.Token(SyntaxKind.ExclamationEqualsToken);
                case "op_GreaterThan": return SyntaxFactory.Token(SyntaxKind.GreaterThanToken);
                case "op_LessThan": return SyntaxFactory.Token(SyntaxKind.LessThanToken);
                case "op_GreaterThanOrEqual": return SyntaxFactory.Token(SyntaxKind.GreaterThanEqualsToken);
                case "op_LessThanOrEqual": return SyntaxFactory.Token(SyntaxKind.LessThanEqualsToken);
                // conversion
                case "op_Implicit": return SyntaxFactory.Token(SyntaxKind.ImplicitKeyword);
                case "op_Explicit": return SyntaxFactory.Token(SyntaxKind.ExplicitKeyword);
                // not supported:
                //case "op_Assign": return SyntaxFactory.Token(SyntaxKind.EqualsToken);
                default: return null;
            }
        }

        private static string RemoveBraces(string text)
        {
            return BracesRegex.Replace(text, string.Empty);
        }

        private static TypeSyntax GetTypeSyntax(ITypeSymbol type)
        {
            // todo : need to verify it when type.language is not c#
            return SyntaxFactory.ParseTypeName(type.ToDisplayString(ShortFormat));
        }

        #endregion
    }
}
