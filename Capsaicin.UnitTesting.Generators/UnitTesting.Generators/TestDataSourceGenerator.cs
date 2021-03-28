using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Diagnostics;
using System;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.IO;

namespace Capsaicin.UnitTesting.Generators
{
    /// <summary>
    /// Generates an Attribute class implementing
    /// <a href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.testtools.unittesting.itestdatasource">Microsoft.VisualStudio.TestTools.UnitTesting.ITestDataSource</a>
    /// for each occurance of ExpressionDataRowAttribute and annotates the source method with this attribute.
    /// </summary>
    [Generator]
    public partial class TestDataSourceGenerator : ISourceGenerator
    {
        private const string DiagnosticIdPrefix = "CUT";
        private const string MessageCategory = "Capsaicin.UnitTesting.Generators";

        private const string TestMethodAttributeFullName = "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute";
        private const string ExpressionDataRowAttributeFullName = "Capsaicin.UnitTesting.ExpressionDataRowAttribute";
        private const string FromCSharpExpressionAttributeFullName = "Capsaicin.UnitTesting.FromCSharpExpressionAttribute";

        #region Error definitions
        private static DiagnosticDescriptor ErrorMethodIsNotPartial => new DiagnosticDescriptor(
    DiagnosticIdPrefix + "001",
    "Method is not partial",
    "The method '{0}' is not partial.",
    MessageCategory,
    DiagnosticSeverity.Error,
    true);

        private static DiagnosticDescriptor ErrorParameterCountMismatch => new DiagnosticDescriptor(
            DiagnosticIdPrefix + "002",
            "Parameter number mismatch",
            "The number of attribute parameters does match the number of method parameters.",
            MessageCategory,
            DiagnosticSeverity.Error,
            true);

        private static DiagnosticDescriptor ErrorExpressionNotString => new DiagnosticDescriptor(
            DiagnosticIdPrefix + "003",
            "Expression parameter value is not type of string",
            $"Parameter '{{0}}' is annotated with FromCSharpExpressionAttribute but the specified value is not type of string.",
            MessageCategory,
            DiagnosticSeverity.Error,
            true);
        #endregion

        /// <inheritdoc/>
        public void Initialize(GeneratorInitializationContext context)
        {
            //Debugger.Launch(); // enable this line for debugging

            context.RegisterForPostInitialization(GenerateAttributeClasses);
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        /// <inheritdoc/>
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is SyntaxReceiver syntaxReceiver)
            {
                var executeContext = new ExecuteContext(context, syntaxReceiver);
                executeContext.Generate();
            }
        }

        private static void GenerateAttributeClasses(GeneratorPostInitializationContext context)
        {
            GenerateSourceFromResource("ExpressionDataRowAttribute.cs", context);
            GenerateSourceFromResource("FromCSharpExpressionAttribute.cs", context);
        }

        private static void GenerateSourceFromResource(string resourceName, GeneratorPostInitializationContext context)
        {
            using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Capsaicin." + resourceName);
            using var resourceStreamReader = new StreamReader(resourceStream);
            var sourceCode = resourceStreamReader.ReadToEnd();
            context.AddSource(resourceName, SourceText.From(sourceCode, Encoding.UTF8));
        }
    }
}
