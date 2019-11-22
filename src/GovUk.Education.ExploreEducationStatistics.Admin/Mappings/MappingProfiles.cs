using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using PublicationViewModel = GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.PublicationViewModel;
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
            CreateMap<Release, Data.Processor.Model.Release>().ForMember(dest => dest.Title,
                opts => opts.MapFrom(release => release.ReleaseName));
            
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

            CreateMap<Release, ReleaseSummaryViewModel>();

            CreateMap<ReleaseSummaryViewModel, Release>();

            CreateMap<CreateReleaseViewModel, Release>();
            
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

            CreateMap<DataBlock, DataBlockViewModel>();
            CreateMap<CreateDataBlockViewModel, DataBlock>();
            CreateMap<UpdateDataBlockViewModel, DataBlock>();

            CreateMap<Release, ViewModels.ManageContent.ReleaseViewModel>()
                .ForMember(dest => dest.Content, 
                    m => m.MapFrom(r => 
                        r.Content.Select(ContentSectionViewModel.ToViewModel)))
                .ForMember(
                    dest => dest.Updates,
                    m => m.MapFrom(r => new List<ReleaseNoteViewModel>
                    {
                        new ReleaseNoteViewModel
                        {
                            Id = new Guid("df18b1f6-9f24-4e87-9275-31a0c78b1dad"),
                            Reason = "Release note 1",
                            On = new DateTime(2019, 06, 02)
                        },
                        new ReleaseNoteViewModel
                        {
                            Id = new Guid("c9948cde-01ce-4315-87af-eaa6cd7c6879"),
                            Reason = "Release note 2",
                            On = new DateTime(2019, 08, 10)
                        }
                    })
                )
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
                        NextUpdate = r.Publication.NextUpdate,
                        Topic = new TopicViewModel
                        {
                            Theme = new ThemeViewModel
                            {
                                Title = r.Publication.Topic.Theme.Title
                            } 
                        },
                        Releases = r.Publication.Releases
                            .FindAll(otherRelease => otherRelease.Id != r.Id)    
                            .Select(otherRelease => new PreviousReleaseViewModel
                            {
                                Id = otherRelease.Id,
                                Slug = otherRelease.Slug,
                                Title = otherRelease.Title,
                                ReleaseName = otherRelease.ReleaseName
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
                        Methodology = new MethodologyViewModel
                        {
                            Id = r.Publication.Methodology.Id,
                            Title = r.Publication.Methodology.Title
                        }
                    }))
                .ForMember(
                    dest => dest.LatestRelease,
                    m => m.MapFrom(r => r.Publication.LatestRelease().Id == r.Id));
        }
    }
}