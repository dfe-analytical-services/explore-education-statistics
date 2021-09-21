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

        public string Content { get; }

        public TimePeriodRangeLabels TimePeriodRange { get; }

        public List<string> GeographicLevels { get; }

        public FileInfo File { get; }

        public SubjectViewModel(
            Guid id,
            string name,
            string content,
            TimePeriodRangeLabels timePeriodRange,
            List<string> geographicLevels,
            FileInfo file)
        {
            Id = id;
            Name = name;
            Content = content;
            TimePeriodRange = timePeriodRange;
            GeographicLevels = geographicLevels;
            File = file;
        }
    }
}
