using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;
using static System.DateTime;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PartialDate;
using static System.String;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Release
    {
        public Guid Id { get; set; }

        public string Title => TimePeriodCoverage.GetEnumLabel() + (IsNullOrEmpty(ReleaseName) ? "" : " " + ReleaseName);

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

        /**
         * The last date the release was published - this should be set when the PublishScheduled date is reached and
         * the release is published.
         */
        public DateTime? Published { get; set; }

        // The date that the release is scheduled to be published - when this time is reached then the release should
        // be published and the Published date set.
        public DateTime? PublishScheduled { get; set; }

        [NotMapped] public bool Live => Published.HasValue && (Compare(UtcNow, Published.Value) > 0);

        public string Slug { get; set; }

        public string Summary { get; set; }

        public Guid PublicationId { get; set; }

        public Publication Publication { get; set; }

        public List<Update> Updates { get; set; }

        public List<ContentSection> Content { get; set; }

        public DataBlock KeyStatistics { get; set; }

        public Guid? TypeId { get; set; }

        public ReleaseType Type { get; set; }

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier TimePeriodCoverage { get; set; }

        public int Order { get; set; }

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
    }
}