#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FilterMapping = GovUk.Education.ExploreEducationStatistics.Content.Model.FilterMapping;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReplacementPlanService(
    ContentDbContext contentDbContext,
    IFootnoteRepository footnoteRepository,
    IDataSetVersionService dataSetVersionService,
    ITimePeriodService timePeriodService,
    IUserService userService,
    IDataSetVersionMappingService apiDataSetVersionMappingService,
    IReleaseFileRepository releaseFileRepository
) : IReplacementPlanService
{
    private static IComparer<string> LabelComparer { get; } = new LabelRelationalComparer();

    public async Task<Either<ActionResult, DataReplacementPlanViewModel>> GetReplacementPlan(
        Guid releaseVersionId,
        Guid originalFileId,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext
            .ReleaseVersions.Include(rv => rv.Release)
            .FirstOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken: cancellationToken)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(() =>
                releaseFileRepository.CheckLinkedOriginalAndReplacementReleaseFilesExist(
                    releaseVersionId: releaseVersionId,
                    originalFileId: originalFileId
                )
            )
            .OnSuccess(async releaseFiles =>
                await GenerateReplacementPlan(
                    originalReleaseFile: releaseFiles.originalReleaseFile,
                    replacementReleaseFile: releaseFiles.replacementReleaseFile,
                    cancellationToken: cancellationToken
                )
            );
    }

    private async Task<ReplaceApiDataSetVersionPlanViewModel?> GetApiVersionPlanViewModel(
        DataSetVersion replacementApiDataSetVersion,
        CancellationToken cancellationToken
    )
    {
        var apiDataSetVersionPlan = new ReplaceApiDataSetVersionPlanViewModel
        {
            DataSetId = replacementApiDataSetVersion.DataSetId,
            DataSetTitle = replacementApiDataSetVersion.DataSet.Title,
            Id = replacementApiDataSetVersion.Id,
            Version = replacementApiDataSetVersion.PublicVersion,
            Status = replacementApiDataSetVersion.Status,
            Valid = false,
        };

        var apiMappingStatus = await apiDataSetVersionMappingService.GetMappingStatus(
            replacementApiDataSetVersion.Id,
            cancellationToken
        );
        var isPatch = DataSetVersionNumber.TryParse(apiDataSetVersionPlan.Version, out var number) && number.Patch > 0;

        // If no mapping is found and the API version status is DRAFT, this data set version was deleted and recreated (& no mapping was necessary)
        // `completeStatusResult` is used for when we are replacing a draft release file (not an amendment) and therefore the mapping is complete/not applicable.
        var completeStatusResult = new MappingStatusViewModel
        {
            FiltersComplete = true,
            FiltersHaveMajorChange = false,
            LocationsComplete = true,
            LocationsHaveMajorChange = false,
            IndicatorsComplete = true,
            IndicatorsHaveMajorChange = false,
            HasDeletionChanges = false,
        };

        return apiDataSetVersionPlan with
        {
            MappingStatus = apiMappingStatus ?? (apiDataSetVersionPlan.ReadyToPublish ? completeStatusResult : null), // If no mapping is found, this data set version was deleted and recreated (& no mapping was necessary)
            Valid =
                (
                    isPatch
                        ? apiMappingStatus is { IsMajorVersionUpdate: false } && apiDataSetVersionPlan.ReadyToPublish
                        : apiDataSetVersionPlan.ReadyToPublish
                ) || (apiMappingStatus is null && apiDataSetVersionPlan.ReadyToPublish), // Data set version was deleted and recreated (as opposed to as a patch increment of a previous data set version)
        };
    }

    public async Task<Either<ActionResult, DataReplacementPlanViewModel>> GenerateReplacementPlan(
        ReleaseFile originalReleaseFile,
        ReleaseFile replacementReleaseFile,
        CancellationToken cancellationToken
    )
    {
        return await GetLinkedDataSetVersion(replacementReleaseFile, cancellationToken)
            .OnSuccess(async replacementApiDataSetVersion =>
            {
                var originalSubjectId = originalReleaseFile.File.SubjectId!.Value;
                var replacementSubjectId = replacementReleaseFile.File.SubjectId!.Value;

                var replacementSubjectMeta = await GetReplacementSubjectMeta(replacementSubjectId);

                var mapping = await contentDbContext.DataSetMappings.SingleAsync(
                    map =>
                        map.OriginalDataFileId == originalReleaseFile.FileId
                        && map.ReplacementDataFileId == replacementReleaseFile.FileId,
                    cancellationToken
                );

                var releaseVersionId = replacementReleaseFile.ReleaseVersionId;

                var dataBlocks = ValidateDataBlocks(
                    releaseVersionId: releaseVersionId,
                    subjectId: originalSubjectId,
                    mapping,
                    replacementSubjectMeta
                );
                var footnotes = await ValidateFootnotes(
                    releaseVersionId: releaseVersionId,
                    subjectId: originalSubjectId,
                    mapping,
                    replacementSubjectMeta
                );

                var apiDataSetVersionPlan = replacementApiDataSetVersion is null
                    ? null
                    : await GetApiVersionPlanViewModel(replacementApiDataSetVersion, cancellationToken);

                var mappingPlan = GenerateMappingViewModel(mapping);

                return new DataReplacementPlanViewModel
                {
                    DataBlocks = dataBlocks,
                    Footnotes = footnotes,
                    ApiDataSetVersionPlan = apiDataSetVersionPlan,
                    OriginalSubjectId = originalSubjectId,
                    ReplacementSubjectId = replacementSubjectId,
                    Mapping = mappingPlan,
                };
            });
    }

    public async Task<bool> HasValidReplacementPlan(
        ReleaseFile originalReleaseFile,
        ReleaseFile replacementReleaseFile,
        CancellationToken cancellationToken = default
    )
    {
        var result = await GenerateReplacementPlan(originalReleaseFile, replacementReleaseFile, cancellationToken);

        return result.IsRight && result.Right.Valid;
    }

    private async Task<Either<ActionResult, DataSetVersion?>> GetLinkedDataSetVersion(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken = default
    )
    {
        if (releaseFile.PublicApiDataSetId is null)
        {
            return (DataSetVersion)null!;
        }

        return await dataSetVersionService
            .GetDataSetVersion(
                releaseFile.PublicApiDataSetId.Value,
                releaseFile.PublicApiDataSetVersion!,
                cancellationToken
            )
            .OnSuccess(dsv => (DataSetVersion?)dsv)
            .OnFailureDo(_ =>
                throw new InvalidOperationException(
                    $"API data set version could not be found. Data set ID: '{releaseFile.PublicApiDataSetId}', version: '{releaseFile.PublicApiDataSetVersion}'"
                )
            );
    }

    private async Task<ReplacementSubjectMeta> GetReplacementSubjectMeta(Guid subjectId)
    {
        var timePeriods = await timePeriodService.GetTimePeriods(subjectId);

        // We don't need to include data about locations or indicators here - DataSetMapping contains the data we require to
        // validate the replacement

        return new ReplacementSubjectMeta { TimePeriods = timePeriods };
    }

    private List<DataBlockReplacementPlanViewModel> ValidateDataBlocks(
        Guid releaseVersionId,
        Guid subjectId,
        DataSetMapping mapping,
        ReplacementSubjectMeta replacementSubjectMeta
    )
    {
        return contentDbContext
            .ContentBlocks.Where(block => block.ReleaseVersionId == releaseVersionId)
            .OfType<DataBlock>()
            .ToList()
            .Where(dataBlock => dataBlock.Query.SubjectId == subjectId)
            .Select(dataBlock =>
            {
                var existingFilters = ValidateFiltersForDataBlock(dataBlock.Query.GetFilterItemIds(), mapping);
                var indicatorGroups = CreateIndicatorGroupReplacementViewModel(dataBlock.Query.Indicators, mapping);
                var locations = ValidateLocationsForDataBlock(dataBlock.Query.LocationIds, mapping);
                var timePeriods = ValidateTimePeriodsForDataBlock(dataBlock, replacementSubjectMeta);

                return new DataBlockReplacementPlanViewModel(
                    dataBlock.Id,
                    dataBlock.Name,
                    existingFilters,
                    indicatorGroups,
                    locations,
                    timePeriods
                );
            })
            .ToList();
    }

    private async Task<List<FootnoteReplacementPlanViewModel>> ValidateFootnotes(
        Guid releaseVersionId,
        Guid subjectId,
        DataSetMapping mapping,
        ReplacementSubjectMeta replacementSubjectMeta
    )
    {
        var footnotes = await footnoteRepository.GetFootnotes(releaseVersionId: releaseVersionId, subjectId: subjectId);
        return footnotes.Select(footnote => ValidateFootnote(footnote, replacementSubjectMeta, mapping)).ToList();
    }

    private static FootnoteReplacementPlanViewModel ValidateFootnote(
        Footnote footnote,
        ReplacementSubjectMeta replacementSubjectMeta,
        DataSetMapping mapping
    )
    {
        var filters = ValidateFiltersForFootnote(footnote, mapping);
        var filterGroups = ValidateFilterGroupsForFootnote(footnote, mapping);
        var filterItems = ValidateFilterItemsForFootnote(footnote, mapping);
        var indicatorGroups = CreateIndicatorGroupReplacementViewModel(
            footnote.Indicators.Select(indFootnote => indFootnote.IndicatorId),
            mapping
        );

        return new FootnoteReplacementPlanViewModel(
            footnote.Id,
            footnote.Content,
            filters,
            filterGroups,
            filterItems,
            indicatorGroups
        );
    }

    private static List<FootnoteFilterReplacementViewModel> ValidateFiltersForFootnote(
        Footnote footnote,
        DataSetMapping mapping
    )
    {
        var footnoteFilterIds = footnote.Filters.Select(f => f.FilterId).ToHashSet();

        return mapping
            .FilterMappings.Values.Where(filterMap => footnoteFilterIds.Contains(filterMap.OriginalId))
            .Select(filterMap => new FootnoteFilterReplacementViewModel(
                id: filterMap.OriginalId,
                label: filterMap.OriginalLabel,
                target: filterMap.ReplacementId
            ))
            .OrderBy(f => f.Label, LabelComparer)
            .ToList();
    }

    private static List<FootnoteFilterGroupReplacementViewModel> ValidateFilterGroupsForFootnote(
        Footnote footnote,
        DataSetMapping mapping
    )
    {
        var footnoteFilterGroupIds = footnote.FilterGroups.Select(g => g.FilterGroupId).ToHashSet();
        return mapping
            .FilterMappings.Values.SelectMany(
                filterMap => filterMap.FilterGroupMappings.Values,
                (filterMap, groupMap) =>
                    new
                    {
                        FilterId = filterMap.OriginalId,
                        FilterLabel = filterMap.OriginalLabel,
                        Group = groupMap,
                    }
            )
            .Where(pair => footnoteFilterGroupIds.Contains(pair.Group.OriginalId))
            .Select(pair => new FootnoteFilterGroupReplacementViewModel(
                id: pair.Group.OriginalId,
                label: pair.Group.OriginalLabel,
                filterId: pair.FilterId,
                filterLabel: pair.FilterLabel,
                target: pair.Group.ReplacementId
            ))
            .OrderBy(f => f.Label, LabelComparer)
            .ToList();
    }

    private static List<FootnoteFilterItemReplacementViewModel> ValidateFilterItemsForFootnote(
        Footnote footnote,
        DataSetMapping mapping
    )
    {
        var footnoteFilterItemIds = footnote.FilterItems.Select(f => f.FilterItemId).ToHashSet();
        return mapping
            .FilterMappings.Values.SelectMany(
                filterMap => filterMap.FilterGroupMappings.Values,
                (filterMap, groupMap) => new { Filter = filterMap, FilterGroup = groupMap }
            )
            .SelectMany(
                x => x.FilterGroup.FilterItemMappings.Values,
                (x, itemMap) =>
                    new
                    {
                        x.Filter,
                        x.FilterGroup,
                        FilterItem = itemMap,
                    }
            )
            .Where(x => footnoteFilterItemIds.Contains(x.FilterItem.OriginalId))
            .Select(x => new FootnoteFilterItemReplacementViewModel(
                id: x.FilterItem.OriginalId,
                label: x.FilterItem.OriginalLabel,
                filterId: x.Filter.OriginalId,
                filterLabel: x.Filter.OriginalLabel,
                filterGroupId: x.FilterGroup.OriginalId,
                filterGroupLabel: x.FilterGroup.OriginalLabel,
                target: x.FilterItem.ReplacementId
            ))
            .ToList(); // @MarkFix you finished here!!!! you didn't read through this to ensure it is correct
    }

    private static Dictionary<Guid, FilterReplacementViewModel> ValidateFiltersForDataBlock(
        List<Guid> dataBlockFilterItemIds,
        DataSetMapping mapping
    )
    {
        return mapping
            .FilterMappings.Values.Where(filterMap =>
                filterMap
                    .FilterGroupMappings.Values.SelectMany(groupMap => groupMap.FilterItemMappings.Values)
                    .Select(item => item.OriginalId)
                    .Any(dataBlockFilterItemIds.Contains)
            )
            .ToDictionary(
                filterMap => filterMap.OriginalId,
                filterMap => new FilterReplacementViewModel(
                    id: filterMap.OriginalId,
                    name: filterMap.OriginalColumnName,
                    label: filterMap.OriginalLabel,
                    target: filterMap.ReplacementId,
                    groups: filterMap
                        .FilterGroupMappings.Values.Where(groupMap =>
                            groupMap
                                .FilterItemMappings.Values.Select(item => item.OriginalId)
                                .Any(dataBlockFilterItemIds.Contains)
                        )
                        .ToDictionary(
                            groupMap => groupMap.OriginalId,
                            groupMap => new FilterGroupReplacementViewModel(
                                id: groupMap.OriginalId,
                                label: groupMap.OriginalLabel,
                                target: groupMap.ReplacementId,
                                filters: groupMap
                                    .FilterItemMappings.Values.Where(itemMap =>
                                        dataBlockFilterItemIds.Contains(itemMap.OriginalId)
                                    )
                                    .Select(itemMap => new FilterItemReplacementViewModel(
                                        id: itemMap.OriginalId,
                                        label: itemMap.OriginalLabel,
                                        target: itemMap.ReplacementId
                                    ))
                            )
                        )
                )
            );
    }

    private static Dictionary<string, LocationReplacementViewModel> ValidateLocationsForDataBlock(
        List<Guid> dataBlockLocationIds,
        DataSetMapping mapping
    )
    {
        return mapping
            .LocationMappings.Values.Where(map => dataBlockLocationIds.Contains(map.OriginalId))
            .GroupBy(map => map.OriginalGeographicLevel)
            .ToDictionary(
                group => group.Key.ToString(),
                group => new LocationReplacementViewModel(
                    label: group.Key.ToString(),
                    locationAttributes: group
                        .Select(map => new LocationAttributeReplacementViewModel(
                            id: map.OriginalId,
                            code: map.OriginalCode,
                            label: map.OriginalName,
                            target: map.ReplacementId
                        ))
                        .OrderBy(location => location.Label, LabelComparer)
                )
            );
    }

    private static TimePeriodRangeReplacementViewModel ValidateTimePeriodsForDataBlock(
        DataBlock dataBlock,
        ReplacementSubjectMeta replacementSubjectMeta
    )
    {
        return new TimePeriodRangeReplacementViewModel(
            start: ValidateTimePeriodForReplacement(
                dataBlock.Query.TimePeriod!.StartYear,
                dataBlock.Query.TimePeriod.StartCode,
                replacementSubjectMeta
            ),
            end: ValidateTimePeriodForReplacement(
                dataBlock.Query.TimePeriod.EndYear,
                dataBlock.Query.TimePeriod.EndCode,
                replacementSubjectMeta
            )
        );
    }

    private static TimePeriodReplacementViewModel ValidateTimePeriodForReplacement(
        int year,
        TimeIdentifier code,
        ReplacementSubjectMeta replacementSubjectMeta
    )
    {
        return new TimePeriodReplacementViewModel(
            year: year,
            code: code,
            valid: replacementSubjectMeta.TimePeriods.Contains((year, code))
        );
    }

    private static Dictionary<Guid, IndicatorGroupReplacementViewModel> CreateIndicatorGroupReplacementViewModel(
        IEnumerable<Guid> indicatorIds,
        DataSetMapping mapping
    )
    {
        return mapping
            .IndicatorMappings.Values.Where(map => indicatorIds.Contains(map.OriginalId))
            .GroupBy(map => new { GroupId = map.OriginalGroupId, GroupLabel = map.OriginalGroupLabel })
            .OrderBy(group => group.Key.GroupLabel, LabelComparer)
            .ToDictionary(
                group => group.Key.GroupId,
                group => new IndicatorGroupReplacementViewModel(
                    id: group.Key.GroupId,
                    label: group.Key.GroupLabel,
                    indicators: group
                        .OrderBy(map => map.OriginalLabel, LabelComparer)
                        .Select(map => new IndicatorReplacementViewModel(
                            id: map.OriginalId,
                            name: map.OriginalColumnName,
                            label: map.OriginalLabel,
                            target: map.ReplacementId
                        ))
                )
            );
    }

    private static Filter? FindReplacementFilter(ReplacementSubjectMeta replacementSubjectMeta, string filterName)
    {
        return replacementSubjectMeta.Filters.GetValueOrDefault(filterName);
    }

    private static FilterGroup? FindReplacementFilterGroup(
        ReplacementSubjectMeta replacementSubjectMeta,
        string filterName,
        string filterGroupLabel
    )
    {
        var replacementFilter = FindReplacementFilter(replacementSubjectMeta, filterName);
        return replacementFilter?.FilterGroups.SingleOrDefault(filterGroup => filterGroup.Label == filterGroupLabel);
    }

    private static FilterItem? FindReplacementFilterItem(
        ReplacementSubjectMeta replacementSubjectMeta,
        string filterName,
        string filterGroupLabel,
        string filterItemLabel
    )
    {
        var replacementFilterGroup = FindReplacementFilterGroup(replacementSubjectMeta, filterName, filterGroupLabel);
        return replacementFilterGroup?.FilterItems.SingleOrDefault(filterItem => filterItem.Label == filterItemLabel);
    }

    private class ReplacementSubjectMeta
    {
        public Dictionary<string, Filter> Filters { get; set; } = new(); // @MarkFix remove
        public IList<(int Year, TimeIdentifier TimeIdentifier)> TimePeriods { get; set; } = null!;
    }

    private static ReplacementPlanMappingViewModel GenerateMappingViewModel(DataSetMapping mapping)
    {
        var indicatorMappings = mapping.IndicatorMappings.Values.ToDictionary(
            map => map.OriginalId,
            map => new ReplacementPlanIndicatorMappingViewModel
            {
                Source = new ReplacementPlanIndicatorViewModel
                {
                    Id = map.OriginalId,
                    Name = map.OriginalColumnName,
                    Label = map.OriginalLabel,
                },
                Type = map.Status.ToString(),
                CandidateKey = map.ReplacementId,
            }
        );
        var indicatorCandidates = mapping // candidates are all possible replacement indicators
            .IndicatorMappings.Values.Where(indMap => indMap.ReplacementId != null)
            .Select(indMap => new
            {
                Id = indMap.ReplacementId!.Value,
                ColumnName = indMap.ReplacementColumnName!,
                Label = indMap.ReplacementLabel!,
            })
            .Concat(
                mapping.UnmappedReplacementIndicators.Select(i => new
                {
                    i.Id,
                    i.ColumnName,
                    i.Label,
                })
            )
            .ToDictionary(
                i => i.Id,
                i => new ReplacementPlanIndicatorViewModel
                {
                    Id = i.Id,
                    Name = i.ColumnName,
                    Label = i.Label,
                }
            );
        var locationMappings = mapping.LocationMappings.Values.ToDictionary(
            map => map.OriginalId,
            map => new ReplacementPlanLocationMappingViewModel
            {
                Source = new ReplacementPlanLocationViewModel
                {
                    Id = map.OriginalId,
                    Code = map.OriginalCode,
                    Name = map.OriginalName,
                },
                Type = map.Status.ToString(),
                CandidateKey = map.ReplacementId,
            }
        );
        var locationCandidates = mapping // candidates are all possible replacement locations
            .LocationMappings.Values.Where(l => l.ReplacementId != null)
            .Select(l => new
            {
                Id = l.ReplacementId!.Value,
                Code = l.ReplacementCode!,
                Name = l.ReplacementName!,
            })
            .Concat(
                mapping.UnmappedReplacementLocations.Select(l => new
                {
                    l.Id,
                    l.Code,
                    l.Name,
                })
            )
            .ToDictionary(
                l => l.Id,
                l => new ReplacementPlanLocationViewModel
                {
                    Id = l.Id,
                    Code = l.Code,
                    Name = l.Name,
                }
            );

        var filterMappings = mapping.FilterMappings.Values.ToDictionary(
            f => f.OriginalId,
            f => new ReplacementPlanFilterMappingViewModel
            {
                Source = new ReplacementPlanFilterViewModel
                {
                    Id = f.OriginalId,
                    Name = f.OriginalColumnName,
                    Label = f.OriginalLabel,
                },
                CandidateKey = f.ReplacementId,
                Type = f.Status.ToString(),
                FilterGroups = GenerateFilterGroupMappingsViewModel(f),
            }
        );

        var filterCandidates = mapping
            .FilterMappings.Values.Where(filterMap => filterMap.ReplacementId != null)
            .Select(filterMap => new
            {
                Id = filterMap.ReplacementId!.Value,
                Label = filterMap.ReplacementLabel!,
                Name = filterMap.ReplacementColumnName!,
            })
            .Concat(
                mapping.UnmappedReplacementFilters.Select(unmappedFilter => new
                {
                    unmappedFilter.Id,
                    unmappedFilter.Label,
                    Name = unmappedFilter.ColumnName,
                })
            )
            .ToDictionary(
                x => x.Id,
                x => new ReplacementPlanFilterViewModel
                {
                    Id = x.Id,
                    Label = x.Label,
                    Name = x.Name,
                }
            );

        return new ReplacementPlanMappingViewModel
        {
            Indicators = new ReplacementPlanIndicatorsMappingViewModel
            {
                Mappings = indicatorMappings,
                Candidates = indicatorCandidates,
            },
            Locations = new ReplacementPlanLocationMappingsViewModel
            {
                Mappings = locationMappings,
                Candidates = locationCandidates,
            },
            Filters = new ReplacementPlanFilterMappingsViewModel
            {
                Mappings = filterMappings,
                Candidates = filterCandidates,
            },
        };
    }

    private static ReplacementPlanFilterGroupMappingsViewModel GenerateFilterGroupMappingsViewModel(
        Content.Model.FilterMapping filterMapping
    )
    {
        return new ReplacementPlanFilterGroupMappingsViewModel
        {
            Mappings = filterMapping.FilterGroupMappings.Values.ToDictionary(
                g => g.OriginalId,
                g => new ReplacementPlanFilterGroupMappingViewModel
                {
                    Source = new ReplacementPlanFilterGroupViewModel { Id = g.OriginalId, Label = g.OriginalLabel },
                    CandidateKey = g.ReplacementId,
                    Type = g.Status.ToString(),
                    FilterItems = GenerateFilterItemMappingsViewModel(g),
                }
            ),
            Candidates = filterMapping
                .FilterGroupMappings.Values.Where(f => f.ReplacementId != null)
                .Select(groupMap => new { Id = groupMap.ReplacementId!.Value, Label = groupMap.ReplacementLabel! })
                .Concat(
                    filterMapping.UnmappedReplacementFilterGroups.Select(unmappedGroup => new
                    {
                        unmappedGroup.Id,
                        unmappedGroup.Label,
                    })
                )
                .ToDictionary(x => x.Id, x => new ReplacementPlanFilterGroupViewModel { Id = x.Id, Label = x.Label }),
        };
    }

    private static ReplacementPlanFilterItemMappingsViewModel GenerateFilterItemMappingsViewModel(
        FilterGroupMapping groupMapping
    )
    {
        return new ReplacementPlanFilterItemMappingsViewModel
        {
            Mappings = groupMapping.FilterItemMappings.Values.ToDictionary(
                i => i.OriginalId,
                i => new ReplacementPlanFilterItemMappingViewModel
                {
                    Source = new ReplacementPlanFilterItemViewModel { Id = i.OriginalId, Label = i.OriginalLabel },
                    CandidateKey = i.ReplacementId,
                    Type = i.Status.ToString(),
                }
            ),
            Candidates = groupMapping
                .FilterItemMappings.Values.Where(itemMap => itemMap.ReplacementId != null)
                .Select(itemMap => new { Id = itemMap.ReplacementId!.Value, Label = itemMap.ReplacementLabel! })
                .Concat(
                    groupMapping.UnmappedReplacementFilterItems.Select(unmappedItem => new
                    {
                        unmappedItem.Id,
                        unmappedItem.Label,
                    })
                )
                .ToDictionary(x => x.Id, x => new ReplacementPlanFilterItemViewModel { Id = x.Id, Label = x.Label }),
        };
    }
}
