namespace AngularCrudApi.Domain.Entities
{
    public class ColumnDefinition
    {
        public string Name { get; set; }
        public bool IsNullable { get; set; }

        public string DataType { get; set; }
        public int MaximumLength { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsPrimaryKey { get; set; }
    }
}