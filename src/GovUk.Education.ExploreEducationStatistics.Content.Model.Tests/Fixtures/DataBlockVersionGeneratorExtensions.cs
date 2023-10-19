#nullable enable
using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class DataBlockVersionGeneratorExtensions
{
    public static Generator<DataBlockVersion> DefaultDataBlockVersion(this DataFixture fixture)
        => fixture.Generator<DataBlockVersion>().WithDefaults();

    public static Generator<DataBlockVersion> WithDefaults(this Generator<DataBlockVersion> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static Generator<DataBlockVersion> WithRelease(this Generator<DataBlockVersion> generator, Release release)
        => generator.ForInstance(d => d.SetRelease(release));

    public static Generator<DataBlockVersion> WithReleaseId(this Generator<DataBlockVersion> generator, Guid releaseId)
        => generator.ForInstance(d => d.SetReleaseId(releaseId));

    public static Generator<DataBlockVersion> WithVersion(this Generator<DataBlockVersion> generator, int version)
        => generator.ForInstance(d => d.SetVersion(version));

    public static Generator<DataBlockVersion> WithDates(
        this Generator<DataBlockVersion> generator,
        DateTime? created,
        DateTime? updated,
        DateTime? published)
    {
        return generator.ForInstance(d => d.SetDates(created, updated, published));
    }

    public static InstanceSetters<DataBlockVersion> SetDefaults(this InstanceSetters<DataBlockVersion> setters)
        => setters
            .SetDefault(d => d.Id)
            .SetDefault(d => d.Version)
            .SetDefault(d => d.Created)
            .Set(d => d.ContentBlock, (_, dataBlockVersion) => new DataBlock
            {
                Id = dataBlockVersion.Id
            })
            .SetDefault(d => d.Heading)
            .SetDefault(d => d.Name)
            .SetDefault(d => d.Order, offset: 1)
            .SetDefault(d => d.Source)
            .SetDefault(d => d.Version)
            // TODO EES-4467 - do we need defaults for these? Possibly...
            // .SetDefault(d => d.Charts)
            .Set(d => d.Comments, (_, dataBlockVersion) => Enumerable
                .Range(1, 2)
                .Select(num =>
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        Content = $"{dataBlockVersion.Name} comment {num}"
                    })
                .ToList())
            // .SetDefault(d => d.Query)
            // .SetDefault(d => d.Table)
        ;

    public static InstanceSetters<DataBlockVersion> SetRelease(
        this InstanceSetters<DataBlockVersion> setters,
        Release release)
        => setters
            .Set(d => d.Release, release)
            .SetReleaseId(release.Id);

    public static InstanceSetters<DataBlockVersion> SetReleaseId(
        this InstanceSetters<DataBlockVersion> setters,
        Guid releaseId)
        => setters.Set(d => d.ReleaseId, releaseId);

    public static InstanceSetters<DataBlockVersion> SetVersion(
        this InstanceSetters<DataBlockVersion> setters,
        int version)
        => setters.Set(d => d.Version, version);

    public static InstanceSetters<DataBlockVersion> SetDates(
        this InstanceSetters<DataBlockVersion> setters,
        DateTime? created,
        DateTime? updated,
        DateTime? published)
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
}
