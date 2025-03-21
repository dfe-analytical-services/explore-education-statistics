using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Mappings;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using ContentSectionViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ContentSectionViewModel;
using DataBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.DataBlockViewModel;
using EmbedBlockLinkViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.EmbedBlockLinkViewModel;
using HtmlBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.HtmlBlockViewModel;
using IContentBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.IContentBlockViewModel;
using KeyStatisticDataBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.KeyStatisticDataBlockViewModel;
using KeyStatisticTextViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.KeyStatisticTextViewModel;
using KeyStatisticViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.KeyStatisticViewModel;
using MarkDownBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.MarkDownBlockViewModel;
using MethodologyNoteViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology.MethodologyNoteViewModel;
using MethodologyVersionViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology.MethodologyVersionViewModel;
using PublicationViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.PublicationViewModel;
using ReleaseNoteViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent.ReleaseNoteViewModel;
using ReleaseVersionSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ReleaseVersionSummaryViewModel;
using ReleaseVersionViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ReleaseVersionViewModel;
using ThemeViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ThemeViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    /**
     * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
     */
    public class MappingProfiles : CommonMappingProfile
    {
        public MappingProfiles()
        {
            CreateMap<User, UserDetailsViewModel>();

            CreateMap<ReleaseVersion, ReleaseVersionViewModel>()
                .ForMember(dest => dest.ReleaseId,
                    m => m.MapFrom(rv => rv.ReleaseId))
                .ForMember(dest => dest.Slug,
                    m => m.MapFrom(rv => rv.Release.Slug))
                .ForMember(dest => dest.TimePeriodCoverage,
                    m => m.MapFrom(rv => rv.Release.TimePeriodCoverage))
                .ForMember(dest => dest.Title,
                    m => m.MapFrom(rv => rv.Release.Title))
                .ForMember(dest => dest.Year,
                    m => m.MapFrom(rv => rv.Release.Year))
                .ForMember(dest => dest.YearTitle,
                    m => m.MapFrom(rv => rv.Release.YearTitle))
                .ForMember(dest => dest.LatestRelease,
                    m => m.MapFrom(rv => rv.Release.Publication.LatestPublishedReleaseVersionId == rv.Id))
                .ForMember(dest => dest.PublicationTitle,
                    m => m.MapFrom(rv => rv.Release.Publication.Title))
                .ForMember(dest => dest.PublicationId,
                    m => m.MapFrom(rv => rv.Release.Publication.Id))
                .ForMember(dest => dest.PublicationSlug,
                    m => m.MapFrom(rv => rv.Publication.Slug))
                .ForMember(dest => dest.PublishScheduled,
                    m => m.MapFrom(rv =>
                        rv.PublishScheduled.HasValue
                            ? rv.PublishScheduled.Value.ConvertUtcToUkTimeZone()
                    : (DateTime?)null))
                .ForMember(dest => dest.Label,
                    m => m.MapFrom(rv => rv.Release.Label));

            CreateMap<ReleaseVersion, ReleaseVersionSummaryViewModel>()
                .ForMember(dest => dest.Slug,
                    m => m.MapFrom(rv => rv.Release.Slug))
                .ForMember(dest => dest.TimePeriodCoverage,
                    m => m.MapFrom(rv => rv.Release.TimePeriodCoverage))
                .ForMember(dest => dest.Title,
                    m => m.MapFrom(rv => rv.Release.Title))
                .ForMember(dest => dest.Year,
                    m => m.MapFrom(rv => rv.Release.Year))
                .ForMember(dest => dest.YearTitle,
                    m => m.MapFrom(rv => rv.Release.YearTitle))
                .ForMember(dest => dest.PublishScheduled,
                    m => m.MapFrom(model =>
                        model.PublishScheduled.HasValue
                            ? model.PublishScheduled.Value.ConvertUtcToUkTimeZone()
                            : (DateTime?)null));

            CreateMap<ReleasePublishingStatus, ReleasePublishingStatusViewModel>()
                .ForMember(model => model.LastUpdated, m => m.MapFrom(status => status.Timestamp))
                .ForMember(model => model.ReleaseId, m => m.MapFrom(status => status.PartitionKey));

            CreateMap<MethodologyNote, MethodologyNoteViewModel>();

            CreateMap<MethodologyVersion, MethodologyVersionViewModel>()
                .ForMember(dest => dest.ScheduledWithRelease,
                    m => m.Ignore());

            CreateMap<MethodologyVersion, IdTitleViewModel>();

            CreateMap<Theme, IdTitleViewModel>();
            CreateMap<Publication, PublicationSummaryViewModel>();
            CreateMap<Publication, PublicationViewModel>()
                .ForMember(
                    dest => dest.Theme,
                    m => m.MapFrom(p => p.Theme));
            CreateMap<Publication, PublicationCreateViewModel>()
                .ForMember(
                    dest => dest.Theme,
                    m => m.MapFrom(p => p.Theme));

            CreateContentBlockMap();
            CreateMap<DataBlockCreateRequest, DataBlock>()
                .ForMember(dest => dest.Query,
                    m => m.MapFrom(c => c.Query.AsFullTableQuery()));
            CreateMap<DataBlockUpdateRequest, DataBlock>()
                .ForMember(dest => dest.Query,
                    m => m.MapFrom(c => c.Query.AsFullTableQuery()));

            CreateMap<KeyStatisticDataBlock, KeyStatisticDataBlockViewModel>();
            CreateMap<KeyStatisticText, KeyStatisticTextViewModel>();
            CreateMap<KeyStatistic, KeyStatisticViewModel>()
                .IncludeAllDerived();

            CreateMap<KeyStatisticDataBlockCreateRequest, KeyStatisticDataBlock>();
            CreateMap<KeyStatisticTextCreateRequest, KeyStatisticText>();

            CreateMap<FeaturedTable, FeaturedTableViewModel>();
            CreateMap<FeaturedTableCreateRequest, FeaturedTable>();

            CreateMap<Theme, ThemeViewModel>()
                .ForMember(theme => theme.Publications, m => m.MapFrom(t => t.Publications.OrderBy(publication => publication.Title)));

            CreateMap<ContentSection, ContentSectionViewModel>().ForMember(dest => dest.Content,
                m => m.MapFrom(section => section.Content.OrderBy(contentBlock => contentBlock.Order)));

            CreateMap<ReleaseVersion, ManageContentPageViewModel.ReleaseViewModel>()
                .ForMember(dest => dest.CoverageTitle,
                    m => m.MapFrom(rv => rv.Release.TimePeriodCoverage.GetEnumLabel()))
                .ForMember(dest => dest.ReleaseName,
                    m => m.MapFrom(rv => rv.Release.Year.ToString()))
                .ForMember(dest => dest.Slug,
                    m => m.MapFrom(rv => rv.Release.Slug))
                .ForMember(dest => dest.Title,
                    m => m.MapFrom(rv => rv.Release.Title))
                .ForMember(dest => dest.YearTitle,
                    m => m.MapFrom(rv => rv.Release.YearTitle))
                .ForMember(dest => dest.Content,
                    m => m.MapFrom(rv => rv.GenericContent.OrderBy(s => s.Order)))
                .ForMember(dest => dest.KeyStatistics,
                    m => m.MapFrom(rv => rv.KeyStatistics.OrderBy(ks => ks.Order)))
                .ForMember(
                    dest => dest.Updates,
                    m => m.MapFrom(rv => rv.Updates.OrderByDescending(update => update.On)))
                .ForMember(dest => dest.Publication,
                    m => m.MapFrom(rv => new ManageContentPageViewModel.PublicationViewModel
                    {
                        Id = rv.Release.Publication.Id,
                        Title = rv.Release.Publication.Title,
                        Slug = rv.Release.Publication.Slug,
                        Contact = rv.Release.Publication.Contact,
                        ReleaseSeries = new List<ReleaseSeriesItemViewModel>(), // Must be hydrated after mapping
                        ExternalMethodology = rv.Release.Publication.ExternalMethodology != null
                            ? new ExternalMethodology
                            {
                                Title = rv.Release.Publication.ExternalMethodology.Title,
                                Url = rv.Release.Publication.ExternalMethodology.Url
                            }
                            : null
                    }))
                .ForMember(
                    dest => dest.LatestRelease,
                    m => m.MapFrom(rv => rv.Release.Publication.LatestPublishedReleaseVersionId == rv.Id))
                .ForMember(
                    dest => dest.HasPreReleaseAccessList,
                    m => m.MapFrom(rv => !rv.PreReleaseAccessList.IsNullOrEmpty()))
                .ForMember(model => model.PublishScheduled,
                    m => m.MapFrom(rv =>
                        rv.PublishScheduled.HasValue
                            ? rv.PublishScheduled.Value.ConvertUtcToUkTimeZone()
                            : (DateTime?)null));

            CreateMap<Update, ReleaseNoteViewModel>();

            CreateMap<Comment, CommentViewModel>()
                .ForMember(dest => dest.CreatedBy,
                    m => m.MapFrom(comment =>
                        comment.CreatedById == null
                            ? new User
                            {
#pragma warning disable 612
                                FirstName = comment.LegacyCreatedBy ?? "",
#pragma warning restore 612
                                LastName = ""
                            }
                            : comment.CreatedBy));

            CreateMap<ContentSection, ContentSectionViewModel>()
                .ForMember(dest => dest.Content,
                    m => m.MapFrom(section =>
                        section.Content.OrderBy(contentBlock => contentBlock.Order)));

            CreateMap<MethodologyVersion, ManageMethodologyContentViewModel>()
                .ForMember(dest => dest.Content,
                    m => m.MapFrom(methodologyVersion =>
                        methodologyVersion.MethodologyContent.Content.OrderBy(contentSection => contentSection.Order)))
                .ForMember(dest => dest.Annexes,
                    m => m.MapFrom(methodologyVersion =>
                        methodologyVersion.MethodologyContent.Annexes.OrderBy(annexSection => annexSection.Order)))
                .ForMember(dest => dest.Notes,
                    m => m.MapFrom(methodologyVersion =>
                        methodologyVersion.Notes.OrderByDescending(note => note.DisplayDate)));

            CreateMap<ReleaseVersion, ReleasePublicationStatusViewModel>();

            CreateMap<DataSetVersion, DataSetVersionInfoViewModel>()
                .ForMember(dest => dest.Version,
                    m => m.MapFrom(dataSetVersion =>
                        dataSetVersion.PublicVersion))
                .ForMember(dest => dest.Type,
                    m => m.MapFrom(dataSetVersion =>
                        dataSetVersion.VersionType));
        }

        private void CreateContentBlockMap()
        {
            CreateMap<ContentBlock, IContentBlockViewModel>()
                .IncludeAllDerived()
                .ForMember(dest => dest.Comments,
                    m => m.MapFrom(block => block.Comments.OrderBy(comment => comment.Created)));

            // EES-4640 - we include an AfterMap configuration here to ensure that any time we create a
            // DataBlockViewModel from a plain DataBlock, we also include the DataBlockParentId on the
            // destination DataBlockViewModel that the DataBlock itself does not contain. When DataBlock is
            // removed from the ContentBlock model, this can go too.
            CreateMap<DataBlock, DataBlockViewModel>()
                .AfterMap<DataBlockViewModelPostMappingAction>();

            CreateMap<DataBlockVersion, DataBlockViewModel>();

            CreateMap<EmbedBlockLink, EmbedBlockLinkViewModel>()
                .ForMember(dest => dest.Title,
                    m => m.MapFrom(embedBlockLink =>
                        embedBlockLink.EmbedBlock.Title))
                .ForMember(dest => dest.Url,
                    m => m.MapFrom(embedBlockLink =>
                        embedBlockLink.EmbedBlock.Url));

            CreateMap<HtmlBlock, HtmlBlockViewModel>();

            CreateMap<MarkDownBlock, MarkDownBlockViewModel>();
        }
    }
}
