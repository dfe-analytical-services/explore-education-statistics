#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ReleaseParentGeneratorExtensions
{
    public static Generator<ReleaseParent> DefaultReleaseParent(this DataFixture fixture)
        => fixture.Generator<ReleaseParent>().WithDefaults();

    public static Generator<ReleaseParent> WithDefaults(this Generator<ReleaseParent> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static InstanceSetters<ReleaseParent> SetDefaults(this InstanceSetters<ReleaseParent> setters)
        => setters
            .SetDefault(rp => rp.Id)
            .Set(rp => rp.Created, f => f.Date.Past())
            .Set(rp => rp.Updated, (f, rp) => f.Date.Soon(refDate: rp.Created));

    public static Generator<ReleaseParent> WithReleases(
        this Generator<ReleaseParent> generator,
        IEnumerable<Release> releases)
        => generator.ForInstance(s => s.SetReleases(releases));

    public static Generator<ReleaseParent> WithCreated(
        this Generator<ReleaseParent> generator,
        DateTime created)
    {
        return generator.ForInstance(s => s.SetCreated(created));
    }

    public static Generator<ReleaseParent> WithUpdated(
        this Generator<ReleaseParent> generator,
        DateTime? updated = null)
    {
        return generator.ForInstance(s => s.SetUpdated(updated));
    }

    public static InstanceSetters<ReleaseParent> SetReleases(
        this InstanceSetters<ReleaseParent> setters,
        IEnumerable<Release> releases)
        => setters.SetReleases(_ => releases);

    private static InstanceSetters<ReleaseParent> SetReleases(
        this InstanceSetters<ReleaseParent> setters,
        Func<SetterContext, IEnumerable<Release>> releases)
        => setters.Set(
            p => p.Releases,
            (_, releaseParent, context) =>
            {
                var list = releases.Invoke(context).ToList();

                list.ForEach(release =>
                {
                    release.ReleaseParent = releaseParent;
                    release.ReleaseParentId = releaseParent.Id;
                });

                return list;
            }
        );

    public static InstanceSetters<ReleaseParent> SetCreated(
        this InstanceSetters<ReleaseParent> setters,
        DateTime created)
        => setters.Set(rp => rp.Created, created);

    public static InstanceSetters<ReleaseParent> SetUpdated(
        this InstanceSetters<ReleaseParent> setters,
        DateTime? updated)
        => setters.Set(rp => rp.Updated, updated);
}
