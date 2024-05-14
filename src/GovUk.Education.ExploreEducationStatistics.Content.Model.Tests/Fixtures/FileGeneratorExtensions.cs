using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class FileGeneratorExtensions
{
    public static Generator<File> DefaultFile(this DataFixture fixture)
        => fixture.Generator<File>().WithDefaults();

    public static Generator<File> WithDefaults(this Generator<File> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<File> SetDefaults(this InstanceSetters<File> setters)
        => setters
            .SetDefault(f => f.Id)
            .SetDefault(f => f.RootPath)
            .SetDefault(f => f.DataSetFileId)
            .SetContentLength(1024 * 1024)
            .SetContentType("text/csv")
            .SetType(FileType.Data)
            .SetDefault(f => f.Filename)
            .SetDataSetFileMeta(new DataSetFileMeta
            {
                GeographicLevels = [GeographicLevel.Country],
                TimePeriodRange = new TimePeriodRangeMeta
                {
                    Start = new TimePeriodRangeBoundMeta { TimeIdentifier = TimeIdentifier.CalendarYear, Period = "2000", },
                    End = new TimePeriodRangeBoundMeta { TimeIdentifier = TimeIdentifier.CalendarYear, Period = "2001", }
                },
                Filters = [new() { Id = Guid.NewGuid(), Label = "Filter 1", ColumnName = "filter_1", }],
                Indicators = [new() { Id = Guid.NewGuid(), Label = "Indicator 1", ColumnName = "indicator_1", }],
            })
            .Set(f => f.Filename, (_, f) => $"{f.Filename}.csv");

    public static Generator<File> WithContentLength(
        this Generator<File> generator,
        long contentLength)
        => generator.ForInstance(s => s.SetContentLength(contentLength));

    public static Generator<File> WithContentType(
        this Generator<File> generator,
        string contentType)
        => generator.ForInstance(s => s.SetContentType(contentType));

    public static Generator<File> WithFilename(
        this Generator<File> generator,
        string filename)
        => generator.ForInstance(s => s.SetFilename(filename));

    public static Generator<File> WithReplacing(
        this Generator<File> generator,
        File replacing)
        => generator.ForInstance(s => s.SetReplacing(replacing));

    public static Generator<File> WithReplacingId(
        this Generator<File> generator,
        Guid replacingId)
        => generator.ForInstance(s => s.SetReplacingId(replacingId));

    public static Generator<File> WithReplacedBy(
        this Generator<File> generator,
        File replacedBy)
        => generator.ForInstance(s => s.SetReplacedBy(replacedBy));

    public static Generator<File> WithReplacedById(
        this Generator<File> generator,
        Guid replacedById)
        => generator.ForInstance(s => s.SetReplacedById(replacedById));

    public static Generator<File> WithPublicApiDataSetId(
        this Generator<File> generator,
        Guid publicDataSetId)
        => generator.ForInstance(s => s.SetPublicApiDataSetId(publicDataSetId));

    public static Generator<File> WithPublicApiDataSetVersion(
        this Generator<File> generator,
        int major,
        int minor,
        int patch = 0)
        => generator.ForInstance(s => s.SetPublicApiDataSetVersion(major, minor, patch));

    public static Generator<File> WithPublicApiDataSetVersion(
        this Generator<File> generator,
        SemVersion version)
        => generator.ForInstance(s => s.SetPublicApiDataSetVersion(version));

    public static Generator<File> WithRootPath(
        this Generator<File> generator,
        Guid rootPath)
        => generator.ForInstance(s => s.SetRootPath(rootPath));

    public static Generator<File> WithSource(
        this Generator<File> generator,
        File source)
        => generator.ForInstance(s => s.SetSource(source));

    public static Generator<File> WithSourceId(
        this Generator<File> generator,
        Guid sourceId)
        => generator.ForInstance(s => s.SetSourceId(sourceId));

    public static Generator<File> WithSubjectId(
        this Generator<File> generator,
        Guid subjectId)
        => generator.ForInstance(s => s.SetSubjectId(subjectId));

    public static Generator<File> WithType(
        this Generator<File> generator,
        FileType type)
        => generator.ForInstance(s => s.SetType(type));

    public static Generator<File> WithDataSetFileMeta(
        this Generator<File> generator,
        DataSetFileMeta dataSetFileMeta)
        => generator.ForInstance(s => s.SetDataSetFileMeta(dataSetFileMeta));

    public static InstanceSetters<File> SetContentLength(
        this InstanceSetters<File> setters,
        long contentLength)
        => setters.Set(f => f.ContentLength, contentLength);

    public static InstanceSetters<File> SetContentType(
        this InstanceSetters<File> setters,
        string contentType)
        => setters.Set(f => f.ContentType, contentType);

    public static InstanceSetters<File> SetFilename(
        this InstanceSetters<File> setters,
        string filename)
        => setters.Set(f => f.Filename, filename);

    public static InstanceSetters<File> SetReplacedBy(
        this InstanceSetters<File> setters,
        File replacedBy)
        => setters.Set(f => f.ReplacedBy, replacedBy)
            .SetReplacedById(replacedBy.Id);

    public static InstanceSetters<File> SetReplacedById(
        this InstanceSetters<File> setters,
        Guid replacedById)
        => setters.Set(f => f.ReplacedById, replacedById);

    public static InstanceSetters<File> SetPublicApiDataSetId(
        this InstanceSetters<File> setters,
        Guid publicDataSetId)
        => setters.Set(f => f.PublicApiDataSetId, publicDataSetId);

    public static InstanceSetters<File> SetPublicApiDataSetVersion(
        this InstanceSetters<File> setters,
        int major,
        int minor,
        int patch = 0)
        => setters.Set(
            f => f.PublicApiDataSetVersion,
            new SemVersion(major: major, minor: minor, patch: patch));

    public static InstanceSetters<File> SetPublicApiDataSetVersion(
        this InstanceSetters<File> setters,
        SemVersion version)
        => setters.Set(f => f.PublicApiDataSetVersion, version);

    public static InstanceSetters<File> SetReplacing(
        this InstanceSetters<File> setters,
        File replacing)
        => setters.Set(f => f.Replacing, replacing)
            .SetReplacingId(replacing.Id);

    public static InstanceSetters<File> SetReplacingId(
        this InstanceSetters<File> setters,
        Guid replacingId)
        => setters.Set(f => f.ReplacingId, replacingId);

    public static InstanceSetters<File> SetSource(
        this InstanceSetters<File> setters,
        File source)
        => setters.Set(f => f.Source, source)
            .SetSourceId(source.Id);

    public static InstanceSetters<File> SetSourceId(
        this InstanceSetters<File> setters,
        Guid sourceId)
        => setters.Set(f => f.SourceId, sourceId);

    public static InstanceSetters<File> SetSubjectId(
        this InstanceSetters<File> setters,
        Guid subjectId)
        => setters.Set(f => f.SubjectId, subjectId);

    public static InstanceSetters<File> SetRootPath(
        this InstanceSetters<File> setters,
        Guid rootPath)
        => setters.Set(f => f.RootPath, rootPath);

    public static InstanceSetters<File> SetType(
        this InstanceSetters<File> setters,
        FileType type)
        => setters.Set(f => f.Type, type);

    public static InstanceSetters<File> SetDataSetFileMeta(
        this InstanceSetters<File> setters,
        DataSetFileMeta dataSetFileMeta)
        => setters.Set(f => f.DataSetFileMeta, dataSetFileMeta);
}
