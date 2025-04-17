using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class RandomUtils
{
    public static string RandomString()
    {
        var guidStr = Guid.NewGuid().ToString();
        return guidStr[^5..];
    }
}
