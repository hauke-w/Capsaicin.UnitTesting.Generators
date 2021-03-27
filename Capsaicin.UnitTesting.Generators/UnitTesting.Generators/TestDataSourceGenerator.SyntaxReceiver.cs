using Capsaicin.CodeAnalysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Capsaicin.UnitTesting.Generators
{
    partial class TestDataSourceGenerator
    {
        private class SyntaxReceiver : ISyntaxContextReceiver
        {
            public List<(IMethodSymbol Symbol, MethodDeclarationSyntax Declaration)> TestMethods { get; } = new();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is MethodDeclarationSyntax methodDeclarationSyntax
                    && context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) is IMethodSymbol { MethodKind: MethodKind.Ordinary } methodSymbol
                    && methodSymbol.HasAttribute(TestMethodAttributeFullName)
                    && methodSymbol.HasAttribute(ExpressionDataRowAttributeFullName))
                {
                    TestMethods.Add((methodSymbol, methodDeclarationSyntax));
                }
            }
        }
    }
}
