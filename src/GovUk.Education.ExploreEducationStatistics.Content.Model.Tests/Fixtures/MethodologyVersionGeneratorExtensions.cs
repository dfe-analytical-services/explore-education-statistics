#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class MethodologyVersionGeneratorExtensions
{
    public static Generator<MethodologyVersion> DefaultMethodologyVersion(this DataFixture fixture)
        => fixture.Generator<MethodologyVersion>().WithDefaults();

    public static Generator<MethodologyVersion> WithDefaults(this Generator<MethodologyVersion> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static Generator<MethodologyVersion> WithApprovalStatus(
        this Generator<MethodologyVersion> generator, 
        MethodologyApprovalStatus approvalStatus)
        => generator.ForInstance(d => d.SetApprovalStatus(approvalStatus));
    
    public static Generator<MethodologyVersion> WithApprovalStatuses(
        this Generator<MethodologyVersion> generator, 
        IEnumerable<MethodologyApprovalStatus> approvalStatuses)
    {
        approvalStatuses.ForEach((status, index) => 
            generator.ForIndex(index, s => s.SetApprovalStatus(status)));
        
        return generator;    
    }

    public static InstanceSetters<MethodologyVersion> SetDefaults(this InstanceSetters<MethodologyVersion> setters)
        => setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.Slug)
            .SetDefault(p => p.Title)
            .SetDefault(p => p.Version);
    
    public static InstanceSetters<MethodologyVersion> SetApprovalStatus(
        this InstanceSetters<MethodologyVersion> setters,
        MethodologyApprovalStatus approvalStatus) 
        => setters.Set(mv => mv.Status, approvalStatus);
}
