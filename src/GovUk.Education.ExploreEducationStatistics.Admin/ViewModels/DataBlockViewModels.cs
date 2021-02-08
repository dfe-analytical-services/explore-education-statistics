using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using JsonKnownTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    [JsonKnownThisType(nameof(DataBlock))]
    public class DataBlockViewModel : IContentBlockViewModel
    {
        public Guid Id { get; set; }

        public List<CommentViewModel> Comments { get; set; }

        public string Heading { get; set; }

        public string Name { get; set; }

        public string HighlightName { get; set; }

        public string Source { get; set; }

        public ObservationQueryContext Query { get; set; }

        public List<IChart> Charts { get; set; }

        public int Order { get; set; }

        public DataBlockSummary Summary { get; set; }

        public TableBuilderConfiguration Table { get; set; }
    }

    public class DataBlockCreateViewModel
    {
        [Required]
        public string Heading { get; set; }

        [Required]
        public string Name { get; set; }

        public string HighlightName { get; set; }

        public string Source { get; set; }

        public ObservationQueryContext Query { get; set; }

        public List<IChart> Charts { get; set; }

        public DataBlockSummary Summary { get; set; }

        public TableBuilderConfiguration Table { get; set; }
    }

    public class DataBlockUpdateViewModel
    {
        [Required]
        public string Heading { get; set; }

        [Required]
        public string Name { get; set; }

        public string HighlightName { get; set; }

        public string Source { get; set; }

        public ObservationQueryContext Query { get; set; }

        public List<IChart> Charts { get; set; }

        public DataBlockSummary Summary { get; set; }

        public TableBuilderConfiguration Table { get; set; }
    }
}