#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class TableBuilderQueryViewModel : ObservationQueryContext
    {
        public Guid? PublicationId { get; set; }
    }
}
