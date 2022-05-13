#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class ReleaseSubject : ICreatedUpdatedTimestamps<DateTime?, DateTime?>
    {
        public Subject Subject { get; set; } = null!;

        public Guid SubjectId { get; set; }

        public Release Release { get; set; } = null!;

        public Guid ReleaseId { get; set; }

        public string? DataGuidance { get; set; }

        public List<FilterSequenceEntry>? FilterSequence { get; set; }

        public List<IndicatorGroupSequenceEntry>? IndicatorSequence { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Updated { get; set; }

        public ReleaseSubject CopyForRelease(Release release)
        {
            var releaseSubject = MemberwiseClone() as ReleaseSubject;

            releaseSubject.Release = release;
            releaseSubject.ReleaseId = release.Id;

            return releaseSubject;
        }
    }

    public abstract record SequenceEntry<TEntry, TChild>(TEntry Id, List<TChild> ChildSequence);

    public record FilterSequenceEntry(Guid Id, List<FilterGroupSequenceEntry> ChildSequence) :
        SequenceEntry<Guid, FilterGroupSequenceEntry>(Id, ChildSequence);

    public record FilterGroupSequenceEntry(Guid Id, List<Guid> ChildSequence) :
        SequenceEntry<Guid, Guid>(Id, ChildSequence);

    public record IndicatorGroupSequenceEntry(Guid Id, List<Guid> ChildSequence) :
        SequenceEntry<Guid, Guid>(Id, ChildSequence);
}
