using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Permalink
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }
        
        public TableBuilderResultViewModel FullTable { get; set; }

        public PermalinkQueryContext Query { get; set; }

        public Permalink(TableBuilderResultViewModel result, PermalinkQueryContext query)
        {
            Id = Guid.NewGuid();
            Created = DateTime.UtcNow;
            FullTable = result;
            Query = query;
        }
    }
}