#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class DashboardViewModel : ICreatedUpdatedTimestamps<DateTime, DateTime?>
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Version { get; set; }

    public int Order { get; set; }

    public DateTime? Published { get; set; }

    public DateTime Created { get; set; }

    public Guid CreatedById { get; set; }

    public User CreatedBy { get; set; } = null!:

    public DateTime? Updated { get; set; }

    public Guid? UpdatedById { get; set; }

    public Guid? ParentDashboardId { get; set; }

    public List<DashboardViewModel> ChildDashboards { get; set; } = new();
}
