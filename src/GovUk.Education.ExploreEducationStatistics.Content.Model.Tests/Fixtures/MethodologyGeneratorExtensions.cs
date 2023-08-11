#nullable enable
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
            .SetDefault(p => p.Id)
            .SetDefault(p => p.Slug);
    
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
        );

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
