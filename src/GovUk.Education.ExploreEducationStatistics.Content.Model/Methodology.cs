using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        [Required]
        public string Title { get; set; }

        public string Slug { get; set; }

        public string Summary { get; set; }

        public MethodologyStatus Status { get; set; }

        public DateTime? Published { get; set; }

        public DateTime? Updated { get; set; }

        public List<ContentSection> Content { get; set; }

        public List<ContentSection> Annexes { get; set; }

        public List<Publication> Publications { get; set; }

        public string InternalReleaseNote { get; set; }
        
        public MethodologyPublishingStrategy PublishingStrategy { get; set; }

        public Release ScheduledWithRelease { get; set; }
        
        public Guid ScheduledWithReleaseId { get; set; }

        public bool Approved => Status == MethodologyStatus.Approved;

        public bool Live => Published.HasValue && DateTime.Compare(DateTime.UtcNow, Published.Value) > 0;

        public bool ScheduledForPublishingImmediately => PublishingStrategy == MethodologyPublishingStrategy.Immediately;
        public bool ScheduledForPublishingWithPublishedRelease => PublishingStrategy == MethodologyPublishingStrategy.WithRelease
            && ScheduledWithRelease.Live;

        public bool PubliclyAccessible => Approved &&
                                          (ScheduledForPublishingImmediately ||
                                           ScheduledForPublishingWithPublishedRelease);
    }
}