#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ReleaseGeneratorExtensions
{
    public static Generator<Release> DefaultRelease(this DataFixture fixture)
        => fixture.Generator<Release>().WithDefaults();

    public static Generator<Release> WithDefaults(this Generator<Release> generator)
        => generator.ForInstance(d => d.SetDefaults());
    
    public static Generator<Release> WithApprovalStatus(this Generator<Release> generator, ReleaseApprovalStatus status)
        => generator.ForInstance(d => d.SetApprovalStatus(status));
    
    public static Generator<Release> WithApprovalStatuses(this Generator<Release> generator, IEnumerable<ReleaseApprovalStatus> statuses)
    {
        statuses.ForEach((status, index) => 
            generator.ForIndex(index, s => s.SetApprovalStatus(status)));
        
        return generator;    
    }

    public static InstanceSetters<Release> SetDefaults(this InstanceSetters<Release> setters)
        => setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.Slug)
            .SetDefault(p => p.Title)
            .SetDefault(p => p.DataGuidance)
            .Set(p => p.ReleaseName, (_, _, context) => $"{1000 + context.Index}");
    
    public static InstanceSetters<Release> SetApprovalStatus(
        this InstanceSetters<Release> setters,
        ReleaseApprovalStatus status)
        => setters.Set(d => d.ApprovalStatus, status);
}
