using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Moq;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Tests;

public abstract class DataSetVersionPathResolverTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock = new();

    public class ConstructorTests : DataSetVersionPathResolverTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  ")]
        public void EmptyBasePath_Throws(string basePath)
        {
            Assert.Throws<ArgumentException>(() =>
                BuildService(options: new DataFilesOptions
                {
                    BasePath = basePath
                })
            );
        }
    }

    public class PathTests : DataSetVersionPathResolverTests
    {
        private static readonly string[] EnvironmentNames =
        [
            Environments.Development,
            HostEnvironmentExtensions.IntegrationTestEnvironment,
            Environments.Production
        ];
        
        public static readonly TheoryData<string> GetEnvironmentNames = new(EnvironmentNames);
        
        public static readonly TheoryData<(string, DataSetVersionStatus)> GetEnvironmentNamesAndPublicStatuses = 
            new(EnvironmentNames.Cartesian(DataSetVersionAuthExtensions.PublicStatuses));

        public static readonly TheoryData<(string, DataSetVersionStatus)> GetEnvironmentNamesAndPrivateStatuses = 
            new(EnvironmentNames.Cartesian(DataSetVersionAuthExtensions.PrivateStatuses));

        [Fact]
        public void DevelopmentEnv_ValidBasePath()
        {
            _webHostEnvironmentMock
                .SetupGet(s => s.EnvironmentName)
                .Returns(Environments.Development);

            var resolver = BuildService(options: new DataFilesOptions
            {
                BasePath = Path.Combine("data", "data-files")
            });

            Assert.Equal(
                Path.Combine(
                    PathUtils.ProjectRootPath,
                    "data",
                    "data-files"
                ),
                resolver.BasePath());
        }

        [Fact]
        public void IntegrationTestEnv_ValidBasePath()
        {
            _webHostEnvironmentMock
                .SetupGet(s => s.EnvironmentName)
                .Returns(HostEnvironmentExtensions.IntegrationTestEnvironment);

            var resolver = BuildService(options: new DataFilesOptions
            {
                BasePath = Path.Combine("data", "data-files")
            });

            var basePath = resolver.BasePath();

            // Expect the last path segment to be a random test instance directory
            var expectedLastPathSegment = basePath[(basePath.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
            Assert.NotEmpty(expectedLastPathSegment);
            Assert.True(Guid.TryParse(expectedLastPathSegment, out var randomTestInstanceDir));

            Assert.Equal(
                Path.Combine(
                    Path.GetTempPath(),
                    "ExploreEducationStatistics",
                    "data",
                    "data-files",
                    randomTestInstanceDir.ToString()
                ),
                basePath
            );
        }

        [Fact]
        public void ProductionEnv_ValidBasePath()
        {
            _webHostEnvironmentMock
                .SetupGet(s => s.EnvironmentName)
                .Returns(Environments.Production);

            var resolver = BuildService(options: new DataFilesOptions
            {
                BasePath = Path.Combine("data", "data-files")
            });

            Assert.Equal(
                Path.Combine("data", "data-files"),
                resolver.BasePath());
        }

        [Theory]
        [MemberData(nameof(GetEnvironmentNamesAndPublicStatuses))]
        public void ValidDirectoryPath_PublicVersion((string, DataSetVersionStatus) environmentNameAndStatus)
        {
            var (environmentName, status) = environmentNameAndStatus;
            
            DataSetVersion version = _dataFixture
                .DefaultDataSetVersion()
                .WithStatus(status);

            _webHostEnvironmentMock
                .SetupGet(s => s.EnvironmentName)
                .Returns(environmentName);

            var resolver = BuildService(options: new DataFilesOptions
            {
                BasePath = Path.Combine("data", "data-files")
            });

            Assert.Equal(
                Path.Combine(
                    resolver.DataSetsPath(),
                    version.DataSetId.ToString(),
                    "v1.0.0"
                ),
                resolver.DirectoryPath(version));
        }

        [Theory]
        [MemberData(nameof(GetEnvironmentNamesAndPrivateStatuses))]
        public void ValidDirectoryPath_PrivateVersion((string, DataSetVersionStatus) environmentNameAndStatus)
        {
            var (environmentName, status) = environmentNameAndStatus;
            
            DataSetVersion version = _dataFixture
                .DefaultDataSetVersion()
                .WithStatus(status);
            
            _webHostEnvironmentMock
                .SetupGet(s => s.EnvironmentName)
                .Returns(environmentName);

            var resolver = BuildService(options: new DataFilesOptions
            {
                BasePath = Path.Combine("data", "data-files")
            });

            Assert.Equal(
                Path.Combine(
                    resolver.DataSetsPath(),
                    version.DataSetId.ToString(),
                    "draft"
                ),
                resolver.DirectoryPath(version));
        }

        [Theory]
        [MemberData(nameof(GetEnvironmentNames))]
        public void ValidFilePaths(string environmentName)
        {
            DataSetVersion version = _dataFixture.DefaultDataSetVersion();

            _webHostEnvironmentMock
                .SetupGet(s => s.EnvironmentName)
                .Returns(environmentName);

            var resolver = BuildService(options: new DataFilesOptions
            {
                BasePath = Path.Combine("data", "data-files")
            });

            var directoryPath = resolver.DirectoryPath(version);

            Assert.Equal(
                Path.Combine(directoryPath, DataSetFilenames.CsvDataFile),
                resolver.CsvDataPath(version)
            );
            Assert.Equal(
                Path.Combine(directoryPath, DataSetFilenames.CsvMetadataFile),
                resolver.CsvMetadataPath(version)
            );
            Assert.Equal(
                Path.Combine(directoryPath, DataSetFilenames.DuckDbDatabaseFile),
                resolver.DuckDbPath(version)
            );
            Assert.Equal(
                Path.Combine(directoryPath, DataSetFilenames.DuckDbLoadSqlFile),
                resolver.DuckDbLoadSqlPath(version)
            );
            Assert.Equal(
                Path.Combine(directoryPath, DataSetFilenames.DuckDbSchemaSqlFile),
                resolver.DuckDbSchemaSqlPath(version)
            );
            Assert.Equal(
                Path.Combine(directoryPath, DataTable.ParquetFile),
                resolver.DataPath(version)
            );
            Assert.Equal(
                Path.Combine(directoryPath, FilterOptionsTable.ParquetFile),
                resolver.FiltersPath(version)
            );
            Assert.Equal(
                Path.Combine(directoryPath, IndicatorsTable.ParquetFile),
                resolver.IndicatorsPath(version)
            );
            Assert.Equal(
                Path.Combine(directoryPath, LocationOptionsTable.ParquetFile),
                resolver.LocationsPath(version)
            );
            Assert.Equal(
                Path.Combine(directoryPath, TimePeriodsTable.ParquetFile),
                resolver.TimePeriodsPath(version)
            );
        }
    }

    private IDataSetVersionPathResolver BuildService(
        DataFilesOptions options,
        IWebHostEnvironment? webHostEnvironment = null)
    {
        return new DataSetVersionPathResolver(
            options.ToOptionsWrapper(),
            webHostEnvironment ?? _webHostEnvironmentMock.Object
        );
    }
}
