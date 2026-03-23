using System.Collections.Generic;

namespace BLML.Phase4DataAccess.Models
{
    public class TableMetadata
    {
        public string Name { get; set; } = string.Empty;
        public List<ColumnMetadata> Columns { get; set; } = new();
        public List<string> PrimaryKeyColumns { get; set; } = new();
        public List<RelationshipMetadata> Relationships { get; set; } = new();
    }

    public class ColumnMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = "string";
        public bool IsNullable { get; set; } = true;
        public int? MaxLength { get; set; }
    }

    public class RelationshipMetadata
    {
        public string FromTable { get; set; } = string.Empty;
        public string ToTable { get; set; } = string.Empty;
        public string FromColumn { get; set; } = string.Empty;
        public string ToColumn { get; set; } = string.Empty;
        public string NavigationPropertyName { get; set; } = string.Empty;
    }
}
