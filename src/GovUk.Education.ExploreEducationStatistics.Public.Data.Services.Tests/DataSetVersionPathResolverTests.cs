using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

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
                BuildService(options: new ParquetFilesOptions
                {
                    BasePath = basePath
                })
            );
        }
    }

    public class PathTests : DataSetVersionPathResolverTests
    {
        [Fact]
        public void DevelopmentEnv_ValidBasePath_AllPathsCorrect()
        {
            DataSetVersion version = _dataFixture.DefaultDataSetVersion();

            _webHostEnvironmentMock
                .SetupGet(s => s.EnvironmentName)
                .Returns(Environments.Development);

            var resolver = BuildService(options: new ParquetFilesOptions
            {
                BasePath = Path.Combine("data", "parquet-files")
            });

            var expectedDirectoryPath = Path.Combine(
                PathUtils.ProjectRootPath,
                "data",
                "parquet-files",
                version.DataSetId.ToString(),
                "v1.0"
            );

            Assert.Equal(expectedDirectoryPath, resolver.DirectoryPath(version));
            Assert.Equal(
                Path.Combine(expectedDirectoryPath, DataTable.ParquetFile),
                resolver.DataPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedDirectoryPath, FilterOptionsTable.ParquetFile),
                resolver.FiltersPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedDirectoryPath, IndicatorsTable.ParquetFile),
                resolver.IndicatorsPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedDirectoryPath, LocationOptionsTable.ParquetFile),
                resolver.LocationsPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedDirectoryPath, TimePeriodsTable.ParquetFile),
                resolver.TimePeriodsPath(version)
            );
        }

        [Fact]
        public void IntegrationTestEnv_ValidBasePath_AllPathsCorrect()
        {
            DataSetVersion version = _dataFixture.DefaultDataSetVersion();

            _webHostEnvironmentMock
                .SetupGet(s => s.EnvironmentName)
                .Returns(HostEnvironmentExtensions.IntegrationTestEnvironment);

            var resolver = BuildService(options: new ParquetFilesOptions
            {
                BasePath = Path.Combine("data", "parquet-files")
            });

            var expectedDirectoryPath = Path.Combine(
                Assembly.GetExecutingAssembly().GetDirectoryPath(),
                "data",
                "parquet-files",
                version.DataSetId.ToString(),
                "v1.0"
            );

            Assert.Equal(expectedDirectoryPath, resolver.DirectoryPath(version));
            Assert.Equal(
                Path.Combine(expectedDirectoryPath, DataTable.ParquetFile),
                resolver.DataPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedDirectoryPath, FilterOptionsTable.ParquetFile),
                resolver.FiltersPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedDirectoryPath, IndicatorsTable.ParquetFile),
                resolver.IndicatorsPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedDirectoryPath, LocationOptionsTable.ParquetFile),
                resolver.LocationsPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedDirectoryPath, TimePeriodsTable.ParquetFile),
                resolver.TimePeriodsPath(version)
            );
        }

        [Fact]
        public void ProductionEnv_ValidBasePath_AllPathsCorrect()
        {
            DataSetVersion version = _dataFixture.DefaultDataSetVersion();

            _webHostEnvironmentMock
                .SetupGet(s => s.EnvironmentName)
                .Returns(Environments.Production);

            var resolver = BuildService(options: new ParquetFilesOptions
            {
                BasePath = Path.Combine("data", "parquet-files")
            });

            var expectedDirectoryPath = Path.Combine(
                "data",
                "parquet-files",
                version.DataSetId.ToString(),
                "v1.0"
            );

            Assert.Equal(expectedDirectoryPath, resolver.DirectoryPath(version));
            Assert.Equal(
                Path.Combine(expectedDirectoryPath, DataTable.ParquetFile),
                resolver.DataPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedDirectoryPath, FilterOptionsTable.ParquetFile),
                resolver.FiltersPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedDirectoryPath, IndicatorsTable.ParquetFile),
                resolver.IndicatorsPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedDirectoryPath, LocationOptionsTable.ParquetFile),
                resolver.LocationsPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedDirectoryPath, TimePeriodsTable.ParquetFile),
                resolver.TimePeriodsPath(version)
            );
        }
    }

    private IDataSetVersionPathResolver BuildService(
        ParquetFilesOptions options,
        IWebHostEnvironment? webHostEnvironment = null)
    {
        return new DataSetVersionPathResolver(
            new OptionsWrapper<ParquetFilesOptions>(options),
            webHostEnvironment ?? _webHostEnvironmentMock.Object
        );
    }
}
