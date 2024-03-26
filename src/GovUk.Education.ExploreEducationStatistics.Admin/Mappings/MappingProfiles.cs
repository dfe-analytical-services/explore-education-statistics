using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Mappings;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
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
using ReleaseSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ReleaseSummaryViewModel;
using ReleaseViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ReleaseViewModel;
using ThemeViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ThemeViewModel;
using TopicViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.TopicViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings;

/**
 * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
 */
public class MappingProfiles : CommonMappingProfile
{
    public MappingProfiles()
    {
        CreateMap<User, UserDetailsViewModel>();

        CreateMap<ReleaseVersion, ReleaseViewModel>()
            .ForMember(
                dest => dest.LatestRelease,
                m => m.MapFrom(rv => rv.Publication.LatestPublishedReleaseVersionId == rv.Id))
            .ForMember(dest => dest.PublicationTitle,
                m => m.MapFrom(rv => rv.Publication.Title))
            .ForMember(dest => dest.PublicationId,
                m => m.MapFrom(rv => rv.Publication.Id))
            .ForMember(dest => dest.PublicationSlug,
                m => m.MapFrom(rv => rv.Publication.Slug))
            .ForMember(model => model.PublishScheduled,
                m => m.MapFrom(rv =>
                    rv.PublishScheduled.HasValue
                        ? rv.PublishScheduled.Value.ConvertUtcToUkTimeZone()
                        : (DateTime?)null));

        CreateMap<ReleaseVersion, ReleaseSummaryViewModel>()
            .ForMember(model => model.PublishScheduled,
                m => m.MapFrom(model =>
                    model.PublishScheduled.HasValue
                        ? model.PublishScheduled.Value.ConvertUtcToUkTimeZone()
                        : (DateTime?)null));

        CreateMap<ReleasePublishingStatus, ReleasePublishingStatusViewModel>()
            .ForMember(model => model.LastUpdated, m => m.MapFrom(status => status.Timestamp));

        CreateMap<MethodologyNote, MethodologyNoteViewModel>();

        CreateMap<MethodologyVersion, MethodologyVersionViewModel>()
            .ForMember(dest => dest.ScheduledWithRelease,
                m => m.Ignore());

        CreateMap<MethodologyVersion, IdTitleViewModel>();

        CreateMap<Theme, IdTitleViewModel>();
        CreateMap<Topic, IdTitleViewModel>();
        CreateMap<Publication, PublicationSummaryViewModel>();
        CreateMap<Publication, PublicationViewModel>()
            .ForMember(
                dest => dest.Theme,
                m => m.MapFrom(p => p.Topic.Theme));
        CreateMap<Publication, PublicationCreateViewModel>()
            .ForMember(
                dest => dest.Theme,
                m => m.MapFrom(p => p.Topic.Theme));

        CreateContentBlockMap();
        CreateMap<DataBlockCreateViewModel, DataBlock>();
        CreateMap<DataBlockUpdateViewModel, DataBlock>();

        CreateMap<KeyStatisticDataBlock, KeyStatisticDataBlockViewModel>();
        CreateMap<KeyStatisticText, KeyStatisticTextViewModel>();
        CreateMap<KeyStatistic, KeyStatisticViewModel>()
            .IncludeAllDerived();

        CreateMap<KeyStatisticDataBlockCreateRequest, KeyStatisticDataBlock>();
        CreateMap<KeyStatisticTextCreateRequest, KeyStatisticText>();

        CreateMap<FeaturedTable, FeaturedTableViewModel>();
        CreateMap<FeaturedTableCreateRequest, FeaturedTable>();

        CreateMap<Theme, ThemeViewModel>()
            .ForMember(theme => theme.Topics, m => m.MapFrom(t => t.Topics.OrderBy(topic => topic.Title)));
        CreateMap<Topic, TopicViewModel>();

        CreateMap<ContentSection, ContentSectionViewModel>().ForMember(dest => dest.Content,
            m => m.MapFrom(section => section.Content.OrderBy(contentBlock => contentBlock.Order)));

        CreateMap<ReleaseVersion, ManageContentPageViewModel.ReleaseViewModel>()
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
                    Id = rv.Publication.Id,
                    Title = rv.Publication.Title,
                    Slug = rv.Publication.Slug,
                    Contact = rv.Publication.Contact,
                    Topic = new ManageContentPageViewModel.TopicViewModel
                    {
                        Theme = new ManageContentPageViewModel.ThemeViewModel
                        {
                            Title = rv.Publication.Topic.Theme.Title
                        }
                    },
                    Releases = rv.Publication.ReleaseVersions
                        .FindAll(otherReleaseVersion => rv.Id != otherReleaseVersion.Id &&
                                                 IsLatestVersionOfRelease(rv.Publication.ReleaseVersions, otherReleaseVersion.Id))
                        .OrderByDescending(otherReleaseVersion => otherReleaseVersion.Year)
                        .ThenByDescending(otherReleaseVersion => otherReleaseVersion.TimePeriodCoverage)
                        .Select(otherReleaseVersion => new PreviousReleaseViewModel
                        {
                            Id = otherReleaseVersion.Id,
                            Slug = otherReleaseVersion.Slug,
                            Title = otherReleaseVersion.Title,
                        })
                        .ToList(),
                    ReleaseSeries = new List<ReleaseSeriesItemViewModel>(), // Must be hydrated after mapping
                    ExternalMethodology = rv.Publication.ExternalMethodology != null
                        ? new ExternalMethodology
                        {
                            Title = rv.Publication.ExternalMethodology.Title,
                            Url = rv.Publication.ExternalMethodology.Url
                        }
                        : null
                }))
            .ForMember(
                dest => dest.LatestRelease,
                m => m.MapFrom(rv => rv.Publication.LatestPublishedReleaseVersionId == rv.Id))
            .ForMember(dest => dest.CoverageTitle,
                m => m.MapFrom(rv => rv.TimePeriodCoverage.GetEnumLabel()))
            .ForMember(
                dest => dest.HasPreReleaseAccessList,
                m => m.MapFrom(rv => !rv.PreReleaseAccessList.IsNullOrEmpty()))
            .ForMember(model => model.PublishScheduled,
                m => m.MapFrom(rv =>
                    rv.PublishScheduled.HasValue
                        ? rv.PublishScheduled.Value.ConvertUtcToUkTimeZone()
                        : (DateTime?) null));

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

    private static bool IsLatestVersionOfRelease(IEnumerable<ReleaseVersion> releaseVersions, Guid releaseVersionId)
    {
        return !releaseVersions.Any(rv => rv.PreviousVersionId == releaseVersionId && rv.Id != releaseVersionId);
    }
}
