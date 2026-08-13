#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Moq;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api;

public abstract class DataReplacementControllerTests
{
    public class GetReplacementPlanTests : DataReplacementControllerTests
    {
        [Fact]
        public async Task Success()
        {
            var replacementPlanService = new Mock<IReplacementPlanService>(MockBehavior.Strict);

            var releaseVersionId = Guid.NewGuid();
            var originalFileId = Guid.NewGuid();

            var originalIndicatorId = Guid.NewGuid();
            var replacementIndicatorId = Guid.NewGuid();

            var originalLocationId = Guid.NewGuid();
            var replacementLocationId = Guid.NewGuid();

            var originalFilterId = Guid.NewGuid();
            var replacementFilterId = Guid.NewGuid();

            var originalFilterGroupId = Guid.NewGuid();
            var replacementFilterGroupId = Guid.NewGuid();

            var originalFilterItemId = Guid.NewGuid();
            var replacementFilterItemId = Guid.NewGuid();

            var dataReplacementPlan = new DataReplacementPlanViewModel
            {
                DataBlocks =
                [
                    new DataBlockReplacementPlanViewModel(
                        id: Guid.NewGuid(),
                        name: "my data block",
                        filters: new Dictionary<Guid, FilterReplacementViewModel>
                        {
                            {
                                originalFilterId,
                                new FilterReplacementViewModel(
                                    id: originalFilterId,
                                    target: replacementFilterId,
                                    label: "filter original label",
                                    name: "filter_original_name",
                                    groups: new Dictionary<Guid, FilterGroupReplacementViewModel>
                                    {
                                        {
                                            originalFilterGroupId,
                                            new FilterGroupReplacementViewModel(
                                                id: originalFilterGroupId,
                                                label: "filter group original label",
                                                target: replacementFilterGroupId,
                                                items:
                                                [
                                                    new FilterItemReplacementViewModel(
                                                        id: originalFilterItemId,
                                                        label: "filter item original label",
                                                        target: replacementFilterItemId
                                                    ),
                                                ]
                                            )
                                        },
                                    }
                                )
                            },
                        }
                    ),
                ],
                Footnotes = [],
                ApiDataSetVersionPlan = new ReplaceApiDataSetVersionPlanViewModel
                {
                    DataSetId = Guid.NewGuid(),
                    DataSetTitle = "my data set",
                    Id = Guid.NewGuid(),
                    Version = "v1.0",
                    Status = DataSetVersionStatus.Draft,
                },
                OriginalSubjectId = Guid.NewGuid(),
                ReplacementSubjectId = Guid.NewGuid(),
                Mapping = new ReplacementPlanMappingViewModel
                {
                    Filters = new ReplacementPlanFilterMappingsViewModel
                    {
                        Mappings = new Dictionary<Guid, ReplacementPlanFilterMappingViewModel>
                        {
                            {
                                originalFilterId,
                                new ReplacementPlanFilterMappingViewModel
                                {
                                    Source = new ReplacementPlanFilterViewModel
                                    {
                                        Id = originalFilterId,
                                        Name = "filter_original_name",
                                        Label = "filter original label",
                                    },
                                    Type = MapStatus.ManuallySet,
                                    CandidateKey = replacementFilterId,
                                    FilterGroups = new ReplacementPlanFilterGroupMappingsViewModel
                                    {
                                        Mappings = new Dictionary<Guid, ReplacementPlanFilterGroupMappingViewModel>
                                        {
                                            {
                                                originalFilterGroupId,
                                                new ReplacementPlanFilterGroupMappingViewModel
                                                {
                                                    Source = new ReplacementPlanFilterGroupViewModel
                                                    {
                                                        Id = originalFilterGroupId,
                                                        Label = "filter group original label",
                                                    },
                                                    Type = MapStatus.AutoSet,
                                                    CandidateKey = replacementFilterGroupId,
                                                    FilterItems = new ReplacementPlanFilterItemMappingsViewModel
                                                    {
                                                        Mappings = new Dictionary<
                                                            Guid,
                                                            ReplacementPlanFilterItemMappingViewModel
                                                        >
                                                        {
                                                            {
                                                                originalFilterItemId,
                                                                new ReplacementPlanFilterItemMappingViewModel
                                                                {
                                                                    Source = new ReplacementPlanFilterItemViewModel
                                                                    {
                                                                        Id = originalFilterItemId,
                                                                        Label = "filter item original label",
                                                                    },
                                                                    Type = MapStatus.AutoSet,
                                                                    CandidateKey = replacementFilterItemId,
                                                                }
                                                            },
                                                        },
                                                        Candidates = new Dictionary<
                                                            Guid,
                                                            ReplacementPlanFilterItemViewModel
                                                        >
                                                        {
                                                            {
                                                                replacementFilterItemId,
                                                                new ReplacementPlanFilterItemViewModel
                                                                {
                                                                    Id = replacementFilterItemId,
                                                                    Label = "filter item replacement label",
                                                                }
                                                            },
                                                        },
                                                    },
                                                }
                                            },
                                        },
                                        Candidates = new Dictionary<Guid, ReplacementPlanFilterGroupViewModel>
                                        {
                                            {
                                                replacementFilterGroupId,
                                                new ReplacementPlanFilterGroupViewModel
                                                {
                                                    Id = replacementFilterGroupId,
                                                    Label = "filter group replacement label",
                                                }
                                            },
                                        },
                                    },
                                }
                            },
                        },
                        Candidates = new Dictionary<Guid, ReplacementPlanFilterViewModel>
                        {
                            {
                                replacementFilterId,
                                new ReplacementPlanFilterViewModel
                                {
                                    Id = replacementFilterId,
                                    Name = "filter_replacement_name",
                                    Label = "filter replacement label",
                                }
                            },
                        },
                    },
                    Indicators = new ReplacementPlanIndicatorMappingsViewModel
                    {
                        Mappings = new Dictionary<Guid, ReplacementPlanIndicatorMappingViewModel>
                        {
                            {
                                originalIndicatorId,
                                new ReplacementPlanIndicatorMappingViewModel
                                {
                                    Source = new ReplacementPlanIndicatorViewModel
                                    {
                                        Id = originalIndicatorId,
                                        Name = "original_indicator",
                                        Label = "Original indicator",
                                    },
                                    Type = MapStatus.ManuallySet,
                                    CandidateKey = replacementIndicatorId,
                                }
                            },
                        },
                        Candidates = new Dictionary<Guid, ReplacementPlanIndicatorViewModel>
                        {
                            {
                                replacementIndicatorId,
                                new ReplacementPlanIndicatorViewModel
                                {
                                    Id = replacementIndicatorId,
                                    Name = "replacement_indicator",
                                    Label = "Replacement indicator",
                                }
                            },
                        },
                    },
                    Locations = new ReplacementPlanLocationMappingsViewModel
                    {
                        Mappings = new Dictionary<Guid, ReplacementPlanLocationMappingViewModel>
                        {
                            {
                                originalLocationId,
                                new ReplacementPlanLocationMappingViewModel
                                {
                                    Source = new ReplacementPlanLocationViewModel
                                    {
                                        Id = originalLocationId,
                                        Code = "E9000",
                                        Name = "OriginalLocation",
                                    },
                                    Type = MapStatus.ManuallySet,
                                    CandidateKey = replacementLocationId,
                                }
                            },
                        },
                        Candidates = new Dictionary<Guid, ReplacementPlanLocationViewModel>
                        {
                            {
                                replacementLocationId,
                                new ReplacementPlanLocationViewModel
                                {
                                    Id = replacementLocationId,
                                    Code = "E9393",
                                    Name = "ReplacementLocation",
                                }
                            },
                        },
                    },
                },
            };

            replacementPlanService
                .Setup(s => s.GetReplacementPlan(releaseVersionId, originalFileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataReplacementPlan);

            var controller = BuildController(replacementPlanService: replacementPlanService.Object);

            var result = await controller.GetReplacementPlan(
                releaseVersionId: releaseVersionId,
                originalFileId: originalFileId
            );

            MockUtils.VerifyAllMocks(replacementPlanService);

            var returnedPlan = result.AssertOkResult();

            var originalPlan = dataReplacementPlan.ToSummary();

            Assert.Equal(JsonConvert.SerializeObject(originalPlan), JsonConvert.SerializeObject(returnedPlan));
        }
    }

