#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetFileVersionGeographicLevel
{
    public Guid DataSetFileVersionId { get; set; } // Currently Files.Id, but will become DataSetFileVersion.Id in EES-5105

    public File DataSetFileVersion { get; set; } = null!;

    public GeographicLevel GeographicLevel { get; set; }
}
