using Capsaicin.UnitTesting;
using Capsaicin.UnitTesting.Generators.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization; // this namespace must be included because CultureInfo is used in an expression!

namespace Capsaicin.UnitTesting.Generators
{
    [TestClass]
    public partial class TestDataSourceGeneratorTest
    {
        private static DummyClass<double> CreateObject3() => new DummyClass<double>("Object3", Math.Pow(3, 4));

        private static IFormattable[] Objects = new[]
        {
            (IFormattable)new DummyClass<double>("Object4", Math.PI),
            new DummyClass<DummyClass<double>>("Object5", new DummyClass<double>("nested", 123.456)),
        };

        /// <summary>
        /// Implicitly tests TestDataSourceGenerator
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="testObject"></param>
        /// <param name="format"></param>
        /// <param name="formatProvider"></param>
        [TestMethod]
        [ExpressionDataRow("Object1: -123,456.00", "new DummyClass<int>(\"Object1\", -123456)", "N2", "null")]
        [ExpressionDataRow("Object2: 1.414214", "new DummyClass<double>(\"Object2\", Math.Sqrt(2))", "F6", null)]
        // expression may contain method call, which is usefull for complex objects:
        [ExpressionDataRow("Object3: 81", "CreateObject3()", null, "CultureInfo.InvariantCulture")]
        // expression may contain reference to static field or property:
        [ExpressionDataRow("Object4: 3,142", "Objects[0]", "F3", "CultureInfo.GetCultureInfo(\"de-DE\")")]
        [ExpressionDataRow("Object5: nested: 123.456", "Objects[1]", "", "CultureInfo.GetCultureInfo(\"en-GB\")")]
        public partial void GenerateTest(string expected, // not annotated with FromCSharpExpression because values are expressed with constant expressions
            [FromCSharpExpression] IFormattable testObject,
            string? format, // not annotated with FromCSharpExpression because values are expressed with constant expressions
            [FromCSharpExpression] IFormatProvider? formatProvider)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
            Assert.AreEqual(testObject.GetType().GetGenericTypeDefinition(), typeof(DummyClass<>), $"Invalid test data. {nameof(testObject)} is not typeof {typeof(DummyClass<>).FullName}.");
            var actual = testObject.ToString(format, formatProvider);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Implicitly tests TestDataSourceGenerator and verifies object values are correctly evaluated
        /// </summary>
        /// <param name="notFromExpression"></param>
        /// <param name="fromExpression"></param>
        [TestMethod]
        [ExpressionDataRow(1.23d, "1.23d", typeof(double))]
        [ExpressionDataRow(0d, "0d", typeof(double))]
        [ExpressionDataRow(-6d, "-6d", typeof(double))]
        [ExpressionDataRow(0, "0", typeof(int))]
        public partial void Generate_ObjectValues_Test(object notFromExpression, [FromCSharpExpression] object fromExpression, Type expectedType)
        {
            Assert.IsInstanceOfType(fromExpression, expectedType);
            Assert.IsInstanceOfType(notFromExpression, expectedType);
            Assert.AreEqual(notFromExpression, fromExpression);
        }
    }
}
