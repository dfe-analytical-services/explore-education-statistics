using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class PermalinkViewModel
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public TableResultViewModel Result { get; set; }

        public PermalinkQueryContext Query { get; set; }
    }
}