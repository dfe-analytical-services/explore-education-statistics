#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Mappings;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using ContentSectionViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ContentSectionViewModel;
using DataBlockVersionViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.DataBlockVersionViewModel;
using EmbedBlockLinkViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.EmbedBlockLinkViewModel;
using HtmlBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.HtmlBlockViewModel;
using IContentBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.IContentBlockViewModel;
using MethodologyNoteViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology.MethodologyNoteViewModel;
using MethodologyVersionViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology.MethodologyVersionViewModel;
using PublicationViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.PublicationViewModel;
using ThemeViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ThemeViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings;

/// <summary>
/// AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
/// </summary>
public class MappingProfiles : CommonMappingProfile
{
    public MappingProfiles()
    {
        CreateMap<User, UserDetailsViewModel>();

        CreateMap<MethodologyNote, MethodologyNoteViewModel>();

        CreateMap<MethodologyVersion, MethodologyVersionViewModel>()
            .ForMember(dest => dest.ScheduledWithRelease, m => m.Ignore());

        CreateMap<MethodologyVersion, IdTitleViewModel>();

        CreateMap<Theme, IdTitleViewModel>();
        CreateMap<Publication, PublicationSummaryViewModel>();
        CreateMap<Publication, PublicationViewModel>().ForMember(dest => dest.Theme, m => m.MapFrom(p => p.Theme));
        CreateMap<Publication, PublicationCreateViewModel>()
            .ForMember(dest => dest.Theme, m => m.MapFrom(p => p.Theme));

        CreateContentBlockMap();
        CreateMap<DataBlockCreateRequest, DataBlockVersion>()
            .ForMember(dest => dest.Query, m => m.MapFrom(c => c.Query.AsFullTableQuery(default, default)));
        CreateMap<DataBlockUpdateRequest, DataBlockVersion>()
            .ForMember(dest => dest.Query, m => m.MapFrom(c => c.Query.AsFullTableQuery(default, default)));

        CreateMap<FeaturedTable, FeaturedTableViewModel>();
        CreateMap<FeaturedTableCreateRequest, FeaturedTable>();

        CreateMap<Theme, ThemeViewModel>();

        CreateMap<ContentSection, ContentSectionViewModel>()
            .ForMember(
                dest => dest.Content,
                m => m.MapFrom(section => section.Content.OrderBy(contentBlock => contentBlock.Order))
            );

        CreateMap<Comment, CommentViewModel>()
            .ForMember(
                dest => dest.CreatedBy,
                m =>
                    m.MapFrom(comment =>
                        comment.CreatedById == null
                            ? new User
                            {
#pragma warning disable 612
                                FirstName = comment.LegacyCreatedBy ?? "",
#pragma warning restore 612
                                LastName = "",
                                Email = "",
                                Active = false,
                                RoleId = "",
                                Created = DateTimeOffset.MinValue,
                                CreatedById = Guid.Empty,
                            }
                            : comment.CreatedBy
                    )
            );

        CreateMap<ContentSection, ContentSectionViewModel>()
            .ForMember(
                dest => dest.Content,
                m => m.MapFrom(section => section.Content.OrderBy(contentBlock => contentBlock.Order))
            );

        CreateMap<MethodologyVersion, ManageMethodologyContentViewModel>()
            .ForMember(
                dest => dest.Content,
                m =>
                    m.MapFrom(methodologyVersion =>
                        methodologyVersion.MethodologyContent.Content.OrderBy(contentSection => contentSection.Order)
                    )
            )
            .ForMember(
                dest => dest.Annexes,
                m =>
                    m.MapFrom(methodologyVersion =>
                        methodologyVersion.MethodologyContent.Annexes.OrderBy(annexSection => annexSection.Order)
                    )
            )
            .ForMember(
                dest => dest.Notes,
                m =>
                    m.MapFrom(methodologyVersion =>
                        methodologyVersion.Notes.OrderByDescending(note => note.DisplayDate)
                    )
            );

        CreateMap<ReleaseVersion, ReleasePublicationStatusViewModel>();

        CreateMap<DataSetVersion, DataSetVersionInfoViewModel>()
            .ForMember(dest => dest.Version, m => m.MapFrom(dataSetVersion => dataSetVersion.PublicVersion))
            .ForMember(dest => dest.Type, m => m.MapFrom(dataSetVersion => dataSetVersion.VersionType));

        CreateMap<DataSetUpload, DataSetUploadViewModel>()
            .ForMember(dest => dest.ScreeningStatus, m => m.MapFrom(upload => upload.ScreeningStatus))
            .ForMember(
                dest => dest.PublicApiCompatible,
                m => m.MapFrom(upload => upload.ScreenerResult != null && upload.ScreenerResult.PublicApiCompatible)
            )
            .ForMember(
                dest => dest.DataFileSize,
                m => m.MapFrom(upload => FileExtensions.DisplaySize(upload.DataFileSizeInBytes))
            )
            .ForMember(
                dest => dest.MetaFileSize,
                m => m.MapFrom(upload => FileExtensions.DisplaySize(upload.MetaFileSizeInBytes))
            );

        CreateMap<DataSetScreenerResponse, ScreenerResultViewModel>();

        CreateMap<DataSetScreenerProgress, ScreenerProgressViewModel>();

        CreateMap<DataScreenerTestResult, ScreenerTestResultViewModel>()
            .ForMember(dest => dest.Result, m => m.MapFrom(upload => upload.Result.ToString()));

        CreateMap<DataSetUpload, DataSetStartScreeningRequest>()
            .ForMember(d => d.DataSetId, m => m.MapFrom(upload => upload.Id));
    }

