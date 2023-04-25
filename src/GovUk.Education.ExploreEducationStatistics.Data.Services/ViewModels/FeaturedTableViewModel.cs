#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public record FeaturedTableViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Guid SubjectId { get; set; }
    }
}
