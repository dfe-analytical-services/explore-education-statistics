#nullable enable
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataBlockVersion : ICreatedUpdatedTimestamps<DateTime, DateTime?>
{
    public Guid Id { get; set; }

    public Guid DataBlockId { get; set; }

    public DataBlock DataBlock { get; set; } = null!;

    public Guid ReleaseVersionId { get; set; }

    public ReleaseVersion ReleaseVersion { get; set; } = null!;

    public int Version { get; set; }

    public DateTime? Published { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }

    public string Heading { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Source { get; set; }

    public FullTableQuery Query { get; set; }

    [JsonIgnore]
    public List<IChart> Charts
    {
        get
        {
            return ChartsInternal
                .Select(chart =>
                {
                    if (chart.Title.IsNullOrEmpty())
                    {
                        chart.Title = Heading;
                    }
                    return chart;
                })
                .ToList();
        }
        set => ChartsInternal = value;
    }

    // NOTE: We serialize ChartsInternal into JSON rather than Charts so that a chart title is set to null in the
    // database JSON. If we serialized Charts, then the serialization would run through Chart's getter, and so set
    // a chart title that is identical to the table heading. So to keep the database-stored chart JSON pure, we set
    // this JsonProperty, preventing the need to migrate existing charts, and Chart's getter will provide the table
    // heading in request responses when necessary.
    [JsonProperty("Charts")]
    [NotMapped]
    private List<IChart> ChartsInternal { get; set; } = new();

    public TableBuilderConfiguration Table { get; set; }
}
