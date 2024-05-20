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
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
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
        private const string DataFileContent =
            "time_period,time_identifier,geographic_level,country_code,country_name,enrolments";

        private const string MetadataFileContent =
            "col_name,col_type,label,indicator_grouping,indicator_unit,indicator_dp,filter_hint,filter_grouping_column";

        [Fact]
        public async Task Success()
        {
            var instanceId = Guid.NewGuid();

            var subjectId = Guid.NewGuid();

            ReleaseVersion releaseVersion = DataFixture.DefaultReleaseVersion()
                .WithRelease(DataFixture.DefaultRelease());

            var (releaseDataFile, releaseMetaFile) = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFiles(DataFixture.DefaultFile()
                    .WithSubjectId(subjectId)
                    .ForIndex(0, s => s.SetType(FileType.Data))
                    .ForIndex(1, s => s.SetType(FileType.Metadata))
                    .GenerateList())
                .Generate(2)
                .ToTuple2();

            await AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseVersions.Add(releaseVersion);
                context.ReleaseFiles.AddRange(releaseDataFile, releaseMetaFile);
            });

            DataSet dataSet = DataFixture.DefaultDataSet();

            await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithStatus(DataSetVersionStatus.Processing)
                .WithReleaseFileId(releaseDataFile.Id)
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .WithInstanceId(instanceId)
                    .WithStage(DataSetVersionImportStage.Pending)
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var blobStorageService = GetRequiredService<IPrivateBlobStorageService>();

            await blobStorageService.UploadStream(
                BlobContainers.PrivateReleaseFiles,
                releaseDataFile.Path(),
                DataFileContent.ToStream(),
                ContentTypes.Csv);

            await blobStorageService.UploadStream(
                BlobContainers.PrivateReleaseFiles,
                releaseMetaFile.Path(),
                MetadataFileContent.ToStream(),
                ContentTypes.Csv);

            var function = GetRequiredService<CopyCsvFilesFunction>();

            await function.CopyCsvFiles(
                dataSetVersionId: dataSetVersion.Id,
                instanceId: instanceId,
                CancellationToken.None);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedDataSetVersion = await publicDataDbContext.DataSetVersions
                .Include(dsv => dsv.Imports)
                .FirstAsync(dsv => dsv.Id == dataSetVersion.Id);

            var savedImport = savedDataSetVersion.Imports.Single();
            Assert.Equal(DataSetVersionImportStage.CopyingCsvFiles, savedImport.Stage);

            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();

            var expectedCsvDataPath = dataSetVersionPathResolver.CsvDataPath(dataSetVersion);
            Assert.True(File.Exists(expectedCsvDataPath));
            Assert.Equal(DataFileContent, await DecompressFileToString(expectedCsvDataPath));

            var expectedCsvMetadataPath = dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion);
            Assert.True(File.Exists(expectedCsvMetadataPath));
            Assert.Equal(MetadataFileContent, await DecompressFileToString(expectedCsvMetadataPath));
        }

        private static async Task<string> DecompressFileToString(string path)
        {
            var bytes = await File.ReadAllBytesAsync(path);
            return await CompressionUtils.DecompressToString(bytes, ContentEncodings.Gzip);
        }
    }
}
