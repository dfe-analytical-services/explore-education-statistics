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

    public class MethodologyVersion
    {
        [Key] [Required] public Guid Id { get; set; }

        public string Title => AlternativeTitle ?? Methodology.OwningPublicationTitle;

        public string? AlternativeTitle { get; set; }

        public string Slug => Methodology.Slug;

        public MethodologyStatus Status { get; set; }

        public DateTime? Published { get; set; }

        public DateTime? Updated { get; set; }

        public MethodologyVersionContent MethodologyContent { get; set; } = new();

        public List<MethodologyNote> Notes { get; set; } = new();

        public string? InternalReleaseNote { get; set; }

        public Methodology Methodology { get; set; } = null!;

        public Guid MethodologyId { get; set; }

        public DateTime? Created { get; set; }

        public User? CreatedBy { get; set; }

        public Guid? CreatedById { get; set; }

        public MethodologyVersion? PreviousVersion { get; set; }

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
                    throw new InvalidOperationException("ScheduledWithRelease field not included in MethodologyVersion");
                }

                return ScheduledWithRelease.Live;
            }
        }

        public bool Amendment => PreviousVersionId != null && Published == null;

        public MethodologyVersion CreateMethodologyAmendment(DateTime createdDate, Guid createdByUserId)
        {
            var copy = (MethodologyVersion) MemberwiseClone();

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
            copy.Methodology = null!;
            copy.InternalReleaseNote = null;

            copy.MethodologyContent = MethodologyContent.Clone(createdDate);

            copy.Notes = copy
                .Notes
                .Select(n => n.Clone(copy))
                .ToList();

            return copy;
        }
    }

    public class MethodologyVersionContent
    {
        public Guid MethodologyVersionId { get; set; }
            
        public List<ContentSection> Content { get; set; } = new();

        public List<ContentSection> Annexes { get; set; } = new();

        public MethodologyVersionContent Clone(DateTime createdDate)
        {
            return new MethodologyVersionContent
            {
                Annexes = Annexes
                    .Select(c => c.Clone(createdDate))
                    .ToList(),

                Content = Content
                    .Select(c => c.Clone(createdDate))
                    .ToList()
            };
        }
    }
}


