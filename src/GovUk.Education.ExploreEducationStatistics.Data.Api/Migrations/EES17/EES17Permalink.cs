using System;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations.EES17
{
    public class EES17Permalink
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public TableBuilderResultViewModel FullTable { get; set; }

        public EES17TableBuilderQueryContext Query { get; set; }
    }
}