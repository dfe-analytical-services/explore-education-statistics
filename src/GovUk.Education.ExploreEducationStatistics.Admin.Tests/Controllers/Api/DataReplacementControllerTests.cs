#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api;

public abstract class DataReplacementControllerTests
{
    public class GetReplacementPlanTests : DataReplacementControllerTests
    {
        [Fact]
        public async Task Success()
        {
            var replacementService = new Mock<IReplacementService>(MockBehavior.Strict);

            var releaseVersionId = Guid.NewGuid();
            var originalFileId = Guid.NewGuid();

            var dataReplacementPlan = new DataReplacementPlanViewModel
            {
                DataBlocks = [
                    new DataBlockReplacementPlanViewModel(
                        id: Guid.NewGuid(),
                        name: "my data block",
                        originalFilters: new Dictionary<Guid, FilterReplacementViewModel> {
                            { 
                                Guid.NewGuid(), 
                                new FilterReplacementViewModel(
                                    id: Guid.NewGuid(), // original filterId
                                    target: Guid.NewGuid(), // replacement filterId
                                    label: "filter replacement lebel",
                                    name: "filter replacement name",
                                    groups: new Dictionary<Guid, FilterGroupReplacementViewModel> {
                                        {
                                            Guid.NewGuid(),
                                            new FilterGroupReplacementViewModel(
                                                id: Guid.NewGuid(),
                                                label: "filter group replacement label",
                                                filters: [
                                                    new FilterItemReplacementViewModel(
                                                        id: Guid.NewGuid(),
                                                        label: "filter item replacement label",
                                                        target: Guid.NewGuid())
                                                ])
                                        }
                                    })
                            }
                        })
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
                ReplacementSubjectId = Guid.NewGuid()
            };

            replacementService
                .Setup(s => s.GetReplacementPlan(
                    releaseVersionId, 
                    originalFileId, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataReplacementPlan);

            var controller = BuildController(replacementService: replacementService.Object);

            var result = await controller.GetReplacementPlan(
                releaseVersionId: releaseVersionId,
                originalFileId: originalFileId);

            MockUtils.VerifyAllMocks(replacementService);

            var returnedPlan = result.AssertOkResult();

            var originalPlan = dataReplacementPlan.ToSummary();

            Assert.Equal(
                JsonConvert.SerializeObject(originalPlan),
                JsonConvert.SerializeObject(returnedPlan));
        }
    }

    public class ReplaceTests : DataReplacementControllerTests
    {
        [Fact]
        public async Task Success()
        {
            var replacementService = new Mock<IReplacementService>(MockBehavior.Strict);

            var releaseVersionId = Guid.NewGuid();
            var originalFileId = Guid.NewGuid();

            replacementService
                .Setup(service => service.Replace(
                    releaseVersionId,
                    originalFileId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Instance);

            var controller = BuildController(replacementService: replacementService.Object);

            var result = await controller.Replace(
                releaseVersionId: releaseVersionId,
                originalFileId: originalFileId);

            MockUtils.VerifyAllMocks(replacementService);

            result.AssertOkResult();
        }

        [Fact]
        public async Task ValidationProblem()
        {
            var replacementService = new Mock<IReplacementService>(MockBehavior.Strict);

            var releaseVersionId = Guid.NewGuid();
            var originalFileId = Guid.NewGuid();

            replacementService
                .Setup(service => service.Replace(
                    releaseVersionId,
                    originalFileId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidationUtils.ValidationActionResult(ValidationErrorMessages.ReplacementMustBeValid));

            var controller = BuildController(replacementService: replacementService.Object);

            var result = await controller.Replace(
                releaseVersionId: releaseVersionId,
                originalFileId: originalFileId);

            MockUtils.VerifyAllMocks(replacementService);

            result.AssertValidationProblem(ValidationErrorMessages.ReplacementMustBeValid);
        }
    }

    private static DataReplacementController BuildController(IReplacementService? replacementService = null)
    {
        return new DataReplacementController(
            replacementService ?? Mock.Of<IReplacementService>(MockBehavior.Strict));
    }
}
