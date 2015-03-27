using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DocAsCode.Utility;

namespace DocAsCode.EntityModel
{
    using Microsoft.CodeAnalysis.CSharp;

    public class CSYamlModelGeneratorVisitor : YamlModelGeneratorVisitor
    {
        public CSYamlModelGeneratorVisitor(object context) : base(context, SyntaxLanguage.CSharp)
        {
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
                        syntaxStr
                            = syntax
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
                        syntaxStr =
                            syntax
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
                            syntaxStr = syntax
                            .WithInitializer(null)
                            .NormalizeWhitespace()
                            .ToString()
                            .Trim();
                        }

                        break;
                    };
                case MemberType.Event:
                    {
                        break;
                    };
                case MemberType.Property:
                    {
                        Debug.Assert(syntaxNode is PropertyDeclarationSyntax || syntaxNode is IndexerDeclarationSyntax);

                        var syntax = syntaxNode as PropertyDeclarationSyntax;
                        if (syntax != null)
                        {
                            var accessorList = syntax.AccessorList.Accessors;
                            var simplifiedAccessorList = accessorList.Select(s => s.WithBody(null));
                            SyntaxList<AccessorDeclarationSyntax> syntaxList = new SyntaxList<AccessorDeclarationSyntax>();
                            syntaxList.AddRange(simplifiedAccessorList);
                            var simplifiedSyntax = syntax.WithAccessorList(SyntaxFactory.AccessorList(syntaxList));
                            syntaxStr = simplifiedSyntax.ToString().Trim();
                        }
                        else
                        {
                            var syntaxIndexer = syntaxNode as IndexerDeclarationSyntax;
                            if (syntaxIndexer != null)
                            {
                                var accessorList = syntaxIndexer.AccessorList.Accessors;
                                var simplifiedAccessorList = accessorList.Select(s => s.WithBody(null));
                                SyntaxList<AccessorDeclarationSyntax> syntaxList = new SyntaxList<AccessorDeclarationSyntax>();
                                syntaxList.AddRange(simplifiedAccessorList);
                                var simplifiedSyntax = syntaxIndexer.WithAccessorList(SyntaxFactory.AccessorList(syntaxList));
                                syntaxStr = simplifiedSyntax.ToString().Trim();
                            }
                        }

                        break;
                    };
            }

            if (string.IsNullOrEmpty(syntaxStr)) syntaxStr = syntaxNode.ToString().Trim();
            return syntaxStr;
        }
    }
}
