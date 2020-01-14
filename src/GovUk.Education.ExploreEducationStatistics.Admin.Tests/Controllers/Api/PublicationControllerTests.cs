using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class PublicationControllerTests
    {

        [Fact]
        public async void Create_Publication_Returns_Ok()
        {
            var publicationService = new Mock<IPublicationService>();

            publicationService
                .Setup(s => s.CreatePublicationAsync(It.IsAny<CreatePublicationViewModel>()))
                .Returns<CreatePublicationViewModel>(p => Task.FromResult(new Either<ActionResult, PublicationViewModel>(new PublicationViewModel {TopicId = p.TopicId})));
            var controller = new PublicationController(publicationService.Object);

            var topicId = Guid.NewGuid();
            // Method under test
            var result = await controller.CreatePublicationAsync(new CreatePublicationViewModel(), topicId);
            Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            Assert.IsAssignableFrom<PublicationViewModel>(((OkObjectResult) result.Result).Value);

            var viewModel = (PublicationViewModel) ((OkObjectResult) result.Result).Value;
            Assert.Equal(topicId, viewModel.TopicId);
        }
        
        [Fact] 
        public async void Create_Publication_Validation_Failure()
        {
            var publicationService = new Mock<IPublicationService>();

            var validationResponse =
                new Either<ActionResult, PublicationViewModel>(
                    ValidationUtils.ValidationActionResult(ValidationErrorMessages.SlugNotUnique));
            
            publicationService
                .Setup(s => s.CreatePublicationAsync(It.IsAny<CreatePublicationViewModel>()))
                .Returns<CreatePublicationViewModel>(p => Task.FromResult(validationResponse));
            
            var controller = new PublicationController(publicationService.Object);

            var topicId = Guid.NewGuid();
            // Method under test
            var result = await controller.CreatePublicationAsync(new CreatePublicationViewModel(), topicId);
            var badRequestObjectResult = result.Result;
            Assert.IsAssignableFrom<BadRequestObjectResult>(badRequestObjectResult);
            var validationProblemDetails = (badRequestObjectResult as BadRequestObjectResult)?.Value;  
            Assert.IsAssignableFrom<ValidationProblemDetails>(validationProblemDetails);
            var errors = (validationProblemDetails as ValidationProblemDetails)?.Errors;
            Assert.Contains("SLUG_NOT_UNIQUE", errors.First().Value);
        }
    }
}