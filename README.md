# Capsaicin.UnitTesting.Generators
Provides a C# source generator that generates Attribute classes implementing ITestDataSource. The source generator enables test developers to specify test data using c# expressions with ExpressionDataRows. This overcomes the limitation of C#-Attributes allowing only constant values.

## Usage
First, add a reference to the 'Capsaicin.UnitTesting.Generators' nuget package to your test project.

Then add *ExpressionDataRow* attribute for each test case to your test method. This is simalar to adding a *DataRow* attribute.
However, with *ExpressionDataRow* you specify any parameter as c# expression encoded in a string. Each C# expression parameter must be annotated with *FromCSharpExpression* attribute. Parameters not annotated with *FromCSharpExpression* attribute behave like with *DataRow* attribute.

An *ITestDataSource* implementation will be generated for each *ExpressionDataRow*.
```
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Capsaicin.UnitTesting;
using System.Globalization;

[TestClass]
public class CalculatorTest
{
    [TestMethod]
    [ExpressionDataRow("4", "new Calculator(culture: \"en-GB\"))", 2)]
    [ExpressionDataRow("1,44", "new Calculator(\"de-DE\")", 1.2)]
    [ExpressionDataRow("6.25", "new Calculator(\"en-US\")", 2.5)]
    public void SquareTest(int expected, [FromCSharpExpression]Calculator testObject, double number)
    {
        string actual = testObject.Square(number);
        Assert.AreEqual(expected, actual);
    }
}
```

## Examples
See the ExampleTestProject for a complete example.
