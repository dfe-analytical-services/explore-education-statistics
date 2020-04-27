using System;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations.Temp
{
    public class EES17Permalink
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public TableBuilderResultViewModel FullTable { get; set; }

        public EES17TableBuilderQueryContext Query { get; set; }
    }
}