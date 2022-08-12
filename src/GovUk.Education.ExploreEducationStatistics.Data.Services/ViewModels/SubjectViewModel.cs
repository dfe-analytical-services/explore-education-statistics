#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public record SubjectViewModel
    {
        public Guid Id { get; }

        public string Name { get; }

        public int Order { get; }

        public string Content { get; }

        public TimePeriodLabels TimePeriods { get; }

        public List<string> GeographicLevels { get; }

        public FileInfo File { get; }

        public SubjectViewModel(
            Guid id,
            string name,
            int order,
            string content,
            TimePeriodLabels timePeriods,
            List<string> geographicLevels,
            FileInfo file)
        {
            Id = id;
            Name = name;
            Order = order;
            Content = content;
            TimePeriods = timePeriods;
            GeographicLevels = geographicLevels;
            File = file;
        }
    }
}
