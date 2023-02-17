#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class ReleaseSubjectGeneratorExtensions
{
    public static Generator<ReleaseSubject> DefaultReleaseSubject(this DataFixture fixture)
        => fixture.Generator<ReleaseSubject>().WithDefaults();

    public static Generator<ReleaseSubject> WithDefaults(this Generator<ReleaseSubject> generator)
        => generator.ForInstance(s => s.SetDefaults());
    
    public static InstanceSetters<ReleaseSubject> SetDefaults(this InstanceSetters<ReleaseSubject> setters)
        => setters
            .SetDefault(rs => rs.ReleaseId)
            .SetDefault(rs => rs.SubjectId);
}
