#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class Dashboard
{
    public Guid Id { get; set; }

    [MaxLength(255)] public string Title { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(2047)] // @MarkFix length ok?
    public string Description { get; set; } = string.Empty;

    public int Version { get; set; }

    public int Order { get; set; }

    public DateTime? Published { get; set; }

    public DateTime Created { get; set; }

    public Guid CreatedById { get; set; }

    public DateTime? Updated { get; set; }

    public Guid? UpdatedById { get; set; }

    public Guid? ParentDashboardId { get; set; }

    public Dashboard? ParentDashboard { get; set; } // @MarkFix

    public List<Dashboard> ChildDashboards { get; set; } = new(); // @MarkFix
}
