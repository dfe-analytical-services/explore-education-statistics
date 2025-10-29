using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class SqlStatementsHelper : ISqlStatementsHelper
{
    public string CreateRandomIndexName(string tableName, string columnName)
    {
        return CreateRandomIndexName(tableName, [columnName]);
    }

    public string CreateRandomIndexName(string tableName, string[] columnNames)
    {
        return $"IX_{tableName}_{columnNames.JoinToString('_')}_{Guid.NewGuid():N}";
    }
}
