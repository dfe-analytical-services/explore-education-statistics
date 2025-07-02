namespace GovUk.Education.ExploreEducationStatistics.Common.Database;

public interface ISchemaNameProvider
{
    string SchemaName { get; }
}

public class SchemaNameProvider(string schemaName) : ISchemaNameProvider
{
    public string SchemaName { get; } = schemaName;
}
