using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests;

public abstract class DataSetVersionTests
{
    protected readonly DataFixture DataFixture = new();

    public class VersionTypeTests : DataSetVersionTests
    {
        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, 0)]
        public void MajorVersion(int majorVersion, int minorVersion)
        {
            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(majorVersion, minorVersion);

            var versionType = dataSetVersion.VersionType;

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

            var versionType = dataSetVersion.VersionType;

            Assert.Equal(DataSetVersionType.Minor, versionType);
        }
    }

    public class VersionTests : DataSetVersionTests
    {
        [Theory]
        [InlineData(1, 0, "1.0")]
        [InlineData(1, 1, "1.1")]
        [InlineData(2, 0, "2.0")]
        [InlineData(2, 1, "2.1")]
        public void FormatsCorrectly(int majorVersion, int minorVersion, string formattedVersion)
        {
            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(majorVersion, minorVersion);

            Assert.Equal(formattedVersion, dataSetVersion.Version);
        }
    }
}
