using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Capsaicin.CodeAnalysis.Extensions;
using Capsaicin.CodeAnalysis.Generators;

namespace Capsaicin.UnitTesting.Generators
{
    partial class TestDataSourceGenerator
    {
        private class ExecuteContext : ExecuteContextBase
        {
            public ExecuteContext(GeneratorExecutionContext generatorExecutionContext, SyntaxReceiver syntaxReceiver)
                : base(generatorExecutionContext)
            {
                SyntaxReceiver = syntaxReceiver;
            }

            private readonly SyntaxReceiver SyntaxReceiver;
            private const int IndentionStep = 4;

            internal void Generate()
            {
                var generatedHints = new HashSet<string>();
                foreach (var (methodSymbol, methodDeclaration) in SyntaxReceiver.TestMethods)
                {
                    if (!methodDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                    {
                        ReportDiagnostic(ErrorMethodIsNotPartial, methodSymbol);
                    }
                    else
                    {
                        var source = GeneratePartialClass(methodSymbol, methodDeclaration);
                        var hintName = GetHintName(methodSymbol);
                        GeneratorExecutionContext.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
                    }
                }

                string GetHintName(IMethodSymbol methodSymbol)
                {
                    var hintBuilder = new StringBuilder();
                    foreach (var t in methodSymbol.GetContainingTypes())
                    {
                        hintBuilder.Append(t.Name);
                        hintBuilder.Append('.');
                    }
                    hintBuilder.Append(methodSymbol.Name);
                    hintBuilder.Append("_TestDataSources");
                    var hintNameBase = hintBuilder.ToString();
                    var hintName = hintNameBase;
                    int counter = 1;
                    while (generatedHints.Contains(hintName))
                    {
                        hintName = hintNameBase + (++counter);
                    }
                    generatedHints.Add(hintName);
                    return hintName;
                }
            }

            private string GeneratePartialClass(IMethodSymbol methodSymbol, MethodDeclarationSyntax methodDeclaration)
            {
                var usings = methodDeclaration.GetUsingsFromContainingCompilationUnit();
                usings.Add("using System;");
                usings.Add("using System.Reflection;");
                usings.Add("using Microsoft.VisualStudio.TestTools.UnitTesting;");
                var sortedUsings = usings.ToList();
                sortedUsings.Sort();

                var sourceBuilder = new StringBuilder();
                foreach (var usingLine in sortedUsings)
                {
                    sourceBuilder.AppendLine(usingLine);
                }

                sourceBuilder.AppendLine($@"
namespace {methodSymbol.ContainingNamespace.ToDisplayString()}
{{");
                GenerateContainingPartialTypeRecursive(sourceBuilder, methodSymbol, GenerateClassContent, 1);
                sourceBuilder.Append($@"
}}");
                return sourceBuilder.ToString();

                void GenerateClassContent(StringBuilder stringBuilder, int indentionLevel)
                {
                    var dataSourceAttributes = methodSymbol.GetAttributes()
                        .Where(a => a.EqualsAttributeClass(ExpressionDataRowAttributeFullName))
                        .Select((a, i) => new { DataSourceAttribute = a, Index = i, Name = string.Concat(methodSymbol.Name, "DataRow", i) })
                        .ToList();

                    var indention = new string(' ', indentionLevel * IndentionStep);
                    foreach (var item in dataSourceAttributes)
                    {
                        stringBuilder.Append(indention);
                        stringBuilder.Append("[");
                        stringBuilder.Append(item.Name);
                        stringBuilder.AppendLine("]");
                    }

                    stringBuilder.Append(indention);
                    GeneratePartialMethodSignature(methodDeclaration, stringBuilder);

                    foreach (var item in dataSourceAttributes)
                    {
                        GenerateTestDataSource(stringBuilder, item.Name, item.DataSourceAttribute, item.Index, methodSymbol);
                    }
                }
            }

            private static void GeneratePartialMethodSignature(MethodDeclarationSyntax methodDeclaration, StringBuilder stringBuilder)
            {
                foreach (var modifier in methodDeclaration.Modifiers)
                {
                    stringBuilder.Append(modifier);
                    stringBuilder.Append(" ");
                }

                stringBuilder.Append(methodDeclaration.ReturnType);
                stringBuilder.Append(" ");
                stringBuilder.Append(methodDeclaration.Identifier);
                stringBuilder.Append('(');
                var firstParameter = true;
                foreach (var parameter in methodDeclaration.ParameterList.Parameters)
                {
                    if (firstParameter)
                    {
                        firstParameter = false;
                    }
                    else
                    {
                        stringBuilder.Append(", ");
                    }
                    stringBuilder.Append(parameter.Type);
                    stringBuilder.Append(' ');
                    stringBuilder.Append(parameter.Identifier);
                }
                stringBuilder.Append(')');
                stringBuilder.AppendLine(";");
            }

            private void GenerateTestDataSource(StringBuilder stringBuilder, string name, AttributeData dataSourceAttribute, int index, IMethodSymbol methodSymbol)
            {
                var parametersData = dataSourceAttribute.ConstructorArguments[0].Values;
                var methodParameters = methodSymbol.Parameters;

                var matchesParameterListLength = parametersData.Length == methodParameters.Length;
                if (!matchesParameterListLength)
                {
                    var location = dataSourceAttribute.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                        ?? methodSymbol.Locations[0];
                    ReportDiagnostic(ErrorParameterCountMismatch, location);
                }

                stringBuilder.Append($@"
        [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
        private sealed class {name}Attribute : Attribute, ITestDataSource
        {{
            public IEnumerable<object?[]> GetData(MethodInfo methodInfo)
            {{
                return new object?[][]
                {{
                    new object?[]
                    {{");

                for (int i = 0; i < parametersData.Length; i++)
                {
                    //stringBuilder.AppendLine($"// type: {parametersData[i].Type?.Name ?? "<unkown>"}; ToString={parametersData[i].ToString()}");
                    var valueExpression = GetValueExpression(i);
                    stringBuilder.Append($@"
                        {valueExpression},");
                }
                stringBuilder.AppendLine($@"
                    }}
                }};
            }}

            public string GetDisplayName(MethodInfo methodInfo, object?[] data)
            {{
                return ""Test case #{index + 1}"";
            }}
        }}");

                string GetValueExpression(int index)
                {
                    var methodParameter = methodParameters.Length > index ? methodParameters[index] : null;
                    var parameterData = parametersData[index];

                    if (methodParameter is not null && methodParameter.HasAttribute(FromCSharpExpressionAttributeFullName))
                    {
                        if (!parameterData.IsNull && parameterData.Type is { SpecialType: not SpecialType.System_String })
                        {
                            var location = dataSourceAttribute.ApplicationSyntaxReference!.GetSyntax() switch
                            {
                                AttributeSyntax a when a.ArgumentList?.Arguments.Count > index => a.ArgumentList.Arguments[index].GetLocation(),
                                SyntaxNode s => s.GetLocation(),
                                _ => methodSymbol.Locations[0]
                            };

                            ReportDiagnostic(ErrorExpressionNotString, location, methodParameter.Name);
                        }
                        else
                        {
                            return (parameterData.Value?.ToString() ?? "null");
                        }
                    }

                    var value = parameterData.ToCSharpString();
                    if (parameterData.Type is not null)
                    {
                        // explicit conversion required because some constants may not express the correctly typed value (e.g. 0 is integer but a double may be the correct type: (double)0)
                        // TODO: can we optimize this so that unnecessary conversion is not generated?
                        value = $"({parameterData.Type.ToDisplayString()}){value}";
                    }
                    return value;
                }
            }

            private delegate void ContentGenerator(StringBuilder sourceBuilder, int indentionLevel);
            private static void GenerateContainingPartialTypeRecursive(StringBuilder sourceBuilder, ISymbol symbol, ContentGenerator contentGenerator, int rootIndentionLevel)
            {
                var containingType = symbol.ContainingType;
                if (containingType is null)
                {
                    contentGenerator(sourceBuilder, rootIndentionLevel);
                }
                else
                {
                    GenerateContainingPartialTypeRecursive(sourceBuilder, containingType, GeneratePartialType, rootIndentionLevel);
                }

                void GeneratePartialType(StringBuilder sourceBuilder, int indentionLevel)
                {
                    var padding = new string(' ', rootIndentionLevel * IndentionStep);

                    var typeKind = containingType.TypeKind switch
                    {
                        TypeKind.Struct => "struct",
                        TypeKind.Interface => "interface",
                        TypeKind.Class when containingType.IsRecord => "record",
                        TypeKind.Class => "class",
                        _ => throw new NotSupportedException($"Cannot generate partial type hierarchy, type kind '{containingType.TypeKind}' is not supported.")
                    };

                    sourceBuilder.Append(padding);
                    sourceBuilder.Append("partial ");
                    sourceBuilder.Append(typeKind);
                    sourceBuilder.Append(" ");
                    sourceBuilder.AppendLine(containingType.Name);

                    sourceBuilder.Append(padding);
                    sourceBuilder.AppendLine("{");
                    contentGenerator(sourceBuilder, indentionLevel + 1);

                    sourceBuilder.Append(padding);
                    sourceBuilder.AppendLine("}");
                }
            }
        }
    }
}
