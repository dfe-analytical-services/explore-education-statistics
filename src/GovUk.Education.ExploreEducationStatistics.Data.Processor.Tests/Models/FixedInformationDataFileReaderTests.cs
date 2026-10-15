using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Models;

public class FixedInformationDataFileReaderTests
{
    [Fact]
    public void GetLocation_School()
    {
        var reader = new FixedInformationDataFileReader([
            "geographic_level",
            "country_code",
            "country_name",
            "school_urn",
            "school_laestab",
            "school_name",
        ]);

        var location = reader.GetLocation([
            "School",
            "E92000001",
            "England",
            "100001",
            "2011001",
            "Westminster School",
        ]);

        Assert.Equal(GeographicLevel.School, location.GeographicLevel);
        Assert.NotNull(location.School);
        Assert.Equal("100001", location.School.Code);
        Assert.Equal("2011001", location.School.LaEstab);
        Assert.Equal("Westminster School", location.School.Name);
    }

    [Fact]
    public void GetLocation_School_NoLaEstabColumn()
    {
        var reader = new FixedInformationDataFileReader([
            "geographic_level",
            "country_code",
            "country_name",
            "school_urn",
            "school_name",
        ]);

        var location = reader.GetLocation(["School", "E92000001", "England", "100001", "Westminster School"]);

        Assert.NotNull(location.School);
        Assert.Equal("100001", location.School.Code);
        Assert.Null(location.School.LaEstab);
        Assert.Equal("Westminster School", location.School.Name);
    }
}
