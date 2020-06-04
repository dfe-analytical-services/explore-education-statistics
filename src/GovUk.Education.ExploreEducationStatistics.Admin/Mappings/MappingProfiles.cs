using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using ApiTopicViewModel = GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.TopicViewModel;
using ManageContentTopicViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent.TopicViewModel;
using PublicationViewModel = GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.PublicationViewModel;
using ReleaseStatus = GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatus;
using ReleaseViewModel = GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.ReleaseViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    /**
     * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
     */
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Release, ReleaseViewModel>()
                .ForMember(
                    dest => dest.LatestRelease,
                    m => m.MapFrom(r => r.Publication.LatestRelease().Id == r.Id))
                .ForMember(dest => dest.Contact,
                    m => m.MapFrom(r => r.Publication.Contact))
                .ForMember(dest => dest.PublicationTitle,
                    m => m.MapFrom(r => r.Publication.Title))
                .ForMember(dest => dest.PublicationId,
                    m => m.MapFrom(r => r.Publication.Id));

            CreateMap<Release, MyReleaseViewModel>()
                .ForMember(
                    dest => dest.LatestRelease,
                    m => m.MapFrom(r => r.Publication.LatestRelease().Id == r.Id))
                .ForMember(dest => dest.Contact,
                    m => m.MapFrom(r => r.Publication.Contact))
                .ForMember(dest => dest.PublicationTitle,
                    m => m.MapFrom(r => r.Publication.Title))
                .ForMember(dest => dest.PublicationId,
                    m => m.MapFrom(r => r.Publication.Id))
                .ForMember(dest => dest.Permissions, exp => exp.MapFrom<IMyReleasePermissionSetPropertyResolver>());

            CreateMap<Release, ReleaseSummaryViewModel>();

            CreateMap<ReleaseSummaryViewModel, Release>();

            CreateMap<CreateReleaseViewModel, Release>()
                .ForMember(dest => dest.PublishScheduled, m => m.MapFrom(model =>
                    model.PublishScheduled.HasValue ? model.PublishScheduled.Value.AsStartOfDayUtc() : (DateTime?) null));

            CreateMap<ReleaseStatus, ReleaseStatusViewModel>()
                .ForMember(model => model.LastUpdated, m => m.MapFrom(status => status.Timestamp));

            CreateMap<Methodology, MethodologySummaryViewModel>();
            CreateMap<Methodology, MethodologyTitleViewModel>();
            CreateMap<Methodology, MethodologyPublicationsViewModel>();

            CreateMap<Publication, IdTitlePair>();

            CreateMap<Publication, PublicationViewModel>()
                .ForMember(dest => dest.Releases,
                    m => m.MapFrom(p => p.Releases
                        .FindAll(r => !r.SoftDeleted && IsLatestVersionOfRelease(p.Releases, r.Id))))
                .ForMember(
                    dest => dest.ThemeId,
                    m => m.MapFrom(p => p.Topic.ThemeId));

            CreateMap<Publication, MyPublicationViewModel>()
                .ForMember(
                    dest => dest.ThemeId,
                    m => m.MapFrom(p => p.Topic.ThemeId))
                .ForMember(dest => dest.Releases,
                    m => m.MapFrom(p => p.Releases
                        .FindAll(r => !r.SoftDeleted && IsLatestVersionOfRelease(p.Releases, r.Id))))
                .ForMember(dest => dest.Permissions, exp => exp.MapFrom<IMyPublicationPermissionSetPropertyResolver>());

            CreateMap<DataBlock, DataBlockViewModel>();
            CreateMap<CreateDataBlockViewModel, DataBlock>();
            CreateMap<UpdateDataBlockViewModel, DataBlock>();

            CreateMap<Topic, ApiTopicViewModel>();

            CreateMap<Release, ViewModels.ManageContent.ReleaseViewModel>()
                .ForMember(dest => dest.Content,
                    m => m.MapFrom(r =>
                        r.GenericContent
                            .Select(ContentSectionViewModel.ToViewModel)
                            .OrderBy(s => s.Order)))
                .ForMember(dest => dest.SummarySection,
                    m => m.MapFrom(r =>
                        ContentSectionViewModel.ToViewModel(r.SummarySection)))
                .ForMember(dest => dest.HeadlinesSection,
                    m => m.MapFrom(r =>
                        ContentSectionViewModel.ToViewModel(r.HeadlinesSection)))
                .ForMember(dest => dest.KeyStatisticsSection,
                    m => m.MapFrom(r =>
                        ContentSectionViewModel.ToViewModel(r.KeyStatisticsSection)))
                .ForMember(dest => dest.KeyStatisticsSecondarySection,
                    m => m.MapFrom(r =>
                        ContentSectionViewModel.ToViewModel(r.KeyStatisticsSecondarySection)))
                .ForMember(
                    dest => dest.Updates,
                    m => m.MapFrom(r => r.Updates.OrderByDescending(update => update.On)))
                .ForMember(dest => dest.Publication,
                    m => m.MapFrom(r => new ViewModels.ManageContent.PublicationViewModel
                    {
                        Id = r.Publication.Id,
                        Description = r.Publication.Description,
                        Title = r.Publication.Title,
                        Slug = r.Publication.Slug,
                        Summary = r.Publication.Summary,
                        DataSource = r.Publication.DataSource,
                        Contact = r.Publication.Contact,
                        Topic = new ManageContentTopicViewModel
                        {
                            Theme = new ThemeViewModel
                            {
                                Title = r.Publication.Topic.Theme.Title
                            }
                        },
                        OtherReleases = r.Publication.Releases
                            .FindAll(otherRelease => !otherRelease.SoftDeleted &&
                                                     r.Id != otherRelease.Id &&
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
                            .Select(legacy => new LegacyReleaseViewModel
                            {
                                Id = legacy.Id,
                                Description = legacy.Description,
                                Url = legacy.Url
                            })
                            .ToList(),
                        ExternalMethodology = r.Publication.ExternalMethodology != null
                            ? new ExternalMethodology
                            {
                                Title = r.Publication.ExternalMethodology.Title,
                                Url = r.Publication.ExternalMethodology.Url
                            }
                            : null,
                        Methodology = r.Publication.Methodology != null
                            ? new MethodologyTitleViewModel
                                {
                                    Id = r.Publication.Methodology.Id,
                                    Title = r.Publication.Methodology.Title
                                }
                            : null
                    }))
                .ForMember(
                    dest => dest.LatestRelease,
                    m => m.MapFrom(r => r.Publication.LatestRelease().Id == r.Id))
                .ForMember(dest => dest.CoverageTitle,
                    m => m.MapFrom(r => r.TimePeriodCoverage.GetEnumLabel()));

            CreateMap<Update, ReleaseNoteViewModel>();

            CreateMap<Comment, CommentViewModel>()
                .ForMember(dest => dest.CreatedBy,
#pragma warning disable 612
                    m => m.MapFrom(c => c.LegacyCreatedBy));
#pragma warning restore 612

            CreateMap<ContentSection, ContentSectionViewModel>().ForMember(dest => dest.Content,
                m => m.MapFrom(section => section.Content.OrderBy(contentBlock => contentBlock.Order)));

            CreateMap<Methodology, ManageMethodologyContentViewModel>()
                .ForMember(dest => dest.Content,
                    m => m.MapFrom(methodology => methodology.Content.OrderBy(contentSection => contentSection.Order)))
                .ForMember(dest => dest.Annexes,
                    m => m.MapFrom(methodology => methodology.Annexes.OrderBy(annexSection => annexSection.Order)));

            CreateMap<Release, ReleasePublicationStatusViewModel>();
        }

        private static bool IsLatestVersionOfRelease(List<Release> releases, Guid releaseId)
        {
            return !releases.Any(r => r.PreviousVersionId == releaseId && r.Id != releaseId);
        }
    }
}
