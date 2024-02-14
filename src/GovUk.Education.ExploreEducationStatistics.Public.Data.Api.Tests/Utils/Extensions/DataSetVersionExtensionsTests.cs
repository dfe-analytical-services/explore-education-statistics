using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Utils.Extensions;

public abstract class DataSetVersionExtensionsTests
{
    protected readonly DataFixture DataFixture = new();

    public class VersionTypeTests : DataSetVersionExtensionsTests
    {
        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, 0)]
        public void MajorVersion(int majorVersion, int minorVersion)
        {
            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(majorVersion, minorVersion);

            var versionType = dataSetVersion.VersionType();

            Assert.Equal(DataSetVersionType.Major, versionType);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 1)]
        [InlineData(2, 2)]
        public void MinorVersion(int majorVersion, int minorVersion)
        {
            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(majorVersion, minorVersion);

            var versionType = dataSetVersion.VersionType();

            Assert.Equal(DataSetVersionType.Minor, versionType);
        }
    }
}
