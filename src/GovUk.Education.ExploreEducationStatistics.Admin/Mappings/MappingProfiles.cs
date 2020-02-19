using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
                    m => m.MapFrom(r => r.Publication.Id))
                // TODO return real Comments as soon as commenting on Releases has been implemented
                .ForMember(dest => dest.DraftComments, 
                    m => m.MapFrom(_ => new List<ReleaseViewModel.Comment>()
                    {
                        new ReleaseViewModel.Comment() { Message = "Message 1\nSome multiline content\nSpanning several lines", AuthorName = "TODO User", CreatedDate = DateTime.Now.AddMonths(-2)},
                        new ReleaseViewModel.Comment() { Message = "Message 2", AuthorName = "TODO User 2", CreatedDate = DateTime.Now.AddMonths(-3)},
                        new ReleaseViewModel.Comment() { Message = "Message 3", AuthorName = "TODO User 3", CreatedDate = DateTime.Now.AddMonths(-4)},
                    }))
                .ForMember(dest => dest.HigherReviewComments, 
                    m => m.MapFrom(_ => new List<ReleaseViewModel.Comment>()
                    {
                        new ReleaseViewModel.Comment() { Message = "Message 4", AuthorName = "TODO Responsible Statistician 4", CreatedDate = DateTime.Now.AddDays(-2)},
                    }));
            
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
                // TODO return real Comments as soon as commenting on Releases has been implemented
                .ForMember(dest => dest.DraftComments, 
                    m => m.MapFrom(_ => new List<MyReleaseViewModel.Comment>()
                    {
                        new MyReleaseViewModel.Comment() { Message = "Message 1\nSome multiline content\nSpanning several lines", AuthorName = "TODO User", CreatedDate = DateTime.Now.AddMonths(-2)},
                        new MyReleaseViewModel.Comment() { Message = "Message 2", AuthorName = "TODO User 2", CreatedDate = DateTime.Now.AddMonths(-3)},
                        new MyReleaseViewModel.Comment() { Message = "Message 3", AuthorName = "TODO User 3", CreatedDate = DateTime.Now.AddMonths(-4)},
                    }))
                .ForMember(dest => dest.HigherReviewComments, 
                    m => m.MapFrom(_ => new List<MyReleaseViewModel.Comment>()
                    {
                        new MyReleaseViewModel.Comment() { Message = "Message 4", AuthorName = "TODO Responsible Statistician 4", CreatedDate = DateTime.Now.AddDays(-2)},
                    }))
                .ForMember(dest => dest.Permissions, exp => exp.MapFrom<IMyReleasePermissionSetPropertyResolver>());

            CreateMap<Release, ReleaseSummaryViewModel>();

            CreateMap<ReleaseSummaryViewModel, Release>();

            CreateMap<CreateReleaseViewModel, Release>();

            CreateMap<ReleaseStatus, ReleaseStatusViewModel>()
                .ForMember(model => model.LastUpdated, m => m.MapFrom(status => status.Timestamp));
            
            CreateMap<CreateReleaseViewModel, ReleaseSummaryVersion>().ForMember(r => r.Id, m => m.Ignore());
            CreateMap<ReleaseSummaryViewModel, ReleaseSummaryVersion>().ForMember(r => r.Id, m => m.Ignore());

            CreateMap<ReleaseSummary, ReleaseSummaryViewModel>()
                .ForMember(model => model.InternalReleaseNote,
                    m => m.MapFrom(summary => summary.Release.InternalReleaseNote))
                .ForMember(model => model.Status,
                    m => m.MapFrom(summary => summary.Release.Status));

            CreateMap<Methodology, MethodologyViewModel>();
            CreateMap<Methodology, MethodologyStatusViewModel>();
            CreateMap<Publication, MethodologyStatusPublications>();
            
            CreateMap<Publication, PublicationViewModel>()
                .ForMember(
                    dest => dest.ThemeId,
                    m => m.MapFrom(p => p.Topic.ThemeId));   
            
            CreateMap<Publication, MyPublicationViewModel>()
                .ForMember(
                    dest => dest.ThemeId,
                    m => m.MapFrom(p => p.Topic.ThemeId))
                .ForMember(dest => dest.Permissions, exp => exp.MapFrom<IMyPublicationPermissionSetPropertyResolver>());   

            CreateMap<DataBlock, DataBlockViewModel>();
            CreateMap<CreateDataBlockViewModel, DataBlock>();
            CreateMap<UpdateDataBlockViewModel, DataBlock>();

            CreateMap<Topic, ApiTopicViewModel>();

            CreateMap<CommentState, string>().ConvertUsing(s => s.GetEnumValue());

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
                            .FindAll(otherRelease => otherRelease.Id != r.Id)    
                            .Select(otherRelease => new PreviousReleaseViewModel
                            {
                                Id = otherRelease.Id,
                                Slug = otherRelease.Slug,
                                Title = otherRelease.Title,
                            })
                            .ToList(),
                        LegacyReleases = r.Publication.LegacyReleases
                            .Select(legacy => new BasicLink
                            {
                                Id = legacy.Id,
                                Description = legacy.Description,
                                Url = legacy.Url
                            })
                            .ToList(),
                        Methodology = r.Publication.Methodology != null 
                            ? new MethodologyViewModel
                                {
                                    Id = r.Publication.Methodology.Id,
                                    Title = r.Publication.Methodology.Title
                                } 
                            : null
                    }))
                .ForMember(
                    dest => dest.LatestRelease,
                    m => m.MapFrom(r => r.Publication.LatestRelease().Id == r.Id));
            
            CreateMap<Update, ReleaseNoteViewModel>();

            CreateMap<Comment, CommentViewModel>()
                .ForMember(
                    dest => dest.State,
                    m => m.MapFrom(c => c.State.GetEnumValue())
                );
            
            CreateMap<ContentSection, ContentSectionViewModel>();

            CreateMap<Methodology, ManageMethodologyContentViewModel>();
        }
    }
}