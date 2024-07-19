using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using JsonKnownTypes;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

// EES-4640 - ideally this should be renamed to "DataBlockVersionViewModel" when DataBlock is removed
// from the DataBlock model.
[JsonKnownThisType("DataBlock")]
public class DataBlockViewModel : IContentBlockViewModel
{
    public Guid Id { get; set; }

    public Guid DataBlockParentId { get; set; }

    public int Order { get; set; }

    public string Heading { get; set; }

    public string Name { get; set; }

    public string Source { get; set; }

    public FullTableQuery Query { get; set; }

    private List<IChart> ChartsInternal;

    // TODO EES-3319 - remove backwards-compatibility for Map Configuration without its
    // own Boundary Level selection
    public List<IChart> Charts
    {
        get => ChartsInternal;
        set
        {
            value.ForEach(chart =>
            {
                if (chart is MapChart { BoundaryLevel: null } mapChart)
                {
                    mapChart.BoundaryLevel = Query.BoundaryLevel;
                }
            });
            ChartsInternal = value;
        }
    }

    public TableBuilderConfiguration Table { get; set; }
}
