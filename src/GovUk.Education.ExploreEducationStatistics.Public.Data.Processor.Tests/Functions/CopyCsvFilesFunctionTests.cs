using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using File = System.IO.File;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class CopyCsvFilesFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class CopyCsvFilesTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : CopyCsvFilesFunctionTests(fixture)
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

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseVersions.Add(releaseVersion);
                context.ReleaseFiles.AddRange(releaseDataFile, releaseMetaFile);
            });

            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(
                Stage.PreviousStage(),
                releaseFileId: releaseDataFile.Id
            );

            var blobStorageService = GetRequiredService<IPrivateBlobStorageService>();

            var testData = ProcessorTestData.AbsenceSchool;

            var sourceDataFileContent = await File.ReadAllTextAsync(testData.CsvDataFilePath);

            await using (var contentStream = sourceDataFileContent.ToStream())
            {
                await blobStorageService.UploadStream(
                    BlobContainers.PrivateReleaseFiles,
                    releaseDataFile.Path(),
                    contentStream,
                    ContentTypes.Csv
                );
            }

            var sourceMetadataFileContent = await File.ReadAllTextAsync(testData.CsvMetadataFilePath);

            await using (var contentStream = sourceMetadataFileContent.ToStream())
            {
                await blobStorageService.UploadStream(
                    BlobContainers.PrivateReleaseFiles,
                    releaseMetaFile.Path(),
                    contentStream,
                    ContentTypes.Csv
                );
            }

            var function = GetRequiredService<CopyCsvFilesFunction>();
            await function.CopyCsvFiles(instanceId, CancellationToken.None);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext
                .DataSetVersionImports.Include(dataSetVersionImport => dataSetVersionImport.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Equal(DataSetVersionStatus.Processing, savedImport.DataSetVersion.Status);

            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();

            Assert.True(Directory.Exists(dataSetVersionPathResolver.DirectoryPath(dataSetVersion)));
            var actualDataSetVersionFiles = Directory
                .GetFiles(dataSetVersionPathResolver.DirectoryPath(dataSetVersion))
                .Select(Path.GetFullPath)
                .ToArray();

            Assert.Equal(2, actualDataSetVersionFiles.Length);

            var expectedCsvDataPath = dataSetVersionPathResolver.CsvDataPath(dataSetVersion);
            Assert.Contains(expectedCsvDataPath, actualDataSetVersionFiles);
            Assert.Equal(sourceDataFileContent, await DecompressFileToString(expectedCsvDataPath));

            var expectedCsvMetadataPath = dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion);
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
