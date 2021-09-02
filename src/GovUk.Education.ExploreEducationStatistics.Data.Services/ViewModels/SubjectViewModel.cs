#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public record SubjectViewModel
    {
        public Guid Id { get; }

        public string Name { get; }

        public string Content { get; }

        public TimePeriodLabels TimePeriods { get; }

        public List<string> GeographicLevels { get; }

        public SubjectViewModel(
            Guid id,
            string name,
            string content,
            TimePeriodLabels timePeriods,
            List<string> geographicLevels)
        {
            Id = id;
            Name = name;
            Content = content;
            TimePeriods = timePeriods;
            GeographicLevels = geographicLevels;
        }
    }
}
