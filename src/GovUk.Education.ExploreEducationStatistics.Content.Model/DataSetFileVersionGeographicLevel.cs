#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetFileVersionGeographicLevel
{
    // Currently Files.Id, but will become DataSetFileVersion.Id in EES-5105.
    // When the foreign key is repointed to DataSetFileVersions it must keep cascading on delete, as it does from Files.
    public Guid DataSetFileVersionId { get; set; }

    public File DataSetFileVersion { get; set; } = null!;

    public GeographicLevel GeographicLevel { get; set; }

    public bool CsvOnly { get; set; }
}
