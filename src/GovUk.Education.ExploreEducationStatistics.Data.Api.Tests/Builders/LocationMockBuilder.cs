using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Builders;

public static class LocationMockBuilder
{
    public static Location Build()
    {
        return new Location
        {
            Id = Guid.NewGuid(),
            Country = new("E92000001", "England"),
            GeographicLevel = GeographicLevel.Country,
        };
    }
}
