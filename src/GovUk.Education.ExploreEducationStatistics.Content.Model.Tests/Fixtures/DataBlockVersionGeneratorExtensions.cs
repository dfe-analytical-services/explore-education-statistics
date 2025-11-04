using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class DataBlockVersionGeneratorExtensions
{
    public static Generator<DataBlockVersion> DefaultDataBlockVersion(this DataFixture fixture) =>
        fixture.Generator<DataBlockVersion>().WithDefaults();

    public static Generator<DataBlockVersion> WithDefaults(this Generator<DataBlockVersion> generator) =>
        generator.ForInstance(d => d.SetDefaults());

    public static Generator<DataBlockVersion> WithReleaseVersion(
        this Generator<DataBlockVersion> generator,
        ReleaseVersion releaseVersion
    ) => generator.ForInstance(d => d.SetReleaseVersion(releaseVersion));

    public static Generator<DataBlockVersion> WithReleaseVersionId(
        this Generator<DataBlockVersion> generator,
        Guid releaseVersionId
    ) => generator.ForInstance(d => d.SetReleaseVersionId(releaseVersionId));

    public static Generator<DataBlockVersion> WithSubjectId(
        this Generator<DataBlockVersion> generator,
        Guid subjectId
    ) => generator.ForInstance(d => d.SetSubjectId(subjectId));

    public static Generator<DataBlockVersion> WithOrder(this Generator<DataBlockVersion> generator, int order) =>
        generator.ForInstance(d => d.SetOrder(order));

    public static Generator<DataBlockVersion> WithVersion(this Generator<DataBlockVersion> generator, int version) =>
        generator.ForInstance(d => d.SetVersion(version));

    public static Generator<DataBlockVersion> WithQuery(
        this Generator<DataBlockVersion> generator,
        FullTableQuery query
    ) => generator.ForInstance(d => d.SetQuery(query));

    public static Generator<DataBlockVersion> WithTable(
        this Generator<DataBlockVersion> generator,
        TableBuilderConfiguration table
    ) => generator.ForInstance(d => d.SetTable(table));

    public static Generator<DataBlockVersion> WithCharts(
        this Generator<DataBlockVersion> generator,
        List<IChart> charts
    ) => generator.ForInstance(d => d.SetCharts(charts));

    public static Generator<DataBlockVersion> WithName(this Generator<DataBlockVersion> generator, string? name) =>
        generator.ForInstance(d => d.SetName(name));

    public static Generator<DataBlockVersion> WithDates(
        this Generator<DataBlockVersion> generator,
        DateTime? created = null,
        DateTime? updated = null,
        DateTime? published = null
    )
    {
        return generator.ForInstance(d => d.SetDates(created, updated, published));
    }

    public static InstanceSetters<DataBlockVersion> SetDefaults(this InstanceSetters<DataBlockVersion> setters) =>
        setters
            .SetDefault(d => d.Id)
            .SetDefault(d => d.Version)
            .SetDefault(d => d.Created)
            .Set(d => d.ContentBlock, (_, dataBlockVersion) => new DataBlock { Id = dataBlockVersion.Id })
            .SetDefault(d => d.Heading)
            .SetDefault(d => d.Name)
            .SetDefault(d => d.Order, offset: 1)
            .SetDefault(d => d.Source)
            .SetDefault(d => d.Version)
            .SetQuery(
                new FullTableQuery
                {
                    SubjectId = Guid.NewGuid(),
                    Filters = new List<Guid> { Guid.NewGuid() },
                    Indicators = new List<Guid> { Guid.NewGuid() },
                }
            )
            .SetTable(
                new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders
                    {
                        Rows = new List<TableHeader> { new(Guid.NewGuid().ToString(), TableHeaderType.Indicator) },
                        Columns = new List<TableHeader> { new(Guid.NewGuid().ToString(), TableHeaderType.Filter) },
                    },
                }
            )
            .Set(
                d => d.Comments,
                (_, dataBlockVersion) =>
                    Enumerable
                        .Range(1, 2)
                        .Select(num => new Comment
                        {
                            Id = Guid.NewGuid(),
                            Content = $"{dataBlockVersion.Name} comment {num}",
                        })
                        .ToList()
            );

    public static InstanceSetters<DataBlockVersion> SetReleaseVersion(
        this InstanceSetters<DataBlockVersion> setters,
        ReleaseVersion releaseVersion
    ) =>
        setters
            .Set(d => d.ReleaseVersion, releaseVersion)
            .SetReleaseVersionId(releaseVersion.Id)
            .Set((_, d, _) => releaseVersion.DataBlockVersions.Add(d));

    public static InstanceSetters<DataBlockVersion> SetReleaseVersionId(
        this InstanceSetters<DataBlockVersion> setters,
        Guid releaseVersionId
    ) => setters.Set(d => d.ReleaseVersionId, releaseVersionId);

    public static InstanceSetters<DataBlockVersion> SetOrder(
        this InstanceSetters<DataBlockVersion> setters,
        int order
    ) => setters.Set(d => d.Order, order);

    public static InstanceSetters<DataBlockVersion> SetVersion(
        this InstanceSetters<DataBlockVersion> setters,
        int version
    ) => setters.Set(d => d.Version, version);

    public static InstanceSetters<DataBlockVersion> SetQuery(
        this InstanceSetters<DataBlockVersion> setters,
        FullTableQuery query
    ) => setters.Set(d => d.Query, query);

    public static InstanceSetters<DataBlockVersion> SetTable(
        this InstanceSetters<DataBlockVersion> setters,
        TableBuilderConfiguration table
    ) => setters.Set(d => d.Table, table);

    public static InstanceSetters<DataBlockVersion> SetCharts(
        this InstanceSetters<DataBlockVersion> setters,
        List<IChart> charts
    ) => setters.Set(d => d.Charts, charts);

    public static InstanceSetters<DataBlockVersion> SetName(
        this InstanceSetters<DataBlockVersion> setters,
        string? name
    ) => setters.Set(d => d.Name, name);

    public static InstanceSetters<DataBlockVersion> SetDates(
        this InstanceSetters<DataBlockVersion> setters,
        DateTime? created = null,
        DateTime? updated = null,
        DateTime? published = null
    )
    {
        if (created != null)
        {
            setters = setters.Set(d => d.Created, created);
        }

        if (updated != null)
        {
            setters = setters.Set(d => d.Updated, updated);
        }

        if (published != null)
        {
            setters = setters.Set(d => d.Published, published);
        }

        return setters;
    }

    public static InstanceSetters<DataBlockVersion> SetSubjectId(
        this InstanceSetters<DataBlockVersion> setters,
        Guid subjectId
    ) => setters.Set((_, d, _) => d.Query.SubjectId = subjectId);
}
