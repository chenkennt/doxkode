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
        private readonly SymbolDisplayFormat ShortFormat = new SymbolDisplayFormat(
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
                        IEnumerable<INamedTypeSymbol> baseTypeList;
                        if (symbol.BaseType.GetDocumentationCommentId() == "T:System.Object")
                        {
                            baseTypeList = symbol.AllInterfaces;
                        }
                        else
                        {
                            baseTypeList = new[] { symbol.BaseType }.Concat(symbol.AllInterfaces);
                        }
                        var baseList = baseTypeList.Any() ? SyntaxFactory.BaseList().AddTypes(
                            (from t in baseTypeList
                             select SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(t.ToDisplayString(ShortDisplayFormat)))).ToArray()) : null;
                        syntaxStr
                            = syntax
                            .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                            .WithBaseList(baseList)
                            .WithMembers(new SyntaxList<MemberDeclarationSyntax>())
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
                        syntaxStr
                            = syntax
                                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                                .WithBaseList(null)
                                .WithMembers(new SeparatedSyntaxList<EnumMemberDeclarationSyntax>())
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
                        var baseList = symbol.AllInterfaces.Any() ? SyntaxFactory.BaseList().AddTypes(
                            (from t in symbol.AllInterfaces
                             select SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(t.ToDisplayString(ShortDisplayFormat)))).ToArray()) : null;

                        syntaxStr =
                            syntax
                                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                                .WithBaseList(baseList)
                                .WithMembers(new SyntaxList<MemberDeclarationSyntax>())
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
                        syntaxStr = syntax
                                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                                .WithBaseList(null)
                                .WithMembers(new SyntaxList<MemberDeclarationSyntax>())
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
                        syntaxStr = syntax
                                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                                .NormalizeWhitespace()
                                .ToString().Trim();
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
    }
}
