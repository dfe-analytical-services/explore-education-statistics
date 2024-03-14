using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ReleaseGeneratorExtensions
{
    public static Generator<Release> DefaultRelease(this DataFixture fixture)
        => fixture.Generator<Release>().WithDefaults();

    public static Generator<Release> DefaultRelease(
        this DataFixture fixture,
        int publishedVersions,
        bool draftVersion = false,
        int? year = null)
    {
        ReleaseVersion? previousVersion = null;
        return fixture.Generator<Release>()
            .WithDefaults()
            .WithVersions(releaseContext => fixture
                .DefaultReleaseVersion()
                .ForInstance(s => s
                    .Set(rv => rv.ReleaseName,
                        year != null ? year.ToString() : $"{2000 + releaseContext.FixtureTypeIndex}")
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

    public static Generator<Release> WithDefaults(this Generator<Release> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static InstanceSetters<Release> SetDefaults(this InstanceSetters<Release> setters)
        => setters
            .SetDefault(r => r.Id)
            .Set(r => r.Created, f => f.Date.Past())
            .Set(r => r.Updated, (f, r) => f.Date.Soon(refDate: r.Created));

    public static Generator<Release> WithVersions(
        this Generator<Release> generator,
        Func<SetterContext, IEnumerable<ReleaseVersion>> releaseVersions)
        => generator.ForInstance(s => s.SetVersions(releaseVersions.Invoke));

    public static Generator<Release> WithVersions(
        this Generator<Release> generator,
        IEnumerable<ReleaseVersion> releaseVersions)
        => generator.ForInstance(s => s.SetVersions(releaseVersions));

    public static Generator<Release> WithCreated(
        this Generator<Release> generator,
        DateTime created)
    {
        return generator.ForInstance(s => s.SetCreated(created));
    }

    public static Generator<Release> WithUpdated(
        this Generator<Release> generator,
        DateTime? updated = null)
    {
        return generator.ForInstance(s => s.SetUpdated(updated));
    }

    public static InstanceSetters<Release> SetVersions(
        this InstanceSetters<Release> setters,
        IEnumerable<ReleaseVersion> releaseVersions)
        => setters.SetVersions(_ => releaseVersions);

    private static InstanceSetters<Release> SetVersions(
        this InstanceSetters<Release> setters,
        Func<SetterContext, IEnumerable<ReleaseVersion>> releaseVersions)
        => setters.Set(
            r => r.Versions,
            (_, release, context) =>
            {
                var list = releaseVersions.Invoke(context).ToList();

                list.ForEach(releaseVersion =>
                {
                    releaseVersion.Release = release;
                    releaseVersion.ReleaseId = release.Id;
                });

                return list;
            }
        );

    public static InstanceSetters<Release> SetCreated(
        this InstanceSetters<Release> setters,
        DateTime created)
        => setters.Set(r => r.Created, created);

    public static InstanceSetters<Release> SetUpdated(
        this InstanceSetters<Release> setters,
        DateTime? updated)
        => setters.Set(r => r.Updated, updated);
}
