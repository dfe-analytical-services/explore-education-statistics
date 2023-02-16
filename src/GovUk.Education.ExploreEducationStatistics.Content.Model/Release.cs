using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Newtonsoft.Json;
using static System.DateTime;
using static GovUk.Education.ExploreEducationStatistics.Common.Database.TimePeriodLabelFormat;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.PartialDate;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Release : Versioned<Release>
    {
        public Guid Id { get; set; }

        public string Title => TimePeriodLabelFormatter.Format(Year, TimePeriodCoverage, FullLabelBeforeYear);

        public int Year => int.Parse(_releaseName);

        public string YearTitle => TimePeriodLabelFormatter.FormatYear(Year, TimePeriodCoverage);

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

        [NotMapped] public bool Live => Published.HasValue && UtcNow >= Published.Value;

        [NotMapped] public bool Amendment => Version > 0 && !Live;

        public string Slug { get; set; }

        public Guid PublicationId { get; set; }

        public Publication Publication { get; set; }

        public List<Update> Updates { get; set; } = new();

        public List<ReleaseStatus> ReleaseStatuses { get; set; } = new();

        public string? LatestInternalReleaseNote
        {
            get
            {
                return ReleaseStatuses?.Count > 0
                    ? ReleaseStatuses.OrderBy(rs => rs.Created).Last().InternalReleaseNote
                    : null;
            }
        }

        [JsonIgnore]
        public List<ReleaseContentSection> Content { get; set; } = new();

        // TODO: EES-1568 This should be DataBlocks
        [JsonIgnore]
        public List<ReleaseContentBlock> ContentBlocks { get; set; } = new();

        public List<KeyStatistic> KeyStatistics { get; set; } = new();

        public string PreReleaseAccessList { get; set; } = string.Empty;

        public string DataGuidance { get; set; } = string.Empty;

        public bool NotifySubscribers { get; set; }

        public DateTime? NotifiedOn { get; set; }

        public bool UpdatePublishedDate { get; set; }

        public Release? PreviousVersion { get; set; }

        public bool SoftDeleted { get; set; }

        [NotMapped]
        [JsonProperty("Content")]
        public IEnumerable<ContentSection> GenericContent
        {
            get
            {
                if (Content == null)
                {
                    return new List<ContentSection>();
                }

                return Content
                    .Where(rcs => rcs.ContentSection.Type == ContentSectionType.Generic)
                    .Select(rcs => rcs.ContentSection)
                    .ToImmutableList();
            }
            set => ReplaceContentSectionsOfType(ContentSectionType.Generic, value);
        }

        public void AddGenericContentSection(ContentSection section)
        {
            Content.Add(new ReleaseContentSection
            {
                Release = this,
                ContentSection = section
            });
        }

        public void RemoveGenericContentSection(ContentSection section)
        {
            Content.Remove(Content.Find(join => join.ContentSection == section));
        }

        public void AddContentBlock(ContentBlock contentBlock)
        {
            if (ContentBlocks == null)
            {
                ContentBlocks = new List<ReleaseContentBlock>();
            }

            ContentBlocks.Add(new ReleaseContentBlock
            {
                Release = this,
                ContentBlock = contentBlock
            });
        }

        [NotMapped]
        public ContentSection KeyStatisticsSecondarySection
        {
            get => FindSingleSectionByType(ContentSectionType.KeyStatisticsSecondary);
            set => ReplaceContentSectionsOfType(ContentSectionType.KeyStatisticsSecondary, new List<ContentSection> { value });
        }

        [NotMapped]
        public ContentSection HeadlinesSection
        {
            get => FindSingleSectionByType(ContentSectionType.Headlines);
            set => ReplaceContentSectionsOfType(ContentSectionType.Headlines, new List<ContentSection> { value });
        }

        [NotMapped]
        public ContentSection SummarySection
        {
            get => FindSingleSectionByType(ContentSectionType.ReleaseSummary);
            set => ReplaceContentSectionsOfType(ContentSectionType.ReleaseSummary, new List<ContentSection> { value });
        }

        [NotMapped]
        public ContentSection RelatedDashboardsSection
        {
            get => FindSingleSectionByType(ContentSectionType.RelatedDashboards);
            set => ReplaceContentSectionsOfType(ContentSectionType.RelatedDashboards, new List<ContentSection> { value });
        }

        private ContentSection FindSingleSectionByType(ContentSectionType type)
        {
            if (Content == null)
            {
                Content = new List<ReleaseContentSection>();
            }

            return Content
                .Select(join => join.ContentSection)
                .ToList()
                .Find(section => section.Type == type);
        }

        private void ReplaceContentSectionsOfType(ContentSectionType type, IEnumerable<ContentSection> replacementSections)
        {
            if (Content == null)
            {
                Content = new List<ReleaseContentSection>();
            }

            Content.RemoveAll(join => join.ContentSection.Type == type);
            Content.AddRange(replacementSections.Select(section => new ReleaseContentSection
            {
                Release = this,
                ContentSection = section,
            }));
        }

        public ReleaseType Type { get; set; }

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier TimePeriodCoverage { get; set; }

        public ReleaseApprovalStatus ApprovalStatus { get; set; }

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
                else if (value.IsEmpty())
                {
                    _nextReleaseDate = null;
                }
                else
                {
                    throw new FormatException("The next release date is invalid");
                }
            }
        }

        public List<Link> RelatedInformation { get; set; } = new();

        public Release Clone()
        {
            return MemberwiseClone() as Release;
        }

        public record CloneContext
        {
            // Maps old content block references to new content blocks
            // Ideally we want to try and get rid of this completely as we
            // shouldn't have to deal with the same content blocks being
            // referenced in multiple places.
            // TODO: EES-1306 may be possible to remove this as part of this ticket
            public Dictionary<ContentBlock, ContentBlock> OriginalToAmendmentContentBlockMap { get; } = new();

            public Release NewRelease { get; }

            public CloneContext(Release newRelease)
            {
                NewRelease = newRelease;
            }
        }
    }
}
