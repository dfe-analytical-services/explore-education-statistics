#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data
{
    public class TableBuilderConfiguration
    {
        public TableHeaders TableHeaders { get; set; } = new();

        public TableBuilderConfiguration Clone()
        {
            return new TableBuilderConfiguration
            {
                TableHeaders = TableHeaders.Clone()
            };
        }
    }
}
