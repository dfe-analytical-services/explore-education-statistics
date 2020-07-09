using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class CreateDataBlockViewModel
    {
        [Required]
        public string Heading { get; set; }

        [Required]
        public string Name { get; set; }

        public string HighlightName { get; set; }

        public string Source { get; set; }

        public ObservationQueryContext Query { get; set; }

        public List<IContentBlockChart> Charts { get; set; }

        public DataBlockSummary Summary { get; set; }

        public List<TableBuilderConfiguration> Tables { get; set; }
    }
}