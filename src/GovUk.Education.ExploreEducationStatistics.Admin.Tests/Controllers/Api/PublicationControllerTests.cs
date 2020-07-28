using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class PublicationControllerTests
    {
        [Fact]
        public async void CreatePublication_Ok()
        {
            var publicationService = new Mock<IPublicationService>();

            publicationService
                .Setup(s => s.CreatePublication(It.IsAny<CreatePublicationViewModel>()))
                .Returns<CreatePublicationViewModel>(p => 
                    Task.FromResult(new Either<ActionResult, PublicationViewModel>(
                        new PublicationViewModel 
                        {
                            TopicId = p.TopicId
                        })
                    )
                );

            var controller = new PublicationController(publicationService.Object);

            var topicId = Guid.NewGuid();

            // Method under test
            var result = await controller.CreatePublication(new CreatePublicationViewModel() 
            {
                TopicId = topicId
            });
            
            Assert.IsAssignableFrom<OkObjectResult>(result.Result);
            Assert.IsAssignableFrom<PublicationViewModel>(((OkObjectResult) result.Result).Value);

            var viewModel = (PublicationViewModel) ((OkObjectResult) result.Result).Value;
            Assert.Equal(topicId, viewModel.TopicId);
        }
        
        [Fact] 
        public async void CreatePublication_ValidationFailure()
        {
            var publicationService = new Mock<IPublicationService>();

            var validationResponse =
                new Either<ActionResult, PublicationViewModel>(
                    ValidationUtils.ValidationActionResult(ValidationErrorMessages.SlugNotUnique));
            
            publicationService
                .Setup(s => s.CreatePublication(It.IsAny<CreatePublicationViewModel>()))
                .Returns<CreatePublicationViewModel>(p => Task.FromResult(validationResponse));
            
            var controller = new PublicationController(publicationService.Object);

            var topicId = Guid.NewGuid();
            // Method under test
            var result = await controller.CreatePublication(new CreatePublicationViewModel()
            {
                TopicId = topicId
            });

            var badRequestObjectResult = result.Result;
            Assert.IsAssignableFrom<BadRequestObjectResult>(badRequestObjectResult);

            var validationProblemDetails = (badRequestObjectResult as BadRequestObjectResult)?.Value;  
            Assert.IsAssignableFrom<ValidationProblemDetails>(validationProblemDetails);

            var errors = (validationProblemDetails as ValidationProblemDetails)?.Errors;
            Assert.Contains("SLUG_NOT_UNIQUE", errors.First().Value);
        }
    }
}