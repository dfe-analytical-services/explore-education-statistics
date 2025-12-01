#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Options;

public class TableBuilderOptions
{
    public const string Section = "TableBuilder";

    public int MaxTableCellsAllowed { get; set; }

    public int CroppedTableMaxRows { get; set; }
}
