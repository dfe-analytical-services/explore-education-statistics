#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class Contact
{
    [Key] [Required] public Guid Id { get; set; }

    [Required] public string TeamName { get; set; } = null!;

    [Required] [EmailAddress] public string TeamEmail { get; set; } = null!;

    [Required] public string ContactName { get; set; } = null!;

    public string? ContactTelNo { get; set; }
}
