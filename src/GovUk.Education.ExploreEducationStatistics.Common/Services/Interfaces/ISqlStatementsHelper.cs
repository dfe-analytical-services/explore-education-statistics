namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface ISqlStatementsHelper
{
    string CreateRandomIndexName(string tableName, string columnName);

    string CreateRandomIndexName(string tableName, string[] columnNames);
}
