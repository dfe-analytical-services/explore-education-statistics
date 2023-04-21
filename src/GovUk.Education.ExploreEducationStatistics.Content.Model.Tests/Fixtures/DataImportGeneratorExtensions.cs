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
            .SetDefaultFiles("data");
    
    public static Generator<DataImport> WithSubjectId(
        this Generator<DataImport> generator,
        Guid subjectId)
        => generator.ForInstance(s => s.Set(d => d.SubjectId, subjectId));
    
    public static Generator<DataImport> WithDefaultFiles(
        this Generator<DataImport> generator,
        string filename)
        => generator.ForInstance(s => s.SetDefaultFiles(filename));

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
    
    public static InstanceSetters<DataImport> SetDefaultFiles(
        this InstanceSetters<DataImport> setters,
        string filename)
        => setters
            .Set(d => d.File, new File
            {
                Id = Guid.NewGuid(),
                Filename = $"{filename}.csv",
                Type = FileType.Data

            })
            .Set(d => d.MetaFile, new File
            {
                Id = Guid.NewGuid(),
                Filename = $"{filename}.meta.csv",
                Type = FileType.Data

            });

}
