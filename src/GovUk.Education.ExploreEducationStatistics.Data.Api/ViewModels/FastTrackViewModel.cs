using System;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class FastTrackViewModel
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public TableBuilderResultViewModel FullTable { get; set; }
        
        public TableBuilderQueryViewModel Query { get; set; }
    }
}