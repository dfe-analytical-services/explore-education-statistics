using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class DataBlockViewModel : IContentBlockViewModel
    {
        public Guid Id { get; set; }

        public List<CommentViewModel> Comments { get; set; }

        public string Heading { get; set; }

        public string Name { get; set; }

        public string HighlightName { get; set; }

        public string Source { get; set; }

        public ObservationQueryContext Query { get; set; }

        public List<IContentBlockChart> Charts { get; set; }

        public int Order { get; set; }

        public DataBlockSummary Summary { get; set; }

        public TableBuilderConfiguration Table { get; set; }

        public ContentBlockType Type => ContentBlockType.DataBlock;
    }
}