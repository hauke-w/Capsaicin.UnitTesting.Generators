using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Capsaicin.Generators
{
    internal class InvalidSourceException : Exception
    {
        internal InvalidSourceException(DiagnosticDescriptor diagnosticDescriptor, Location location, params object?[] messageArgs) : base(string.Format(diagnosticDescriptor.MessageFormat.ToString(), messageArgs))
        {
            MessageArgs = messageArgs;
            DiagnosticDescriptor = diagnosticDescriptor;
            Location = location;
        }

        public object?[] MessageArgs { get; }

        public DiagnosticDescriptor DiagnosticDescriptor { get; }
        public Location Location { get; }

        public void ReportDiagnostic(GeneratorExecutionContext generatorExecutionContext)
        {
            var diagnostic = Diagnostic.Create(DiagnosticDescriptor, Location, MessageArgs);
            generatorExecutionContext.ReportDiagnostic(diagnostic);
        }
    }
}
