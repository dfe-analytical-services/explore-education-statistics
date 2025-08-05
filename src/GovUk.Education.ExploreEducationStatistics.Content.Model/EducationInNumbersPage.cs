#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class EducationInNumbersPage
{
    public Guid Id { get; set; }

    [MaxLength(255)] public string Title { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Slug { get; set; } = string.Empty;

    [MaxLength(2047)] // @MarkFix length ok?
    public string Description { get; set; } = string.Empty;

    public int Version { get; set; }

    public int Order { get; set; }

    public DateTime? Published { get; set; }

    public DateTime Created { get; set; }

    public Guid CreatedById { get; set; }

    public User? CreatedBy { get; set; } // @MarkFix rerun migration cause this was added?

    public DateTime? Updated { get; set; }

    public Guid? UpdatedById { get; set; }

    public User? UpdatedBy { get; set; }
}
