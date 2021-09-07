#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public record FeaturedTableViewModel
    {
        public Guid Id { get; }

        public string Name { get; }

        public string Description { get; }

        public FeaturedTableViewModel(Guid id, string name, string description = "")
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}