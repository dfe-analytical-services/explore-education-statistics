using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Semver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.UnitTests;

public class WildCardDatasetVersionTests
{
    [Fact]
    public void TestDataSetVersions()
    {
        
    //    Api.Services.DataSetService dataSetService = new Api.Services.DataSetService(
    //        new PublicDataDbContext(),
    //        new MockDataSetVersionPathResolver(),
    //        userService
            
    //        );
    //    DataSetVersionQueryableExtensions.FindByVersion();
    //    var versionString = "1.1.*";
    //    VersionUtils.TryParse(versionString, out var version);

    //    //Assert.True();


    //    // Arrange
    //    var datasetId = Guid.NewGuid();
    //    var release = new Release()
    //    {
    //        DataSetFileId = Guid.NewGuid(),
    //        ReleaseFileId = Guid.NewGuid(),
    //        Slug = "test",
    //        Title = "Test"
    //    };
    //    //TODO: Mock dataset and then use this list as an attribute. Then test the service with this provided dataset with different versions.
    //    var versions = new List<DataSetVersion>
    //    {
    //        new() { 
    //            DataSetId = datasetId , Notes = "hi", Release = release, Status = DataSetVersionStatus.Published,
    //            VersionMajor = 1, VersionMinor = 0 
    //        },
    //        new() { 
    //            DataSetId = datasetId, Notes = "hi", Release = release, Status = DataSetVersionStatus.Published,   VersionMajor = 1, VersionMinor = 1 
    //        },
    //        new() { 
    //            DataSetId = datasetId, Notes = "hi", Release = release, Status = DataSetVersionStatus.Published,   VersionMajor = 2, VersionMinor = 0 
    //        },
    //        new() { 
    //            DataSetId = datasetId, Notes = "hi", Release = release, Status = DataSetVersionStatus.Published,   VersionMajor = 2, VersionMinor = 1 
    //        },
    //        new() { 
    //            DataSetId = datasetId, Notes = "hi", Release = release, Status = DataSetVersionStatus.Published,   VersionMajor = 1, VersionMinor = 3
    //        }
    //    };

    //    // Act
    //    var sortedVersions = versions.OrderBy(v => v.VersionMajor).ThenBy(v => v.VersionMinor).ToList();

    //    // Assert
    //    Assert.Equal(1, sortedVersions[0].VersionMajor);
    //    Assert.Equal(0, sortedVersions[0].VersionMinor);
    //    Assert.Equal(1, sortedVersions[1].VersionMajor);
    //    Assert.Equal(1, sortedVersions[1].VersionMinor);
    //    Assert.Equal(2, sortedVersions[2].VersionMajor);
    //    Assert.Equal(0, sortedVersions[2].VersionMinor);
    //    Assert.Equal(2, sortedVersions[3].VersionMajor);
    //    Assert.Equal(1, sortedVersions[3].VersionMinor);
    //    Assert.Equal(3, sortedVersions[4].VersionMajor);
    //    Assert.Equal(0, sortedVersions[4].VersionMinor);
    }
    [Theory]
    [InlineData("1.*.*)", 1, 0, 0)]
    [InlineData("v2.0", 2, 0)]
    [InlineData("1", 1)]
    [InlineData("v2", 2)]
    [InlineData("v2.0.0", 2, 0, 0)]
    [InlineData("v1.*.*)", 1, 0, 0)]
    [InlineData("1.*)", 1, 0, 0)]
    [InlineData("v1.*)", 1, 0, 0)]
    [InlineData("1.3.*)", 1, 0, 0)]
    [InlineData("v1.3.*)", 1, 0, 0)]
    public void ValidVersion_SuccessfullyParsed(string versionString,
            int expectedMajor,
            int expectedMinor = default,
            int expectedPatch = default)
    {
        //Assert.True(VersionUtils.TryParse(versionString, out var version));
        //Assert.Equal(expectedMajor, version.Major);
        //Assert.Equal(expectedMinor, version.Minor);
        //Assert.Equal(expectedPatch, version.Patch);
    }
}
