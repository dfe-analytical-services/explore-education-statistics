using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class MethodologyGeneratorExtensions
{
    public static Generator<Methodology> DefaultMethodology(this DataFixture fixture)
        => fixture.Generator<Methodology>().WithDefaults();

    public static Generator<Methodology> WithDefaults(this Generator<Methodology> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<Methodology> SetDefaults(this InstanceSetters<Methodology> setters)
        => setters
            .SetDefault(m => m.Id)
            .SetDefault(m => m.OwningPublicationTitle)
            .SetDefault(m => m.OwningPublicationSlug);

    public static Generator<Methodology> WithOwningPublication(
        this Generator<Methodology> generator,
        Publication publication)
        => generator.ForInstance(s => s.SetOwningPublication(publication));

    public static Generator<Methodology> WithAdoptingPublication(
        this Generator<Methodology> generator,
        Publication publication)
        => generator.ForInstance(s => s.SetAdoptingPublication(publication));

    public static Generator<Methodology> WithMethodologyVersions(
        this Generator<Methodology> generator,
        IEnumerable<MethodologyVersion> methodologyVersions)
        => generator.ForInstance(s => s.SetMethodologyVersions(methodologyVersions));

    public static Generator<Methodology> WithMethodologyVersions(
        this Generator<Methodology> generator,
        Func<SetterContext, IEnumerable<MethodologyVersion>> methodologyVersions)
        => generator.ForInstance(s => s.SetMethodologyVersions(methodologyVersions.Invoke));

    public static Generator<Methodology> WithLatestPublishedVersion(
        this Generator<Methodology> generator,
        MethodologyVersion latestPublishedVersion)
        => generator.ForInstance(s => s.SetLatestPublishedVersion(latestPublishedVersion));

    public static Generator<Methodology> WithLatestPublishedVersionId(
        this Generator<Methodology> generator,
        Guid latestPublishedVersionId)
        => generator.ForInstance(s => s.SetLatestPublishedVersionId(latestPublishedVersionId));

    public static InstanceSetters<Methodology> SetMethodologyVersions(
        this InstanceSetters<Methodology> setters,
        IEnumerable<MethodologyVersion> methodologyVersions)
        => setters.SetMethodologyVersions(_ => methodologyVersions);

    private static InstanceSetters<Methodology> SetMethodologyVersions(
        this InstanceSetters<Methodology> setters,
        Func<SetterContext, IEnumerable<MethodologyVersion>> methodologyVersions)
        => setters.Set(
                m => m.Versions,
                (_, methodology, context) =>
                {
                    var list = methodologyVersions.Invoke(context).ToList();

                    list.ForEach(methodologyVersion => methodologyVersion.Methodology = methodology);

                    return list;
                }
            )
            .Set(
                (_, methodology, context) =>
                {
                    var list = methodologyVersions.Invoke(context).ToList();

                    var latestPublishedVersion = list
                        .Where(mv => mv.Published.HasValue)
                        .OrderBy(mv => mv.Published!)
                        .LastOrDefault();

                    if (latestPublishedVersion != null)
                    {
                        latestPublishedVersion.Methodology = methodology;
                        methodology.LatestPublishedVersion = latestPublishedVersion;
                        methodology.LatestPublishedVersionId = latestPublishedVersion.Id;
                    }
                }
            );

    private static InstanceSetters<Methodology> SetLatestPublishedVersion(
        this InstanceSetters<Methodology> setters,
        MethodologyVersion latestPublishedVersion)
        => setters.Set(
            m => m.LatestPublishedVersion,
            (_, methodology) =>
            {
                latestPublishedVersion.Methodology = methodology;

                return latestPublishedVersion;
            }
        )
        .SetLatestPublishedVersionId(latestPublishedVersion.Id);

    private static InstanceSetters<Methodology> SetLatestPublishedVersionId(
        this InstanceSetters<Methodology> setters,
        Guid latestPublishedVersionId)
        => setters.Set(m => m.LatestPublishedVersionId, latestPublishedVersionId);

    public static InstanceSetters<Methodology> SetOwningPublication(
        this InstanceSetters<Methodology> setters,
        Publication publication)
        => setters.SetPublication(_ => publication, owner: true);

    public static InstanceSetters<Methodology> SetAdoptingPublication(
        this InstanceSetters<Methodology> setters,
        Publication publication)
        => setters.SetPublication(_ => publication, owner: false);

    private static InstanceSetters<Methodology> SetPublication(
        this InstanceSetters<Methodology> setters,
        Func<SetterContext, Publication> publication,
        bool owner)
        => setters.Set(
            m => m.Publications,
            (_, methodology, context) =>
            {
                var newPublication = publication.Invoke(context);
                return methodology
                    .Publications
                    .Append(new PublicationMethodology
                    {
                        Methodology = methodology,
                        Publication = newPublication,
                        Owner = owner
                    })
                    .ToList();
            }
        );
}
