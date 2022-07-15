using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public record FastTrackViewModel
    {
        public Guid Id { get; set; }

        public TableBuilderConfiguration Configuration { get; set; }

        public TableBuilderResultViewModel FullTable { get; set; }

        public TableBuilderQueryViewModel Query { get; set; }

        public Guid ReleaseId { get; set; }

        public string ReleaseSlug { get; set; }

        public bool LatestData { get; set; }

        public string LatestReleaseTitle { get; set; }
    }
}
