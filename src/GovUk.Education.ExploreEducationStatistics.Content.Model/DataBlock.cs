#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class DataBlock
    {
        public Guid Id { get; set; }

        public DateTime? Created { get; set; }

        public string Heading { get; set; }

        public string Name { get; set; }

        public string? HighlightName { get; set; }

        public string? HighlightDescription { get; set; }

        public string Source { get; set; }

        public ObservationQueryContext Query { get; set; }

        public List<IChart> Charts { get; set; }

        public DataBlockSummary? Summary { get; set; }

        public TableBuilderConfiguration Table { get; set; }

        public Release Release { get; set; } = null!;

        public Guid ReleaseId { get; set; }

        public ReleaseContentSection? ContentSection { get; set; }

        public Guid? ContentSectionId { get; set; }
    }
}
