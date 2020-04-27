using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations.Temp
{
    public class EES17DataBlock : IContentBlock
    {
        public string Heading { get; set; }

        public string Name { get; set; }

        public string Source { get; set; }

        public EES17ObservationQueryContext DataBlockRequest { get; set; }

        public List<IContentBlockChart> Charts { get; set; }

        public DataBlockSummary Summary { get; set; }

        public List<EES17Table> Tables { get; set; }

        public override string Type { get; set; } = ContentBlockType.DataBlock.ToString();
    }
}