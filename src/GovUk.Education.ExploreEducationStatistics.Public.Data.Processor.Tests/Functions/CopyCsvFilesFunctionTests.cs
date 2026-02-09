using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TestData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using File = System.IO.File;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class CopyCsvFilesFunctionTestsFixture()
    : OptimisedPublicDataProcessorCollectionFixture(
        capabilities:
        [
            PublicDataProcessorIntegrationTestCapability.Azurite,
            PublicDataProcessorIntegrationTestCapability.Postgres,
        ]
    )
{
    public CopyCsvFilesFunction Function = null!;

    protected override void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        base.ConfigureServicesAndConfiguration(serviceModifications);

        var dataFilesBasePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        serviceModifications.AddInMemoryCollection([
            new KeyValuePair<string, string?>("DataFiles:BasePath", dataFilesBasePath),
        ]);

        serviceModifications.ReplaceService<IDataSetVersionPathResolver, DataSetVersionPathResolver>(
            serviceLifetime: ServiceLifetime.Singleton
        );
    }

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);
        Function = lookups.GetService<CopyCsvFilesFunction>();
    }
}

[CollectionDefinition(nameof(CopyCsvFilesFunctionTestsFixture))]
public class CopyCsvFilesFunctionTestsCollection : ICollectionFixture<CopyCsvFilesFunctionTestsFixture>;

[Collection(nameof(CopyCsvFilesFunctionTestsFixture))]
public abstract class CopyCsvFilesFunctionTests(CopyCsvFilesFunctionTestsFixture fixture)
    : OptimisedFunctionAppIntegrationTestBase(fixture)
{
    private static readonly DataFixture DataFixture = new();

    public class CopyCsvFilesTests(CopyCsvFilesFunctionTestsFixture fixture) : CopyCsvFilesFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.CopyingCsvFiles;

        [Fact]
        public async Task Success()
        {
            var subjectId = Guid.NewGuid();

            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithRelease(DataFixture.DefaultRelease());

            var (releaseDataFile, releaseMetaFile) = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFiles(
                    DataFixture
                        .DefaultFile()
                        .WithSubjectId(subjectId)
                        .ForIndex(0, s => s.SetType(FileType.Data))
                        .ForIndex(1, s => s.SetType(FileType.Metadata))
                        .GenerateList()
                )
                .GenerateTuple2();

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseVersions.Add(releaseVersion);
                    context.ReleaseFiles.AddRange(releaseDataFile, releaseMetaFile);
                });

            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                fixture.GetPublicDataDbContext(),
                Stage.PreviousStage(),
                releaseFileId: releaseDataFile.Id
            );

            var testData = ProcessorTestData.AbsenceSchool;

            var sourceDataFileContent = await File.ReadAllTextAsync(testData.CsvDataFilePath);

            await using (var contentStream = sourceDataFileContent.ToStream())
            {
                await fixture
                    .GetPrivateBlobStorageService()
                    .UploadStream(
                        BlobContainers.PrivateReleaseFiles,
                        releaseDataFile.Path(),
                        contentStream,
                        ContentTypes.Csv
                    );
            }

            var sourceMetadataFileContent = await File.ReadAllTextAsync(testData.CsvMetadataFilePath);

            await using (var contentStream = sourceMetadataFileContent.ToStream())
            {
                await fixture
                    .GetPrivateBlobStorageService()
                    .UploadStream(
                        BlobContainers.PrivateReleaseFiles,
                        releaseMetaFile.Path(),
                        contentStream,
                        ContentTypes.Csv
                    );
            }

            await fixture.Function.CopyCsvFiles(instanceId, CancellationToken.None);

            var savedImport = await fixture
                .GetPublicDataDbContext()
                .DataSetVersionImports.Include(dataSetVersionImport => dataSetVersionImport.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Equal(DataSetVersionStatus.Processing, savedImport.DataSetVersion.Status);

            Assert.True(Directory.Exists(fixture.GetDataSetVersionPathResolver().DirectoryPath(dataSetVersion)));
            var actualDataSetVersionFiles = Directory
                .GetFiles(fixture.GetDataSetVersionPathResolver().DirectoryPath(dataSetVersion))
                .Select(Path.GetFullPath)
                .ToArray();

            Assert.Equal(2, actualDataSetVersionFiles.Length);

            var expectedCsvDataPath = fixture.GetDataSetVersionPathResolver().CsvDataPath(dataSetVersion);
            Assert.Contains(expectedCsvDataPath, actualDataSetVersionFiles);
            Assert.Equal(sourceDataFileContent, await DecompressFileToString(expectedCsvDataPath));

            var expectedCsvMetadataPath = fixture.GetDataSetVersionPathResolver().CsvMetadataPath(dataSetVersion);
            Assert.Contains(expectedCsvMetadataPath, actualDataSetVersionFiles);
            Assert.Equal(sourceMetadataFileContent, await File.ReadAllTextAsync(expectedCsvMetadataPath));
        }

        private static async Task<string> DecompressFileToString(string path)
        {
            var bytes = await File.ReadAllBytesAsync(path);
            return await CompressionUtils.DecompressToString(bytes, ContentEncodings.Gzip);
        }
    }
}
