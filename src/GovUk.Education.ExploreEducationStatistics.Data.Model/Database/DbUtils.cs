#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using Microsoft.Data.SqlClient;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

public static class DbUtils
{
    public static SqlParameter CreateIdListType(string parameterName, IEnumerable<Guid> values)
    {
        return CreateListType(parameterName, values.AsIdListTable(), "dbo.IdListGuidType");
    }

    private static SqlParameter CreateListType(string parameterName, object value, string typeName)
    {
        return new SqlParameter(parameterName, value)
        {
            SqlDbType = SqlDbType.Structured,
            TypeName = typeName
        };
    }
}
