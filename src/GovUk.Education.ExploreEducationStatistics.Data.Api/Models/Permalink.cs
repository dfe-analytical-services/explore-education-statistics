using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Permalink
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public TableBuilderConfiguration Configuration { get; set; }

        public TableBuilderResultViewModel FullTable { get; set; }

        public ObservationQueryContext Query { get; set; }

        public Permalink(TableBuilderConfiguration configuration,
            TableBuilderResultViewModel result,
            ObservationQueryContext query)
        {
            Id = Guid.NewGuid();
            Created = DateTime.UtcNow;
            Configuration = configuration;
            FullTable = result;
            Query = query;
        }
    }
}