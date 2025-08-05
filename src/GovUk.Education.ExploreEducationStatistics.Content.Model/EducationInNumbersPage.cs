#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class EducationInNumbersPage
{
    public Guid Id { get; set; }

    [MaxLength(255)] public string Title { get; set; } = string.Empty;

    [MaxLength(255)] public string? Slug { get; set; } = string.Empty;

    [MaxLength(2047)] // @MarkFix length ok?
    public string Description { get; set; } = string.Empty;

    public int Version { get; set; }

    public int Order { get; set; }

    public DateTimeOffset? Published { get; set; }

    public DateTimeOffset Created { get; set; }

    public Guid CreatedById { get; set; }

    public DateTimeOffset? Updated { get; set; }

    public Guid? UpdatedById { get; set; }
}
