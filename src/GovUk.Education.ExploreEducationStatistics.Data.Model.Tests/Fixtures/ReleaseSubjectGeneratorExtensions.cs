#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class ReleaseSubjectGeneratorExtensions
{
    public static Generator<ReleaseSubject> DefaultReleaseSubject(this DataFixture fixture)
        => fixture.Generator<ReleaseSubject>().WithDefaults();

    public static Generator<ReleaseSubject> WithDefaults(this Generator<ReleaseSubject> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<ReleaseSubject> WithRelease(this Generator<ReleaseSubject> generator, Release release)
        => generator.ForInstance(s => s.SetRelease(release));

    public static Generator<ReleaseSubject> WithSubject(this Generator<ReleaseSubject> generator, Subject subject)
        => generator.ForInstance(s => s.SetSubject(subject));
    
    public static Generator<ReleaseSubject> WithReleases(this Generator<ReleaseSubject> generator, IEnumerable<Release> releases)
    {
        releases.ForEach((release, index) => 
            generator.ForIndex(index, s => s.SetRelease(release)));
        
        return generator;
    }

    public static Generator<ReleaseSubject> WithSubjects(this Generator<ReleaseSubject> generator, IEnumerable<Subject> subjects)
    {
        subjects.ForEach((subject, index) => 
            generator.ForIndex(index, s => s.SetSubject(subject)));
        
        return generator;
    }
    
    public static InstanceSetters<ReleaseSubject> SetDefaults(this InstanceSetters<ReleaseSubject> setters)
        => setters
            .SetDefault(rs => rs.ReleaseId)
            .SetDefault(rs => rs.SubjectId);
    
    public static InstanceSetters<ReleaseSubject> SetRelease(this InstanceSetters<ReleaseSubject> setters, Release release)
        => setters.Set(rs => rs.Release, release);
    
    public static InstanceSetters<ReleaseSubject> SetSubject(this InstanceSetters<ReleaseSubject> setters, Subject subject)
        => setters.Set(rs => rs.Subject, subject);
}
