using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public class TableHighlightViewModel
    {
        public Guid Id { get; }

        public string Name { get; }

        public string Description { get; }

        public TableHighlightViewModel(Guid id, string name, string description = "")
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}