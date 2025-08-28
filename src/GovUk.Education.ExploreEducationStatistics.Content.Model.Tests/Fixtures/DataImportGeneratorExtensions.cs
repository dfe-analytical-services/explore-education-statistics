using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class DataImportGeneratorExtensions
{
    public static Generator<DataImport> DefaultDataImport(this DataFixture fixture)
        => fixture.Generator<DataImport>().WithDefaults();

    public static Generator<DataImport> WithDefaults(this Generator<DataImport> generator)
        => generator.ForInstance(d => d.SetDefaults());
    
    public static Generator<DataImport> WithSubjectId(
        this Generator<DataImport> generator,
        Guid subjectId)
        => generator.ForInstance(s => s.SetSubjectId(subjectId));
    
    public static Generator<DataImport> WithDefaultFiles(
        this Generator<DataImport> generator,
        string dataFileName,
        bool metaSet = true)
    {
        if (metaSet)
        {
            return generator.ForInstance(s => s.SetDefaultFiles(dataFileName));
        }
        return generator.ForInstance(s => s.SetDefaultFilesWithoutMeta(dataFileName));
    }

    public static Generator<DataImport> WithFile(
        this Generator<DataImport> generator,
        File file)
        => generator.ForInstance(s => s.SetFile(file));

    public static Generator<DataImport> WithMetaFile(
        this Generator<DataImport> generator,
        File metaFile)
        => generator.ForInstance(s => s.SetMetaFile(metaFile));

    public static Generator<DataImport> WithZipFile(
        this Generator<DataImport> generator,
        File zipFile)
        => generator.ForInstance(s => s.SetZipFile(zipFile));

    public static Generator<DataImport> WithStatus(
        this Generator<DataImport> generator,
        DataImportStatus status)
        => generator.ForInstance(s => s.SetStatus(status));

    public static Generator<DataImport> WithStagePercentageComplete(
        this Generator<DataImport> generator,
        int stagePercentageComplete)
        => generator.ForInstance(s => s.SetStagePercentageComplete(stagePercentageComplete));

    public static Generator<DataImport> WithTotalRows(
        this Generator<DataImport> generator,
        int totalRows)
        => generator.ForInstance(s => s.SetTotalRows(totalRows));

    public static Generator<DataImport> WithExpectedImportedRows(
        this Generator<DataImport> generator,
        int expectedImportedRows)
        => generator.ForInstance(s => s.SetExpectedImportedRows(expectedImportedRows));

    public static Generator<DataImport> WithImportedRows(
        this Generator<DataImport> generator,
        int importedRows)
        => generator.ForInstance(s => s.SetImportedRows(importedRows));

    public static Generator<DataImport> WithLastProcessedRowIndex(
        this Generator<DataImport> generator,
        int lastProcessedRowIndex)
        => generator.ForInstance(s => s.SetLastProcessedRowIndex(lastProcessedRowIndex));

    public static InstanceSetters<DataImport> SetDefaults(this InstanceSetters<DataImport> setters)
        => setters
            .SetDefault(d => d.Id)
            .SetDefaultFiles("data");

    public static InstanceSetters<DataImport> SetSubjectId(
        this InstanceSetters<DataImport> setters,
        Guid subjectId)
        => setters.Set(d => d.SubjectId, subjectId);

    public static InstanceSetters<DataImport> SetDefaultFiles(
        this InstanceSetters<DataImport> setters,
        string dataFileName)
        => setters
            .Set(
                d => d.File,
                (_, d, context) => context.Fixture
                    .DefaultFile(FileType.Data)
                    .WithFilename($"{dataFileName}.csv")
                    .WithSubjectId(d.SubjectId)
            )
            .Set(d => d.FileId, (_, d) => d.File.Id)
            .Set(
                d => d.MetaFile,
                (_, d, context) => context.Fixture
                    .DefaultFile(FileType.Metadata)
                    .WithFilename($"{dataFileName}.meta.csv")
                    .WithSubjectId(d.SubjectId)
            )
            .Set(d => d.MetaFileId, (_, d) => d.MetaFile.Id);

    public static InstanceSetters<DataImport> SetDefaultFilesWithoutMeta(
        this InstanceSetters<DataImport> setters,
        string dataFileName)
        => setters
            .Set(
                d => d.File,
                (_, d, context) => context.Fixture
                    .DefaultFile(FileType.Data)
                    .WithDataSetFileMeta(null)
                    .WithDataSetFileVersionGeographicLevels([])
                    .WithFilename($"{dataFileName}.csv")
                    .WithSubjectId(d.SubjectId)
            )
            .Set(d => d.FileId, (_, d) => d.File.Id)
            .Set(
                d => d.MetaFile,
                (_, d, context) => context.Fixture
                    .DefaultFile(FileType.Metadata)
                    .WithFilename($"{dataFileName}.meta.csv")
                    .WithSubjectId(d.SubjectId)
            )
            .Set(d => d.MetaFileId, (_, d) => d.MetaFile.Id);

    public static InstanceSetters<DataImport> SetFile(
        this InstanceSetters<DataImport> setters,
        File file)
        => setters
            .Set(d => d.File, file)
            .Set(d => d.FileId, (_, d) => d.File.Id);

    public static InstanceSetters<DataImport> SetMetaFile(
        this InstanceSetters<DataImport> setters,
        File file)
        => setters
            .Set(d => d.MetaFile, file)
            .Set(d => d.MetaFileId, (_, d) => d.MetaFile.Id);

    public static InstanceSetters<DataImport> SetZipFile(
        this InstanceSetters<DataImport> setters,
        File file)
        => setters
            .Set(d => d.ZipFile, file)
            .Set(d => d.ZipFileId, (_, d) => d.ZipFile?.Id);

    public static InstanceSetters<DataImport> SetStatus(
        this InstanceSetters<DataImport> setters,
        DataImportStatus status)
        => setters.Set(d => d.Status, status);
    
    public static InstanceSetters<DataImport> SetStagePercentageComplete(
        this InstanceSetters<DataImport> setters,
        int stagePercentageComplete)
        => setters.Set(d => d.StagePercentageComplete, stagePercentageComplete);

    public static InstanceSetters<DataImport> SetTotalRows(
        this InstanceSetters<DataImport> setters,
        int totalRows)
        => setters.Set(d => d.TotalRows, totalRows);

    public static InstanceSetters<DataImport> SetExpectedImportedRows(
        this InstanceSetters<DataImport> setters,
        int expectedImportedRows)
        => setters.Set(d => d.ExpectedImportedRows, expectedImportedRows);

    public static InstanceSetters<DataImport> SetImportedRows(
        this InstanceSetters<DataImport> setters,
        int importedRows)
        => setters.Set(d => d.ImportedRows, importedRows);

    public static InstanceSetters<DataImport> SetLastProcessedRowIndex(
        this InstanceSetters<DataImport> setters,
        int lastProcessedRowIndex)
        => setters.Set(d => d.LastProcessedRowIndex, lastProcessedRowIndex);
}
