#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class Update : ICreatedTimestamp<DateTime?>
{
    [Key] [Required] public Guid Id { get; set; }

    public Guid ReleaseId { get; set; }

    public Release Release { get; set; } = null!;

    // TODO - Can this be non-nullable?
    public DateTime? Created { get; set; }

    // TODO - Can this be non-nullable?
    public User? CreatedBy { get; set; }

    // TODO - Can this be non-nullable?
    public Guid? CreatedById { get; set; }

    [Required] public DateTime On { get; set; }

    [Required] public string Reason { get; set; } = null!;
}