#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Logging.Abstractions;
using static System.DateTime;
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
        [Key] 
        [Required] 
        public Guid Id { get; set; }

        public MethodologyStatus Status { get; set; }

        public DateTime? Published { get; set; }

        public DateTime? Updated { get; set; }

        public List<ContentSection> Content { get; set; } = new List<ContentSection>();

        public List<ContentSection> Annexes { get; set; } = new List<ContentSection>();

        public string? InternalReleaseNote { get; set; }

        public MethodologyParent MethodologyParent { get; set; }

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

        public bool ScheduledForPublishingImmediately => PublishingStrategy == MethodologyPublishingStrategy.Immediately;

        public bool ScheduledForPublishingWithPublishedRelease
        {
            get
            {
                if (PublishingStrategy != MethodologyPublishingStrategy.WithRelease)
                {
                    return false;
                }
                
                if (ScheduledWithReleaseId != null && ScheduledWithRelease == null)
                {
                    throw new InvalidOperationException("ScheduledWithRelease field not included in Methodology");
                }
                
                return ScheduledWithRelease.Live;
            }
        }

        public Methodology CreateAmendment(DateTime createdDate, User createdByUser)
        {
            var copy = MemberwiseClone() as Methodology;
            copy.Id = Guid.NewGuid();
            copy.Status = Draft;
            copy.Published = null;
            copy.Updated = null;
            copy.Version = Version++;
            copy.Created = createdDate;
            copy.CreatedBy = createdByUser;
            copy.PreviousVersion = this;
            copy.PublishingStrategy = Immediately;
            copy.ScheduledWithRelease = null;
            copy.ScheduledWithReleaseId = Guid.Empty;
            return copy;
        }
    }
}
