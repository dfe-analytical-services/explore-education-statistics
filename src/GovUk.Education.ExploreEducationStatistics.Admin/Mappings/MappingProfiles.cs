using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using ContactViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ContactViewModel;
using ContentSectionViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ContentSectionViewModel;
using DataBlockSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.DataBlockSummaryViewModel;
using DataBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.DataBlockViewModel;
using HtmlBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.HtmlBlockViewModel;
using IContentBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.IContentBlockViewModel;
using LegacyReleaseViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.LegacyReleaseViewModel;
using MarkDownBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.MarkDownBlockViewModel;
using MethodologyNoteViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology.MethodologyNoteViewModel;
using MethodologyVersionViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology.MethodologyVersionViewModel;
using PublicationViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.PublicationViewModel;
using ReleaseNoteViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent.ReleaseNoteViewModel;
using ReleaseSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ReleaseSummaryViewModel;
using ReleaseViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ReleaseViewModel;
using ThemeViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ThemeViewModel;
using TopicViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.TopicViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    /**
     * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
     */
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<User, UserDetailsViewModel>();

            CreateMap<Release, ReleaseViewModel>()
                .ForMember(
                    dest => dest.LatestRelease,
                    m => m.MapFrom(r => r.Publication.LatestPublishedReleaseId == r.Id))
                .ForMember(dest => dest.Contact,
                    m => m.MapFrom(r => r.Publication.Contact))
                .ForMember(dest => dest.PublicationTitle,
                    m => m.MapFrom(r => r.Publication.Title))
                .ForMember(dest => dest.PublicationSummary,
                    m => m.MapFrom(r => r.Publication.Summary))
                .ForMember(dest => dest.PublicationId,
                    m => m.MapFrom(r => r.Publication.Id))
                .ForMember(dest => dest.PublicationSlug,
                    m => m.MapFrom(r => r.Publication.Slug))
                .ForMember(model => model.PublishScheduled,
                    m => m.MapFrom(model =>
                        model.PublishScheduled.HasValue
                            ? model.PublishScheduled.Value.ConvertUtcToUkTimeZone()
                            : (DateTime?)null));

            CreateMap<ReleaseCreateRequest, Release>()
                .ForMember(dest => dest.PublishScheduled,
                    m => m.MapFrom(model => model.PublishScheduledDate))
                .ForMember(dest => dest.ReleaseName,
                    m => m.MapFrom(r => r.Year.ToString()));

            CreateMap<Release, ReleaseSummaryViewModel>()
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
            CreateMap<Publication, PublicationViewModel>()
                .ForMember(
                    dest => dest.Theme,
                    m => m.MapFrom(p => p.Topic.Theme));
            CreateMap<Publication, PublicationCreateViewModel>()
                .ForMember(
                    dest => dest.Theme,
                    m => m.MapFrom(p => p.Topic.Theme));

            CreateMap<Contact, ContactViewModel>();

            CreateContentBlockMap();
            CreateMap<DataBlockCreateViewModel, DataBlock>();
            CreateMap<DataBlockUpdateViewModel, DataBlock>();
            CreateMap<DataBlock, DataBlockSummaryViewModel>()
                .ForMember(
                    dest => dest.ChartsCount,
                    m => m.MapFrom(d => d.Charts.Count));

            CreateMap<Release, Data.Model.Release>()
                .ForMember(dest => dest.TimeIdentifier, m => m.MapFrom(r => r.TimePeriodCoverage));

            CreateMap<Theme, ThemeViewModel>()
                .ForMember(theme => theme.Topics, m => m.MapFrom(t => t.Topics.OrderBy(topic => topic.Title)));
            CreateMap<Topic, TopicViewModel>();

            CreateMap<ContentSection, ContentSectionViewModel>().ForMember(dest => dest.Content,
                m => m.MapFrom(section => section.Content.OrderBy(contentBlock => contentBlock.Order)));

            CreateMap<Release, ManageContentPageViewModel.ReleaseViewModel>()
                .ForMember(dest => dest.Content,
                    m => m.MapFrom(r => r.GenericContent.OrderBy(s => s.Order)))
                .ForMember(
                    dest => dest.Updates,
                    m => m.MapFrom(r => r.Updates.OrderByDescending(update => update.On)))
                .ForMember(dest => dest.Publication,
                    m => m.MapFrom(r => new ManageContentPageViewModel.PublicationViewModel
                    {
                        Id = r.Publication.Id,
                        Title = r.Publication.Title,
                        Slug = r.Publication.Slug,
                        Contact = r.Publication.Contact,
                        Topic = new ManageContentPageViewModel.TopicViewModel
                        {
                            Theme = new ManageContentPageViewModel.ThemeViewModel
                            {
                                Title = r.Publication.Topic.Theme.Title
                            }
                        },
                        Releases = r.Publication.Releases
                            .FindAll(otherRelease => r.Id != otherRelease.Id &&
                                                     IsLatestVersionOfRelease(r.Publication.Releases, otherRelease.Id))
                            .OrderByDescending(otherRelease => otherRelease.Year)
                            .ThenByDescending(otherRelease => otherRelease.TimePeriodCoverage)
                            .Select(otherRelease => new PreviousReleaseViewModel
                            {
                                Id = otherRelease.Id,
                                Slug = otherRelease.Slug,
                                Title = otherRelease.Title,
                            })
                            .ToList(),
                        LegacyReleases = r.Publication.LegacyReleases
                            .OrderByDescending(legacyRelease => legacyRelease.Order)
                            .Select(legacy => new ManageContentPageViewModel.LegacyReleaseViewModel
                            {
                                Id = legacy.Id,
                                Description = legacy.Description,
                                Url = legacy.Url,
                            })
                            .ToList(),
                        ExternalMethodology = r.Publication.ExternalMethodology != null
                            ? new ExternalMethodology
                            {
                                Title = r.Publication.ExternalMethodology.Title,
                                Url = r.Publication.ExternalMethodology.Url
                            }
                            : null
                    }))
                .ForMember(
                    dest => dest.LatestRelease,
                    m => m.MapFrom(r => r.Publication.LatestPublishedReleaseId == r.Id))
                .ForMember(dest => dest.CoverageTitle,
                    m => m.MapFrom(r => r.TimePeriodCoverage.GetEnumLabel()))
                .ForMember(
                    dest => dest.HasPreReleaseAccessList,
                    m => m.MapFrom(r => !r.PreReleaseAccessList.IsNullOrEmpty()))
                .ForMember(model => model.PublishScheduled,
                    m => m.MapFrom(model =>
                        model.PublishScheduled.HasValue
                            ? model.PublishScheduled.Value.ConvertUtcToUkTimeZone()
                            : (DateTime?) null));

            CreateMap<Update, ReleaseNoteViewModel>();

            CreateMap<LegacyRelease, LegacyReleaseViewModel>();

            CreateMap<Comment, CommentViewModel>()
                .ForMember(dest => dest.CreatedBy,
                    m => m.MapFrom(comment =>
                        comment.CreatedById == null
                            ? new User
                            {
#pragma warning disable 612
                                FirstName = comment.LegacyCreatedBy,
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

            CreateMap<Release, ReleasePublicationStatusViewModel>();
        }

        private void CreateContentBlockMap()
        {
            CreateMap<ContentBlock, IContentBlockViewModel>()
                .IncludeAllDerived()
                .ForMember(dest => dest.Comments,
                    m => m.MapFrom(block => block.Comments.OrderBy(comment => comment.Created)));

            CreateMap<DataBlock, DataBlockViewModel>();

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

        private static bool IsLatestVersionOfRelease(IEnumerable<Release> releases, Guid releaseId)
        {
            return !releases.Any(r => r.PreviousVersionId == releaseId && r.Id != releaseId);
        }
    }
}
