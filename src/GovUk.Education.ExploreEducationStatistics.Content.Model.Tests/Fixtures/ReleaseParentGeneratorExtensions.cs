using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ReleaseParentGeneratorExtensions
{
    public static Generator<ReleaseParent> DefaultReleaseParent(this DataFixture fixture)
        => fixture.Generator<ReleaseParent>().WithDefaults();

    public static Generator<ReleaseParent> DefaultReleaseParent(
        this DataFixture fixture,
        int publishedVersions,
        bool draftVersion = false,
        int? year = null)
    {
        ReleaseVersion? previousVersion = null;
        return fixture.Generator<ReleaseParent>()
            .WithDefaults()
            .WithVersions(releaseParentContext => fixture
                .DefaultReleaseVersion()
                .ForInstance(s => s
                    .Set(rv => rv.ReleaseName,
                        year != null ? year.ToString() : $"{2000 + releaseParentContext.FixtureTypeIndex}")
                    .Set(p => p.Slug,
                        (_, releaseVersion, _) => NamingUtils.SlugFromTitle(releaseVersion.YearTitle))
                    .Set(rv => rv.Version,
                        (_, _, context) => context.Index))
                .ForRange(..publishedVersions, s => s
                    .SetApprovalStatus(ReleaseApprovalStatus.Approved)
                    .Set(rv => rv.Published, (f, rel) =>
                        rel.Version == 0
                            ? f.Date.Past()
                            : f.Date.Between(previousVersion!.Published!.Value, DateTime.UtcNow)))
                .ForRange(1.., s => s
                    .SetPreviousVersion(previousVersion))
                .FinishWith(releaseVersion => previousVersion = releaseVersion)
                .Generate(draftVersion ? publishedVersions + 1 : publishedVersions));
    }

    public static Generator<ReleaseParent> WithDefaults(this Generator<ReleaseParent> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static InstanceSetters<ReleaseParent> SetDefaults(this InstanceSetters<ReleaseParent> setters)
        => setters
            .SetDefault(rp => rp.Id)
            .Set(rp => rp.Created, f => f.Date.Past())
            .Set(rp => rp.Updated, (f, rp) => f.Date.Soon(refDate: rp.Created));

    public static Generator<ReleaseParent> WithVersions(
        this Generator<ReleaseParent> generator,
        Func<SetterContext, IEnumerable<ReleaseVersion>> releaseVersions)
        => generator.ForInstance(s => s.SetVersions(releaseVersions.Invoke));

    public static Generator<ReleaseParent> WithVersions(
        this Generator<ReleaseParent> generator,
        IEnumerable<ReleaseVersion> releaseVersions)
        => generator.ForInstance(s => s.SetVersions(releaseVersions));

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

    public static InstanceSetters<ReleaseParent> SetVersions(
        this InstanceSetters<ReleaseParent> setters,
        IEnumerable<ReleaseVersion> releaseVersions)
        => setters.SetVersions(_ => releaseVersions);

    private static InstanceSetters<ReleaseParent> SetVersions(
        this InstanceSetters<ReleaseParent> setters,
        Func<SetterContext, IEnumerable<ReleaseVersion>> releaseVersions)
        => setters.Set(
            p => p.Versions,
            (_, releaseParent, context) =>
            {
                var list = releaseVersions.Invoke(context).ToList();

                list.ForEach(releaseVersion =>
                {
                    releaseVersion.ReleaseParent = releaseParent;
                    releaseVersion.ReleaseParentId = releaseParent.Id;
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
