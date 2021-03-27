using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleTestProject
{
    public class DemoClass<TValue> : IFormattable
        where TValue : IFormattable
    {
        public DemoClass(string name, TValue value)
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
