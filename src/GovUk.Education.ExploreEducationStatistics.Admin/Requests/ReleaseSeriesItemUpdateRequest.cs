#nullable enable

using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public class ReleaseSeriesItemUpdateRequest
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public int Order { get; set; }

    public bool IsLegacy { get; set; }

    public bool IsAmendment { get; set; }
}

