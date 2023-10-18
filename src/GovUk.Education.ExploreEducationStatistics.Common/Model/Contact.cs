#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public class Contact
{
    public Guid Id { get; set; }

    public string TeamName { get; set; } = null!;

    public string TeamEmail { get; set; } = null!;

    public string ContactName { get; set; } = null!;

    public string? ContactTelNo { get; set; }
}