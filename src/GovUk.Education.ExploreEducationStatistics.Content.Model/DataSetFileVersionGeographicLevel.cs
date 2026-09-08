#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetFileVersionGeographicLevel
{
    public Guid DataSetFileVersionId { get; set; } // Currently Files.Id, but will become DataSetFileVersion.Id in EES-5105

    public File DataSetFileVersion { get; set; } = null!;

    public GeographicLevel GeographicLevel { get; set; }

    /// <summary>
    /// Null means this row predates the CsvOnly backfill and its value isn't known yet. Reads filter on
    /// `CsvOnly != true`, so null rows behave exactly as false ones do and data sets that haven't been
    /// backfilled keep displaying the geographic levels they always have. Once the backfill is complete,
    /// this will be made non-nullable in EES-7584.
    /// </summary>
    public bool? CsvOnly { get; set; }
}
