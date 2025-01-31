#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public class CronExpressionUtilTests
{
    [Fact]
    public void CronExpressionHasSecondPrecision()
    {
        Assert.True(CronExpressionUtil.CronExpressionHasSecondPrecision("0 30 9 * * *"));
        Assert.False(CronExpressionUtil.CronExpressionHasSecondPrecision("30 9 * * *"));
    }
}
