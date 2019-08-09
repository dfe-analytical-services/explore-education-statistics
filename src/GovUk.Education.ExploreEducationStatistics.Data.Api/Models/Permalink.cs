using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Permalink
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }
        
        public TableResultViewModel Result { get; set; }

        public PermalinkQueryContext Query { get; set; }

        public Permalink(TableResultViewModel result, PermalinkQueryContext query)
        {
            Id = Guid.NewGuid();
            Created = DateTime.UtcNow;
            Result = result;
            Query = query;
        }
    }
}