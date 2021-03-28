using Microsoft.VisualStudio.TestTools.UnitTesting;
using Capsaicin.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Capsaicin.UnitTesting
{
    [TestClass]
    public class ExpressionDataRowAttributeTests
    {
        [TestMethod]
        public void ConstructorTest()
        {
            object?[] parameters = null!;
            Assert.ThrowsException<ArgumentNullException>(() => new ExpressionDataRowAttribute(parameters));

            parameters = new object?[] { };
            var actual = new ExpressionDataRowAttribute(parameters);
            Assert.AreSame(parameters, actual.Parameters);

            parameters = new object?[] { new object() };
            actual = new ExpressionDataRowAttribute(parameters);
            Assert.AreSame(parameters, actual.Parameters);
        }
    }
}