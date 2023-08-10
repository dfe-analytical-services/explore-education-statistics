#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class MethodologyVersionGeneratorExtensions
{
    public static Generator<MethodologyVersion> DefaultMethodologyVersion(this DataFixture fixture)
        => fixture.Generator<MethodologyVersion>().WithDefaults();

    public static Generator<MethodologyVersion> WithDefaults(this Generator<MethodologyVersion> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<MethodologyVersion> SetDefaults(this InstanceSetters<MethodologyVersion> setters)
        => setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.Slug)
            .SetDefault(p => p.Title)
            .SetDefault(p => p.Version);
    
    // public static Generator<MethodologyVersion> WithMethodologyVersions(
    //     this Generator<MethodologyVersion> generator,
    //     IEnumerable<MethodologyVersion> methodologyVersions)
    //     => generator.ForInstance(s => s.SetMethodologyVersions(methodologyVersions));
    //
    // public static Generator<MethodologyVersion> WithMethodologyVersions(
    //     this Generator<MethodologyVersion> generator,
    //     Func<SetterContext, IEnumerable<MethodologyVersion>> methodologyVersions)
    //     => generator.ForInstance(s => s.SetMethodologyVersions(methodologyVersions.Invoke));
    //
    // public static InstanceSetters<MethodologyVersion> SetMethodologyVersions(
    //     this InstanceSetters<MethodologyVersion> setters,
    //     IEnumerable<MethodologyVersion> methodologyVersions) 
    //     => setters.SetMethodologyVersions(_ => methodologyVersions);
    
    // private static InstanceSetters<MethodologyVersion> SetMethodologyVersions(
    //     this InstanceSetters<MethodologyVersion> setters,
    //     Func<SetterContext, IEnumerable<MethodologyVersion>> methodologyVersions)
    //     => setters.Set(
    //         m => m.Versions,
    //         (_, methodology, context) =>
    //         {
    //             var list = methodologyVersions.Invoke(context).ToList();
    //
    //             list.ForEach(methodologyVersion => methodologyVersion.Methodology = methodology);
    //
    //             return list;
    //         }
    //     );
    
    // public static InstanceSetters<MethodologyVersion> SetMethodologyVersions(
    //     this InstanceSetters<MethodologyVersion> setters,
    //     IEnumerable<MethodologyVersion> MethodologyVersions)
    //     => setters
    //         .Set(d => d.MethodologyVersions, MethodologyVersions.ToList());

}
