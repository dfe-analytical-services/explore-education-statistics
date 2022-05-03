#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class ReleaseSubject : ICreatedUpdatedTimestamps<DateTime?, DateTime?>
    {
        public DateTime? Created { get; set; }

        public Subject Subject { get; set; } = null!;

        public Guid SubjectId { get; set; }

        public Release Release { get; set; } = null!;

        public Guid ReleaseId { get; set; }

        public string? DataGuidance { get; set; }

        public List<FilterSequenceEntry>? FilterSequence { get; set; }

        public List<IndicatorGroupSequenceEntry>? IndicatorSequence { get; set; }

        public ReleaseSubject CopyForRelease(Release release)
        {
            var releaseSubject = MemberwiseClone() as ReleaseSubject;

            releaseSubject.Release = release;
            releaseSubject.ReleaseId = release.Id;

            return releaseSubject;
        }

        public DateTime? Updated { get; set; }
    }

    public interface ISequenceEntry<TChildSequenceEntry>
    {
        Guid Id { get; init; }
        List<TChildSequenceEntry> ChildSequence { get; init; }
    }

    public record FilterSequenceEntry : ISequenceEntry<FilterGroupSequenceEntry>
    {
        public Guid Id { get; init; }
        public List<FilterGroupSequenceEntry> ChildSequence { get; init; } = new();
    }

    public record FilterGroupSequenceEntry : ISequenceEntry<Guid>
    {
        public Guid Id { get; init; }
        public List<Guid> ChildSequence { get; init; } = new();
    }

    public record IndicatorGroupSequenceEntry : ISequenceEntry<Guid>
    {
        public Guid Id { get; init; }
        public List<Guid> ChildSequence { get; init; } = new();
    }
}
