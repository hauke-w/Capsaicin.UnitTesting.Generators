using System;
using System.Collections.Generic;
using System.Text;

namespace Capsaicin.UnitTesting
{
    /// <summary>
    /// Denotes a test data row that might include parameters that are evaluated using an expression.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class ExpressionDataRowAttribute : Attribute
    {
        /// <summary>
        /// Creates test data definition using c# expressions.
        /// </summary>
        /// <remarks>
        /// An attribute implementing <a href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.testtools.unittesting.itestdatasource">Microsoft.VisualStudio.TestTools.UnitTesting.ITestDataSource</a>
        /// will be generated for this attribute and added to the method 
        /// so that the methods parameters will be populated using the <paramref name="parameters"/> when the test is executed.
        /// </remarks>
        /// <param name="parameters">The test data parameters.</param>
        public ExpressionDataRowAttribute(params object?[] parameters)
        {
            Parameters = parameters;
        }

        /// <summary>
        /// The test data parameters.
        /// </summary>
        public object?[] Parameters { get; }
    }
}
