using System.Text.RegularExpressions;

namespace analog
{
    public class DatabaseColumn : IEquatable<DatabaseColumn>
    {
        private readonly Func<string[], string> _valueGetter;

        public DatabaseColumn(
            int index,
            string name,
            string dataType,
            IEnumerable<LogColumn> sources,
            Func<string[], string> valueGetter)
        {
            Index = index;
            Name = name;
            Sources = sources;

            DataType = dataType;

            ParameterName = Regex.Replace(name, "[^a-z0-9]+", string.Empty).ToLowerInvariant();

            _valueGetter = valueGetter;
        }

        public int Index { get; }

        public string Name { get; }

        public IEnumerable<LogColumn> Sources { get; }

        public string DataType { get; }

        public string ParameterName { get; }

        public string GetValue(string[] rowValues) =>
            _valueGetter(rowValues);

        public DatabaseColumn With(int index) =>
            new(index, Name, DataType, Sources, _valueGetter);

        public bool Equals(DatabaseColumn? other) =>
            other is not null && (ReferenceEquals(this, other) || other.Name == Name);

        public override bool Equals(object? o) =>
            o is not null && o is DatabaseColumn c && Equals(c);

        public override int GetHashCode() =>
            Name.GetHashCode();
    }
}