    public class ReplaceTests : DataReplacementControllerTests
    {
        [Fact]
        public async Task Success()
        {
            var replacementBatchService = new Mock<IReplacementBatchService>(MockBehavior.Strict);

            var releaseVersionId = Guid.NewGuid();
            var originalFileId = Guid.NewGuid();

            replacementBatchService
                .Setup(service =>
                    service.Replace(releaseVersionId, new List<Guid> { originalFileId }, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(Unit.Instance);

            var controller = BuildController(replacementBatchService: replacementBatchService.Object);

            var result = await controller.Replace(
                releaseVersionId: releaseVersionId,
                new ReplacementRequest { OriginalFileIds = [originalFileId] }
            );

            MockUtils.VerifyAllMocks(replacementBatchService);

            result.AssertOkResult();
        }

        [Fact]
        public async Task ValidationProblem()
        {
            var replacementBatchService = new Mock<IReplacementBatchService>(MockBehavior.Strict);

            var releaseVersionId = Guid.NewGuid();
            var originalFileId = Guid.NewGuid();

            replacementBatchService
                .Setup(service =>
                    service.Replace(releaseVersionId, new List<Guid> { originalFileId }, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(ValidationUtils.ValidationActionResult(ValidationErrorMessages.ReplacementMustBeValid));

            var controller = BuildController(replacementBatchService: replacementBatchService.Object);

            var result = await controller.Replace(
                releaseVersionId: releaseVersionId,
                new ReplacementRequest { OriginalFileIds = [originalFileId] }
            );

            MockUtils.VerifyAllMocks(replacementBatchService);

            result.AssertValidationProblem(ValidationErrorMessages.ReplacementMustBeValid);
        }
    }

    private static DataReplacementController BuildController(
        IReplacementPlanService? replacementPlanService = null,
        IReplacementBatchService? replacementBatchService = null
    )
    {
        return new DataReplacementController(
            replacementPlanService ?? Mock.Of<IReplacementPlanService>(MockBehavior.Strict),
            replacementBatchService ?? Mock.Of<IReplacementBatchService>(MockBehavior.Strict)
        );
    }
}
