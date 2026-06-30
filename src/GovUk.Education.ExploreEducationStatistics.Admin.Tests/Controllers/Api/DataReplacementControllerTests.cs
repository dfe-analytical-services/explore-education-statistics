#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
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
                                Guid.NewGuid(),
                                new FilterReplacementViewModel(
                                    id: Guid.NewGuid(), // original filterId
                                    target: Guid.NewGuid(), // replacement filterId
                                    label: "filter replacement lebel",
                                    name: "filter replacement name",
                                    groups: new Dictionary<Guid, FilterGroupReplacementViewModel>
                                    {
                                        {
                                            Guid.NewGuid(),
                                            new FilterGroupReplacementViewModel(
                                                id: Guid.NewGuid(),
                                                label: "filter group replacement label",
                                                filters:
                                                [
                                                    new FilterItemReplacementViewModel(
                                                        id: Guid.NewGuid(),
                                                        label: "filter item replacement label",
                                                        target: Guid.NewGuid()
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
                    Indicators = new ReplacementPlanIndicatorsMappingViewModel
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
                                    Type = "ManuallySet",
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
                                    Type = "ManuallySet",
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
