#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class Permalink : ICreatedTimestamp<DateTime>
{
    public Guid Id { get; init; }

    public string PublicationTitle { get; init; }

    public string DataSetTitle { get; init; }

    public Guid? ReleaseId { get; init; }

    public Guid SubjectId { get; init; }

    public int CountFilterItems { get; set; }

    public int CountFootnotes { get; set; }

    public int CountIndicators { get; set; }

    public int CountLocations { get; set; }

    public int CountObservations { get; set; }

    public int CountTimePeriods { get; set; }

    /// <summary>
    /// Content length in bytes of the legacy Permalink in blob storage 
    /// </summary>
    public long? LegacyContentLength { get; set; }

    /// <summary>
    /// True if the legacy Permalink in blob storage has table configuration headers
    /// </summary>
    public bool? LegacyHasConfigurationHeaders { get; set; }

    public DateTime Created { get; set; }
}
