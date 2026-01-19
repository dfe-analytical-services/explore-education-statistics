using System.Numerics;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TestData;

public static class CommonTestDataUtils
{
    private static readonly DataFixture DataFixture = new(new Random().Next());

    public static void SetupCsvDataFilesForDataSetVersion(
        IDataSetVersionPathResolver dataSetVersionPathResolver,
        ProcessorTestData processorTestData,
        DataSetVersion dataSetVersion
    )
    {
        var dataSetVersionDir = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);

        if (!Directory.Exists(dataSetVersionDir))
        {
            Directory.CreateDirectory(dataSetVersionDir);
        }

        // Prepare the data set version directory with data and metadata CSV files
        File.Copy(
            sourceFileName: processorTestData.CsvDataGzipFilePath,
            destFileName: dataSetVersionPathResolver.CsvDataPath(dataSetVersion)
        );
        File.Copy(
            sourceFileName: processorTestData.CsvMetadataFilePath,
            destFileName: dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion)
        );
    }

    public static async Task<(
        DataSetVersion initialVersion,
        DataSetVersion nextVersion,
        Guid instanceId
    )> CreateDataSetInitialAndNextVersion(
        PublicDataDbContext publicDataDbContext,
        DataSetVersionStatus nextVersionStatus,
        DataSetVersionImportStage nextVersionImportStage,
        DataSetVersionMeta? initialVersionMeta = null,
        DataSetVersionMeta? nextVersionMeta = null
    )
    {
        var (initialVersion, _) = await CreateDataSetInitialVersion(
            publicDataDbContext: publicDataDbContext,
            dataSetStatus: DataSetStatus.Published,
            dataSetVersionStatus: DataSetVersionStatus.Published,
            importStage: DataSetVersionImportStage.Completing,
            meta: initialVersionMeta
        );

        var (nextVersion, instanceId) = await CreateDataSetNextVersion(
            publicDataDbContext: publicDataDbContext,
            initialVersion: initialVersion,
            status: nextVersionStatus,
            importStage: nextVersionImportStage,
            meta: nextVersionMeta
        );

        return (initialVersion, nextVersion, instanceId);
    }

    public static async Task<(DataSetVersion dataSetVersion, Guid instanceId)> CreateDataSetInitialVersion(
        PublicDataDbContext publicDataDbContext,
        DataSetVersionImportStage importStage,
        DataSetStatus dataSetStatus = DataSetStatus.Draft,
        DataSetVersionStatus dataSetVersionStatus = DataSetVersionStatus.Processing,
        Guid? releaseFileId = null,
        DataSetVersionMeta? meta = null
    )
    {
        DataSet dataSet = DataFixture.DefaultDataSet().WithStatus(dataSetStatus);

        await publicDataDbContext.AddTestData(context => context.DataSets.Add(dataSet));

        return await CreateDataSetVersion(
            publicDataDbContext: publicDataDbContext,
            dataSetId: dataSet.Id,
            importStage: importStage,
            status: dataSetVersionStatus,
            releaseFileId: releaseFileId,
            meta: meta
        );
    }

    public static async Task<(DataSetVersion nextVersion, Guid instanceId)> CreateDataSetNextVersion(
        PublicDataDbContext publicDataDbContext,
        DataSetVersion initialVersion,
        DataSetVersionStatus status,
        DataSetVersionImportStage importStage,
        DataSetVersionMeta? meta = null
    )
    {
        var defaultNextVersion = initialVersion.DefaultNextVersion();

        return await CreateDataSetVersion(
            publicDataDbContext: publicDataDbContext,
            dataSetId: initialVersion.DataSetId,
            status: status,
            importStage: importStage,
            versionMajor: defaultNextVersion.Major,
            versionMinor: defaultNextVersion.Minor,
            meta: meta
        );
    }

    private static async Task<(DataSetVersion dataSetVersion, Guid instanceId)> CreateDataSetVersion(
        PublicDataDbContext publicDataDbContext,
        Guid dataSetId,
        DataSetVersionImportStage importStage,
        DataSetVersionStatus status = DataSetVersionStatus.Processing,
        Guid? releaseFileId = null,
        BigInteger? versionMajor = null, // default to 1 below
        BigInteger versionMinor = default, // default == 0
        DataSetVersionMeta? meta = null
    )
    {
        var dataSet = await publicDataDbContext.DataSets.SingleAsync(ds => ds.Id == dataSetId);

        DataSetVersionImport dataSetVersionImport = DataFixture.DefaultDataSetVersionImport().WithStage(importStage);

        var dataSetVersionGenerator = DataFixture
            .DefaultDataSetVersion()
            .WithDataSet(dataSet)
            .WithStatus(status)
            .WithImports(() => [dataSetVersionImport])
            .WithVersionNumber(major: versionMajor ?? new BigInteger(1), minor: versionMinor)
            .FinishWith(dsv =>
            {
                if (releaseFileId != null)
                {
                    dsv.Release.ReleaseFileId = releaseFileId.Value;
                }

                if (status == DataSetVersionStatus.Published)
                {
                    dsv.DataSet.LatestLiveVersion = dsv;
                }
                else
                {
                    dsv.DataSet.LatestDraftVersion = dsv;
                }
            });

        if (meta?.FilterMetas is not null)
        {
            dataSetVersionGenerator = dataSetVersionGenerator.WithFilterMetas(() => meta.FilterMetas);
        }

        if (meta?.LocationMetas is not null)
        {
            dataSetVersionGenerator = dataSetVersionGenerator.WithLocationMetas(() => meta.LocationMetas);
        }

        if (meta?.GeographicLevelMeta is not null)
        {
            dataSetVersionGenerator = dataSetVersionGenerator.WithGeographicLevelMeta(() => meta.GeographicLevelMeta);
        }

        if (meta?.IndicatorMetas is not null)
        {
            dataSetVersionGenerator = dataSetVersionGenerator.WithIndicatorMetas(() => meta.IndicatorMetas);
        }

        if (meta?.TimePeriodMetas is not null)
        {
            dataSetVersionGenerator = dataSetVersionGenerator.WithTimePeriodMetas(() => meta.TimePeriodMetas);
        }

        var dataSetVersion = dataSetVersionGenerator.Generate();

        await publicDataDbContext.AddTestData(context =>
        {
            context.DataSetVersions.Add(dataSetVersion);
            context.DataSets.Update(dataSet);
        });

        return (dataSetVersion, dataSetVersionImport.InstanceId);
    }

    public static DuckDbConnection GetDuckDbConnection(
        IDataSetVersionPathResolver dataSetVersionPathResolver,
        DataSetVersion dataSetVersion
    )
    {
        return DuckDbConnection.CreateFileConnectionReadOnly(dataSetVersionPathResolver.DuckDbPath(dataSetVersion));
    }

    public static void AssertDataSetVersionDirectoryContainsOnlyFiles(
        IDataSetVersionPathResolver dataSetVersionPathResolver,
        DataSetVersion dataSetVersion,
        params string[] expectedFiles
    )
    {
        var actualFiles = Directory
            .GetFiles(dataSetVersionPathResolver.DirectoryPath(dataSetVersion))
            .Select(Path.GetFileName)
            .ToArray();

        // Assert that the directory contains the expected files and no others
        Assert.Equal(expectedFiles.Order(), actualFiles.Order());
    }
}