    private void CreateContentBlockMap()
    {
        CreateMap<ContentBlock, IContentBlockViewModel>()
            .IncludeAllDerived()
            .ForMember(
                dest => dest.Comments,
                m => m.MapFrom(block => block.Comments.OrderBy(comment => comment.Created))
            );

        // A DataBlockVersionLink (a ContentBlock) carries the positional/locking state (Order, Comments, Locked, ...)
        // mapped by the base ContentBlock -> IContentBlockViewModel map above, while the data block's content
        // (Heading, Name, Query, ...) and DataBlockId are read through its DataBlockVersion navigation.
        CreateMap<DataBlockVersionLink, DataBlockVersionViewModel>()
            .ForMember(dest => dest.DataBlockId, m => m.MapFrom(link => link.DataBlockVersion.DataBlockId))
            .ForMember(dest => dest.Heading, m => m.MapFrom(link => link.DataBlockVersion.Heading))
            .ForMember(dest => dest.Name, m => m.MapFrom(link => link.DataBlockVersion.Name))
            .ForMember(dest => dest.Source, m => m.MapFrom(link => link.DataBlockVersion.Source))
            .ForMember(dest => dest.Query, m => m.MapFrom(link => link.DataBlockVersion.Query))
            .ForMember(dest => dest.Charts, m => m.MapFrom(link => link.DataBlockVersion.Charts))
            .ForMember(dest => dest.Table, m => m.MapFrom(link => link.DataBlockVersion.Table));

        // An unattached data block has no DataBlockVersionLink, so it is mapped straight from its DataBlockVersion
        // and simply has no positional or locking state. The view model's Id remains the DataBlockVersion's Id
        // either way, as a link shares its Id with the DataBlockVersion it points at.
        CreateMap<DataBlockVersion, DataBlockVersionViewModel>();

        CreateMap<EmbedBlockLink, EmbedBlockLinkViewModel>()
            .ForMember(dest => dest.Title, m => m.MapFrom(embedBlockLink => embedBlockLink.EmbedBlock.Title))
            .ForMember(dest => dest.Url, m => m.MapFrom(embedBlockLink => embedBlockLink.EmbedBlock.Url));

        CreateMap<HtmlBlock, HtmlBlockViewModel>();
    }
}
