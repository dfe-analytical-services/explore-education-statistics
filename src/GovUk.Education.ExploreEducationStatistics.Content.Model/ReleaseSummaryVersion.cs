using System;
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using Newtonsoft.Json;
using static System.DateTime;
using static System.String;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PartialDate;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseSummaryVersion : IVersion
    {
        public Guid Id { get; set; }

        public Guid ReleaseSummaryId { get; set; }

        public ReleaseSummary ReleaseSummary { get; set; }
        
        public string Title => CoverageTitle + (IsNullOrEmpty(YearTitle) ? "" : " " + YearTitle);

        public string YearTitle => TimePeriodLabelFormatter.FormatYear(ReleaseName, TimePeriodCoverage);

        public string CoverageTitle => TimePeriodCoverage.GetEnumLabel();
        
        private string _releaseName;

        public string ReleaseName
        {
            get => _releaseName;
            set
            {
                if (value == null || YearRegex.Match(value).Success)
                {
                    _releaseName = value;
                }
                else
                {
                    throw new FormatException("The release name is invalid");
                }
            }
        }

        // The date that the release is scheduled to be published - when this time is reached then the release should
        // be published and the Published date set.
        public DateTime? PublishScheduled { get; set; }

        public string Slug { get; set; }

        public string Summary { get; set; }
        
        public ReleaseType Type { get; set; }
        
        public Guid TypeId { get; set; }

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier TimePeriodCoverage { get; set; }
        
        private PartialDate _nextReleaseDate;

        public PartialDate NextReleaseDate
        {
            get => _nextReleaseDate;
            set
            {
                if (value == null || value.IsValid())
                {
                    _nextReleaseDate = value;
                }
                else
                {
                    throw new FormatException("The next release date is invalid");
                }
            }
        }

        public DateTime Created { get; set; }
    }
}