using Capsaicin.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization; // this namespace must be included because CultureInfo is used in an expression!

namespace ExampleTestProject
{
    [TestClass]
    public partial class ExampleUnitTest
    {
        private static DemoClass<double> CreateObject3() => new DemoClass<double>("Object3", Math.Pow(3, 4));

        private static IFormattable[] Objects = new[]
        {
            (IFormattable)new DemoClass<double>("Object4", Math.PI),
            new DemoClass<DemoClass<double>>("Object5", new DemoClass<double>("nested", 123.456)),
        };

        [TestMethod]
        [ExpressionDataRow("Object1: -123,456.00", "new DemoClass<int>(\"Object1\", -123456)", "N2", "null")] // "null" and ...
        [ExpressionDataRow("Object2: 1.414214", "new DemoClass<double>(\"Object2\", Math.Sqrt(2))", "F6", null)] // ... null have no difference for FromCSharpExpression parameters
        // expression may contain method call, which is usefull for complex objects:
        [ExpressionDataRow("Object3: 81", "CreateObject3()", null, "CultureInfo.InvariantCulture")]
        // expression may contain reference to static field or property:
        [ExpressionDataRow("Object4: 3,142", "Objects[0]", "F3", "CultureInfo.GetCultureInfo(\"de-DE\")")]
        [ExpressionDataRow("Object5: nested: 123.456", "Objects[1]", "", "CultureInfo.GetCultureInfo(\"en-GB\")")]
        public partial void ToStringTest(string expected, // not annotated with FromCSharpExpression because values are expressed with constant expressions
            [FromCSharpExpression] IFormattable testObject,
            string? format, // not annotated with FromCSharpExpression because values are expressed with constant expressions
            [FromCSharpExpression] IFormatProvider? formatProvider)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
            Assert.AreEqual(testObject.GetType().GetGenericTypeDefinition(), typeof(DemoClass<>), $"Invalid test data. {nameof(testObject)} is not typeof {typeof(DemoClass<>).FullName}.");
            var actual = testObject.ToString(format, formatProvider);
            Assert.AreEqual(expected, actual);
        }
    }
}
