using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Migrations.EES17
{
    public class EES17TableHeaders
    {
        public List<List<EES17TableOption>> columnGroups { get; set; }
        public List<EES17TableOption> columns { get; set; }
        public List<List<EES17TableRowGroupOption>> rowGroups { get; set; }
        public List<EES17TableOption> rows { get; set; }
    }
}