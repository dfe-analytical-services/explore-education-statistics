#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Mappings;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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
using KeyStatisticDataBlockViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.KeyStatisticDataBlockViewModel;
using KeyStatisticTextViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.KeyStatisticTextViewModel;
using KeyStatisticViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.KeyStatisticViewModel;
using MethodologyNoteViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology.MethodologyNoteViewModel;
using MethodologyVersionViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology.MethodologyVersionViewModel;
using OrganisationViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.OrganisationViewModel;
using PublicationViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.PublicationViewModel;
using ReleaseNoteViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent.ReleaseNoteViewModel;
using ReleaseVersionSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ReleaseVersionSummaryViewModel;
using ReleaseVersionViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ReleaseVersionViewModel;
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

        CreateMap<ReleaseVersion, ReleaseVersionViewModel>()
            .ForMember(dest => dest.ReleaseId, m => m.MapFrom(rv => rv.ReleaseId))
            .ForMember(dest => dest.Slug, m => m.MapFrom(rv => rv.Release.Slug))
            .ForMember(dest => dest.TimePeriodCoverage, m => m.MapFrom(rv => rv.Release.TimePeriodCoverage))
            .ForMember(dest => dest.Title, m => m.MapFrom(rv => rv.Release.Title))
            .ForMember(dest => dest.Year, m => m.MapFrom(rv => rv.Release.Year))
            .ForMember(dest => dest.YearTitle, m => m.MapFrom(rv => rv.Release.YearTitle))
            .ForMember(dest => dest.Published, m => m.MapFrom(releaseVersion => releaseVersion.PublishedDisplayDate))
            .ForMember(
                dest => dest.LatestRelease,
                m => m.MapFrom(rv => rv.Release.Publication.LatestPublishedReleaseVersionId == rv.Id)
            )
            .ForMember(dest => dest.PublicationTitle, m => m.MapFrom(rv => rv.Release.Publication.Title))
            .ForMember(dest => dest.PublicationId, m => m.MapFrom(rv => rv.Release.Publication.Id))
            .ForMember(dest => dest.PublicationSlug, m => m.MapFrom(rv => rv.Release.Publication.Slug))
            .ForMember(
                dest => dest.PublishScheduled,
                m =>
                    m.MapFrom(rv =>
                        rv.PublishScheduled.HasValue ? rv.PublishScheduled.Value.ToUkDateOnly() : (DateOnly?)null
                    )
            )
            .ForMember(
                dest => dest.PublishingOrganisations,
                m => m.MapFrom(rv => rv.PublishingOrganisations.OrderBy(o => o.Title))
            )
            .ForMember(dest => dest.Label, m => m.MapFrom(rv => rv.Release.Label));

        CreateMap<ReleaseVersion, ReleaseVersionSummaryViewModel>()
            .ForMember(dest => dest.Slug, m => m.MapFrom(rv => rv.Release.Slug))
            .ForMember(dest => dest.Label, m => m.MapFrom(rv => rv.Release.Label))
            .ForMember(dest => dest.TimePeriodCoverage, m => m.MapFrom(rv => rv.Release.TimePeriodCoverage))
            .ForMember(dest => dest.Title, m => m.MapFrom(rv => rv.Release.Title))
            .ForMember(dest => dest.Year, m => m.MapFrom(rv => rv.Release.Year))
            .ForMember(dest => dest.YearTitle, m => m.MapFrom(rv => rv.Release.YearTitle))
            .ForMember(dest => dest.Published, m => m.MapFrom(releaseVersion => releaseVersion.PublishedDisplayDate))
            .ForMember(
                dest => dest.PublishScheduled,
                m =>
                    m.MapFrom(model =>
                        model.PublishScheduled.HasValue ? model.PublishScheduled.Value.ToUkDateOnly() : (DateOnly?)null
                    )
            );

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
            .ForMember(dest => dest.Query, m => m.MapFrom(c => c.Query.AsFullTableQuery()));
        CreateMap<DataBlockUpdateRequest, DataBlock>()
            .ForMember(dest => dest.Query, m => m.MapFrom(c => c.Query.AsFullTableQuery()));

        CreateMap<KeyStatisticDataBlock, KeyStatisticDataBlockViewModel>();
        CreateMap<KeyStatisticText, KeyStatisticTextViewModel>();
        CreateMap<KeyStatistic, KeyStatisticViewModel>().IncludeAllDerived();

        CreateMap<KeyStatisticDataBlockCreateRequest, KeyStatisticDataBlock>();
        CreateMap<KeyStatisticTextCreateRequest, KeyStatisticText>();

        CreateMap<FeaturedTable, FeaturedTableViewModel>();
        CreateMap<FeaturedTableCreateRequest, FeaturedTable>();

        CreateMap<Theme, ThemeViewModel>();

        CreateMap<ContentSection, ContentSectionViewModel>()
            .ForMember(
                dest => dest.Content,
                m => m.MapFrom(section => section.Content.OrderBy(contentBlock => contentBlock.Order))
            );

        CreateMap<Organisation, OrganisationViewModel>();

        CreateMap<ReleaseVersion, ManageContentPageViewModel.ReleaseViewModel>()
            .ForMember(dest => dest.CoverageTitle, m => m.MapFrom(rv => rv.Release.TimePeriodCoverage.GetEnumLabel()))
            .ForMember(dest => dest.ReleaseName, m => m.MapFrom(rv => rv.Release.Year.ToString()))
            .ForMember(dest => dest.Slug, m => m.MapFrom(rv => rv.Release.Slug))
            .ForMember(dest => dest.Title, m => m.MapFrom(rv => rv.Release.Title))
            .ForMember(dest => dest.YearTitle, m => m.MapFrom(rv => rv.Release.YearTitle))
            .ForMember(dest => dest.Published, m => m.MapFrom(releaseVersion => releaseVersion.PublishedDisplayDate))
            .ForMember(dest => dest.Content, m => m.MapFrom(rv => rv.GenericContent.OrderBy(s => s.Order)))
            .ForMember(dest => dest.KeyStatistics, m => m.MapFrom(rv => rv.KeyStatistics.OrderBy(ks => ks.Order)))
            .ForMember(dest => dest.Updates, m => m.MapFrom(rv => rv.Updates.OrderByDescending(update => update.On)))
            .ForMember(dest => dest.Publication, m => m.Ignore())
            .ForMember(
                dest => dest.LatestRelease,
                m => m.MapFrom(rv => rv.Release.Publication.LatestPublishedReleaseVersionId == rv.Id)
            )
            .ForMember(
                dest => dest.HasPreReleaseAccessList,
                m => m.MapFrom(rv => !rv.PreReleaseAccessList.IsNullOrEmpty())
            )
            .ForMember(
                model => model.PublishScheduled,
                m =>
                    m.MapFrom(rv =>
                        rv.PublishScheduled.HasValue ? rv.PublishScheduled.Value.ToUkDateOnly() : (DateOnly?)null
                    )
            )
            .ForMember(
                dest => dest.PublishingOrganisations,
                m => m.MapFrom(rv => rv.PublishingOrganisations.OrderBy(o => o.Title))
            );

        CreateMap<Update, ReleaseNoteViewModel>();

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
