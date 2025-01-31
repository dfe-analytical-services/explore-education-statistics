#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class CronExpressionUtil
{
    public static bool CronExpressionHasSecondPrecision(string cronExpression)
    {
        return cronExpression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length == 6;
    }
}
