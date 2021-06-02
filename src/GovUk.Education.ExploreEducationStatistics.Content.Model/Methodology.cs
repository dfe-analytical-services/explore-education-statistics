#nullable enable
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

    public class Methodology
    {
        [Key] 
        [Required] 
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Slug { get; set; }

        public string? Summary { get; set; }

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

        public bool Live => Published.HasValue && DateTime.Compare(DateTime.UtcNow, Published.Value) > 0;
    }
}
