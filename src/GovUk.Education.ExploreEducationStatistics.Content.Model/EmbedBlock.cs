#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class EmbedBlock : ICreatedUpdatedTimestamps<DateTime, DateTime?>
{
    public Guid Id { get; set; }

    [Required] public string Title { get; set; }

    [Required] public string Url { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }

    public EmbedBlock Clone()
    {
        var copy = MemberwiseClone() as EmbedBlock;
        copy.Id = Guid.NewGuid();
        return copy;
    }
}
