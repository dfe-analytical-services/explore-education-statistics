using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Tests;

public abstract class ParquetPathResolverTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock = new();

    public class ConstructorTests : ParquetPathResolverTests
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

    public class PathTests : ParquetPathResolverTests
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

            var expectedBasePath = Path.Combine(
                PathUtils.ProjectRootPath,
                "data",
                "parquet-files"
            );

            Assert.Equal(
                Path.Combine(expectedBasePath, version.ParquetDirectoryPath),
                resolver.DirectoryPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedBasePath, version.DataParquetPath),
                resolver.DataPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedBasePath, version.FiltersParquetPath),
                resolver.FiltersPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedBasePath, version.LocationsParquetPath),
                resolver.LocationsPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedBasePath, version.TimePeriodsParquetPath),
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

            var expectedBasePath = Path.Combine(
                Assembly.GetExecutingAssembly().GetDirectoryPath(),
                "data",
                "parquet-files"
            );

            Assert.Equal(
                Path.Combine(expectedBasePath, version.ParquetDirectoryPath),
                resolver.DirectoryPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedBasePath, version.DataParquetPath),
                resolver.DataPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedBasePath, version.FiltersParquetPath),
                resolver.FiltersPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedBasePath, version.LocationsParquetPath),
                resolver.LocationsPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedBasePath, version.TimePeriodsParquetPath),
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

            var expectedBasePath = Path.Combine(
                "data",
                "parquet-files"
            );

            Assert.Equal(
                Path.Combine(expectedBasePath, version.ParquetDirectoryPath),
                resolver.DirectoryPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedBasePath, version.DataParquetPath),
                resolver.DataPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedBasePath, version.FiltersParquetPath),
                resolver.FiltersPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedBasePath, version.LocationsParquetPath),
                resolver.LocationsPath(version)
            );
            Assert.Equal(
                Path.Combine(expectedBasePath, version.TimePeriodsParquetPath),
                resolver.TimePeriodsPath(version)
            );
        }
    }

    private ParquetPathResolver BuildService(
        ParquetFilesOptions options,
        IWebHostEnvironment? webHostEnvironment = null)
    {
        return new ParquetPathResolver(
            new OptionsWrapper<ParquetFilesOptions>(options),
            webHostEnvironment ?? _webHostEnvironmentMock.Object
        );
    }
}
