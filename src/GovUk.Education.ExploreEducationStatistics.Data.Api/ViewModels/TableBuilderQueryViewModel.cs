using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class TableBuilderQueryViewModel : TableBuilderQueryContext
    {
        public Guid PublicationId { get; set; }
    }
}