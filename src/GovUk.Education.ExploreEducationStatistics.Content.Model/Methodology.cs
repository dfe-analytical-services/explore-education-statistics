#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public enum MethodologyStatus
    {
        Draft,
        Approved
    }

    public enum MethodologyPublishingStrategy
    {
        Immediately,
        WithRelease
    }

    public class Methodology
    {
        [Key] [Required] public Guid Id { get; set; }

        public string Title => AlternativeTitle ?? MethodologyParent.OwningPublicationTitle;

        public string? AlternativeTitle { get; set; }

        public string Slug => MethodologyParent.Slug;

        public MethodologyStatus Status { get; set; }

        public DateTime? Published { get; set; }

        public DateTime? Updated { get; set; }

        public List<ContentSection> Content { get; set; } = new();

        public List<ContentSection> Annexes { get; set; } = new();

        public string? InternalReleaseNote { get; set; }

        public MethodologyParent MethodologyParent { get; set; } = null!;

        public Guid MethodologyParentId { get; set; }

        public DateTime? Created { get; set; }

        public User? CreatedBy { get; set; }

        public Guid? CreatedById { get; set; }

        public Methodology? PreviousVersion { get; set; }

        public Guid? PreviousVersionId { get; set; }

        public int Version { get; set; }

        public MethodologyPublishingStrategy PublishingStrategy { get; set; }

        public Release? ScheduledWithRelease { get; set; }

        public Guid? ScheduledWithReleaseId { get; set; }

        public bool Approved => Status == MethodologyStatus.Approved;

        public bool ScheduledForPublishingWithRelease => PublishingStrategy == WithRelease;

        public bool ScheduledForPublishingImmediately => PublishingStrategy == Immediately;

        public bool ScheduledForPublishingWithPublishedRelease
        {
            get
            {
                if (PublishingStrategy != WithRelease)
                {
                    return false;
                }

                if (ScheduledWithReleaseId == null || ScheduledWithRelease == null)
                {
                    throw new InvalidOperationException("ScheduledWithRelease field not included in Methodology");
                }

                return ScheduledWithRelease.Live;
            }
        }

        public bool Amendment => PreviousVersionId != null && Published == null;

        public bool DraftFirstVersion => PreviousVersionId == null && Status == Draft;

        public Methodology CreateMethodologyAmendment(DateTime createdDate, Guid createdByUserId)
        {
            var copy = MemberwiseClone() as Methodology;
            copy.Id = Guid.NewGuid();
            copy.Status = Draft;
            copy.Published = null;
            copy.Updated = null;
            copy.Version = Version + 1;
            copy.Created = createdDate;
            copy.CreatedBy = null;
            copy.CreatedById = createdByUserId;
            copy.PreviousVersion = null;
            copy.PreviousVersionId = Id;
            copy.PublishingStrategy = Immediately;
            copy.ScheduledWithRelease = null;
            copy.ScheduledWithReleaseId = null;
            copy.MethodologyParent = null!;
            copy.InternalReleaseNote = null;

            copy.Annexes = Annexes
                .Select(c => c.Clone(createdDate))
                .ToList();

            copy.Content = Content
                .Select(c => c.Clone(createdDate))
                .ToList();

            return copy;
        }
    }
}
