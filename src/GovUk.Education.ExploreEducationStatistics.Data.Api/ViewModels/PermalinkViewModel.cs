using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class PermalinkViewModel
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public bool Invalidated { get; set; }

        public TableBuilderConfiguration Configuration { get; set; }

        public TableBuilderResultViewModel FullTable { get; set; }
    }
}
