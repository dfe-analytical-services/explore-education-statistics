#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Responses.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Mappings;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using ContentSectionViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ContentSectionViewModel;
using DataBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.DataBlockViewModel;
using EmbedBlockLinkViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.EmbedBlockLinkViewModel;
using HtmlBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.HtmlBlockViewModel;
using IContentBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.IContentBlockViewModel;
using MethodologyNoteViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology.MethodologyNoteViewModel;
using MethodologyVersionViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology.MethodologyVersionViewModel;
using PublicationViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.PublicationViewModel;
using ThemeViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ThemeViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings;

/**
 * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
 */
public class MappingProfiles : CommonMappingProfile
{
    public MappingProfiles()
    {
        CreateMap<User, UserDetailsViewModel>();

        CreateMap<ReleasePublishingStatus, ReleasePublishingStatusViewModel>()
            .ForMember(model => model.LastUpdated, m => m.MapFrom(status => status.Timestamp))
            .ForMember(model => model.ReleaseId, m => m.MapFrom(status => status.PartitionKey));

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
        CreateMap<DataBlockCreateRequest, DataBlock>()
            .ForMember(dest => dest.Query, m => m.MapFrom(c => c.Query.AsFullTableQuery(default, default)));
        CreateMap<DataBlockUpdateRequest, DataBlock>()
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
            .ForMember(dest => dest.Status, m => m.MapFrom(upload => GetDataSetUploadStatus(upload.ScreenerResult)))
            .ForMember(
                dest => dest.PublicApiCompatible,
                m => m.MapFrom(upload => upload.ScreenerResult.PublicApiCompatible)
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

        CreateMap<DataScreenerTestResult, ScreenerTestResultViewModel>()
            .ForMember(dest => dest.Result, m => m.MapFrom(upload => upload.Result.ToString()));

        CreateMap<DataSetUpload, DataSetScreenerRequest>()
            .BeforeMap((_, d) => d.StorageContainerName = Constants.ContainerNames.PrivateReleaseTempFiles);

        CreateMap<DataSetUpload, DataSetStartScreeningRequest>()
            .BeforeMap((_, d) => d.StorageContainerName = Constants.ContainerNames.PrivateReleaseTempFiles)
            .ForMember(d => d.DataSetId, m => m.MapFrom(upload => upload.Id));
    }

    private static string GetDataSetUploadStatus(DataSetScreenerResponse screenerResult)
    {
        if (screenerResult is null)
        {
            return nameof(DataSetUploadStatus.SCREENER_ERROR);
        }

        if (screenerResult.Passed && screenerResult.TestResults.Any(test => test.Result == TestResult.WARNING))
        {
            return nameof(DataSetUploadStatus.PENDING_REVIEW);
        }

        return !screenerResult.Passed
            ? nameof(DataSetUploadStatus.FAILED_SCREENING)
            : nameof(DataSetUploadStatus.PENDING_IMPORT);
    }

    private void CreateContentBlockMap()
    {
        CreateMap<ContentBlock, IContentBlockViewModel>()
            .IncludeAllDerived()
            .ForMember(
                dest => dest.Comments,
                m => m.MapFrom(block => block.Comments.OrderBy(comment => comment.Created))
            );

        // EES-4640 - we include an AfterMap configuration here to ensure that any time we create a
        // DataBlockViewModel from a plain DataBlock, we also include the DataBlockParentId on the
        // destination DataBlockViewModel that the DataBlock itself does not contain. When DataBlock is
        // removed from the ContentBlock model, this can go too.
        CreateMap<DataBlock, DataBlockViewModel>().AfterMap<DataBlockViewModelPostMappingAction>();

        CreateMap<DataBlockVersion, DataBlockViewModel>();

        CreateMap<EmbedBlockLink, EmbedBlockLinkViewModel>()
            .ForMember(dest => dest.Title, m => m.MapFrom(embedBlockLink => embedBlockLink.EmbedBlock.Title))
            .ForMember(dest => dest.Url, m => m.MapFrom(embedBlockLink => embedBlockLink.EmbedBlock.Url));

        CreateMap<HtmlBlock, HtmlBlockViewModel>();
    }
}
