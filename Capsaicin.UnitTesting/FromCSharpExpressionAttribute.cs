using System;

namespace Capsaicin.UnitTesting
{
    /// <summary>
    /// Denotes c# expression parameters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class FromCSharpExpressionAttribute : Attribute
    {
    }
}
