using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Permalink
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public TableBuilderResultViewModel FullTable { get; set; }

        public TableBuilderQueryContext Query { get; set; }

        public Permalink(TableBuilderResultViewModel result, TableBuilderQueryContext query)
        {
            Id = Guid.NewGuid();
            Created = DateTime.UtcNow;
            FullTable = result;
            Query = query;
        }
    }
}