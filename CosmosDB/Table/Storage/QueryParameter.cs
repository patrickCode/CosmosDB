namespace Table.Data
{
    public class QueryParameter
    {
        public string Propery { get; }
        public string Operation { get; }
        public string Value { get; }

        public QueryParameter(string property, string operation, string value)
        {
            Propery = property;
            Operation = operation;
            Value = value;
        }
    }
}