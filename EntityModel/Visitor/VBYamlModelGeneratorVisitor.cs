﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DocAsCode.Utility;

namespace DocAsCode.EntityModel
{
    public class VBYamlModelGeneratorVisitor : YamlModelGeneratorVisitor
    {
        public VBYamlModelGeneratorVisitor(object context) : base(context, SyntaxLanguage.VB)
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
                        var syntax = syntaxNode as ClassStatementSyntax;
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
    }
}
