using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    using System;
    using Microsoft.Azure.Cosmos.Table;

    public class PermalinkEntity
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public string Title { get; set; }

        public ResultWithMetaViewModel Data { get; set; }

        public ObservationQueryContext Query { get; set; }
    }
}