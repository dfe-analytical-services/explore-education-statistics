#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class DataImportGeneratorExtensions
{
    public static Generator<DataImport> DefaultDataImport(this DataFixture fixture)
        => fixture.Generator<DataImport>().WithDefaults();

    public static Generator<DataImport> WithDefaults(this Generator<DataImport> generator)
        => generator.ForInstance(d => d.SetDefaults());
    
    public static InstanceSetters<DataImport> SetDefaults(this InstanceSetters<DataImport> setters)
        => setters
            .SetDefault(d => d.Id)
            .SetFiles("data");
    
    public static Generator<DataImport> WithSubjectId(
        this Generator<DataImport> generator,
        Guid subjectId)
        => generator.ForInstance(s => s.Set(d => d.SubjectId, subjectId));
    
    public static Generator<DataImport> WithFiles(
        this Generator<DataImport> generator,
        string dataFileName)
        => generator.ForInstance(s => s.SetFiles(dataFileName));

    public static Generator<DataImport> WithStatus(
        this Generator<DataImport> generator,
        DataImportStatus status)
        => generator.ForInstance(s => s.Set(d => d.Status, status));

    public static Generator<DataImport> WithRowCounts(
        this Generator<DataImport> generator,
        int? totalRows = null,
        int? expectedImportedRows = null,
        int? importedRows = null,
        int? lastProcessedRowIndex = null)
        => generator.ForInstance(s => s
            .Set(d => d.TotalRows, totalRows)
            .Set(d => d.ExpectedImportedRows, expectedImportedRows)
            .Set(d => d.ImportedRows, importedRows ?? 0)
            .Set(d => d.LastProcessedRowIndex, lastProcessedRowIndex));
    
    public static InstanceSetters<DataImport> SetFiles(
        this InstanceSetters<DataImport> setters,
        string dataFileName)
        => setters
            .Set(d => d.File, new File
            {
                Id = Guid.NewGuid(),
                Filename = $"{dataFileName}.csv",
                Type = FileType.Data
            })
            .Set(d => d.MetaFile, new File
            {
                Id = Guid.NewGuid(),
                Filename = $"{dataFileName}.meta.csv",
                Type = FileType.Metadata
            });

}
