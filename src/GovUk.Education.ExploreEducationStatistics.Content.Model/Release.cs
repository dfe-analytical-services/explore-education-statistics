using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;
using static System.DateTime;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PartialDateUtil;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Release
    {
        public Guid Id { get; set; }

        [Required] public string Title { get; set; }

        public string ReleaseName { get; set; }

        /**
         * The last date the release was published - this should be set when the PublishScheduled date is reached and
         * the release is published.
         */
        public DateTime? Published { get; set; }

        // The date that the release is scheduled to be published - when this time is reached then the release should
        // be published and the Published date set.
        public DateTime? PublishScheduled { get; set; }

        [NotMapped] public bool Live => Published.HasValue && (DateTime.Compare(UtcNow, Published.Value) > 0);

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

        private string _nextReleaseDate;

        public string NextReleaseDate
        {
            get => _nextReleaseDate;
            set
            {
                if (PartialDateValid(value))
                {
                    _nextReleaseDate = value;
                }
                throw new FormatException("Must be of the form YYYY-MM-DD where each value can be missing");
            }
        }

    }
}