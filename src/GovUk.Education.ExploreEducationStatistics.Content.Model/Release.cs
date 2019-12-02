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
using static System.String;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PartialDate;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Release
    {
        public Guid Id { get; set; }

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

        public Guid PublicationId { get; set; }

        public Publication Publication { get; set; }

        public List<Update> Updates { get; set; }

        [JsonIgnore]
        public List<ReleaseContentSection> Content { get; set; }

        [JsonIgnore]
        public List<ReleaseContentBlock> ContentBlocks { get; set; }

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
        
        public void AddContentBlock(IContentBlock contentBlock)
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

        public int Order { get; set; }
        
        public ReleaseStatus Status { get; set; }
        
        public string InternalReleaseNote { get; set; }

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
        
        public List<BasicLink> RelatedInformation { get; set; }
    }
}