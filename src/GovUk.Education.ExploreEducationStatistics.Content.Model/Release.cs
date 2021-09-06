using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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

        [NotMapped] public bool Live => Published.HasValue && (Compare(UtcNow, Published.Value) > 0);

        [NotMapped] public bool Amendment => Version > 0 && !Live;

        public string Slug { get; set; }

        public Guid PublicationId { get; set; }

        public Publication Publication { get; set; }

        public List<Update> Updates { get; set; }

        public List<ReleaseStatus> ReleaseStatuses { get; set; }

        public bool NotifySubscribers
        {
            get
            {
                return ReleaseStatuses.Any(rs => rs.ApprovalStatus == ReleaseApprovalStatus.Approved)
                       && ReleaseStatuses
                           .Where(rs => rs.ApprovalStatus == ReleaseApprovalStatus.Approved)
                           .OrderBy(rs => rs.Created)
                           .Last()
                           .NotifySubscribers;
            }
        }

        public string LatestInternalReleaseNote
        {
            get
            {
                return ReleaseStatuses?.Count > 0
                    ? ReleaseStatuses.OrderBy(rs => rs.Created).Last().InternalReleaseNote
                    : null;
            }
        }

        [JsonIgnore]
        public List<ReleaseContentSection> Content { get; set; }

        // TODO: EES-1568 This should be DataBlocks
        [JsonIgnore]
        public List<ReleaseContentBlock> ContentBlocks { get; set; }

        public string PreReleaseAccessList { get; set; } = string.Empty;

        public string MetaGuidance { get; set; } = string.Empty;

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
                    .Select(join => join.ContentSection)
                    .ToList()
                    .FindAll(section => section.Type == ContentSectionType.Generic)
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
        public ContentSection KeyStatisticsSection
        {
            get => FindSingleSectionByType(ContentSectionType.KeyStatistics);
            set => ReplaceContentSectionsOfType(ContentSectionType.KeyStatistics, new List<ContentSection> { value });
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

        public Guid? TypeId { get; set; }

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
                else
                {
                    throw new FormatException("The next release date is invalid");
                }
            }
        }

        public List<Link> RelatedInformation { get; set; }

        public DateTime? DataLastPublished { get; set; }

        public Release CreateReleaseAmendment(DateTime createdDate, Guid createdByUserId)
        {
            var amendment = MemberwiseClone() as Release;

            // Set new values for fields that should be altered in the amended
            // Release rather than copied from the original Release
            amendment.Id = Guid.NewGuid();
            amendment.Published = null;
            amendment.PublishScheduled = null;
            amendment.ApprovalStatus = ReleaseApprovalStatus.Draft;
            amendment.Created = createdDate;
            amendment.CreatedById = createdByUserId;
            amendment.Version = Version + 1;
            amendment.PreviousVersionId = Id;

            var context = new CloneContext();

            amendment.Content = amendment
                .Content?
                .Select(content => content.Clone(amendment, context))
                .ToList();

            amendment.ContentBlocks = amendment
                .ContentBlocks?
                .Select(releaseContentBlock => releaseContentBlock.Clone(amendment, context))
                .ToList();

            amendment.RelatedInformation = amendment
                .RelatedInformation?
                .Select(link => link.Clone())
                .ToList();

            amendment.Updates = amendment
                .Updates?
                .Select(update => update.Clone(amendment))
                .ToList();

            UpdateAmendmentContent(context);

            return amendment;
        }

        // Bit cheeky to re-use the clone context, but it's a nice
        // easy way to access and modify all of the content blocks
        // that we used during the clone.
        private static void UpdateAmendmentContent(CloneContext context)
        {
            var dataBlocks = context.ContentBlocks
                .Where(pair => pair.Key is DataBlock && pair.Value is DataBlock)
                .ToDictionary(pair => pair.Key as DataBlock, pair => pair.Value as DataBlock);

            foreach (var contentBlock in context.ContentBlocks.Values)
            {
                switch (contentBlock)
                {
                    case HtmlBlock block:
                        block.Body = UpdateFastTrackLinks(block.Body, dataBlocks);
                        break;
                    case MarkDownBlock block:
                        block.Body = UpdateFastTrackLinks(block.Body, dataBlocks);
                        break;
                }
            }
        }

        private static string UpdateFastTrackLinks(string content, Dictionary<DataBlock, DataBlock> dataBlocks)
        {
            if (content.IsNullOrEmpty())
            {
                return content;
            }

            var nextContent = content;

            foreach (var (oldDataBlock, newDataBlock) in dataBlocks)
            {
                // Not a particularly fast way to replace fast tracks
                // if there's a lot of content. Could be improved
                // using something like a parallel substring approach.
                nextContent = nextContent.Replace(
                    $"/fast-track/{oldDataBlock.Id}",
                    $"/fast-track/{newDataBlock.Id}"
                );
            }

            return nextContent;
        }

        public void CreateGenericContentFromTemplate(Release newRelease)
        {
            var context = new CloneContext();

            newRelease.Content = Content.Where(c => c.ContentSection.Type == ContentSectionType.Generic).ToList();

            newRelease.Content = newRelease
                .Content?
                .Select(content => content.Clone(newRelease, context))
                .ToList();

            newRelease.ContentBlocks = newRelease
                .ContentBlocks?
                .Select(releaseContentBlock => releaseContentBlock.Clone(newRelease, context))
                .ToList();
        }

        // Ideally we want to try and get rid of this completely as we
        // shouldn't have to deal with the same content blocks being
        // referenced in multiple places.
        // TODO: EES-1306 may be possible to remove this as part of this ticket
        public class CloneContext
        {
            // Maps references to old content blocks to new content blocks
            public Dictionary<ContentBlock, ContentBlock> ContentBlocks { get; } = new Dictionary<ContentBlock, ContentBlock>();
        }
    }
}
