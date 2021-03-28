using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capsaicin.UnitTesting.Generators.Tests
{
    public class DummyClass<TValue> : IFormattable
        where TValue : IFormattable
    {
        public DummyClass(string name, TValue value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value;
        }

        public string Name { get; }
        public TValue Value { get; }

        public override string ToString() => $"{Name}: {Value}";

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return $"{Name}: {Value.ToString(format, formatProvider)}";
        }
    }
}
