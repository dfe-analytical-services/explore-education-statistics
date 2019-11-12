using System;
using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Publication = GovUk.Education.ExploreEducationStatistics.Content.Model.Publication;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    /**
     * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
     */
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Footnote, FootnoteViewModel>();

            CreateMap<IndicatorFootnote, long>().ConvertUsing(footnote => footnote.IndicatorId);
            CreateMap<FilterFootnote, long>().ConvertUsing(footnote => footnote.FilterId);
            CreateMap<FilterGroupFootnote, long>().ConvertUsing(footnote => footnote.FilterGroupId);
            CreateMap<FilterItemFootnote, long>().ConvertUsing(footnote => footnote.FilterItemId);
            CreateMap<SubjectFootnote, long>().ConvertUsing(footnote => footnote.SubjectId);
            
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

            CreateMap<Publication, PublicationViewModel>()
                .ForMember(
                    dest => dest.ThemeId,
                    m => m.MapFrom(p => p.Topic.ThemeId));    

            CreateMap<DataBlock, DataBlockViewModel>();
            CreateMap<CreateDataBlockViewModel, DataBlock>();
            CreateMap<UpdateDataBlockViewModel, DataBlock>();
        }
    }
}