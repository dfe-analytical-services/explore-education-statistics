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
                .Setup(s => s.CreatePublication(It.IsAny<PublicationSaveViewModel>()))
                .Returns<PublicationSaveViewModel>(p => 
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
            var result = await controller.CreatePublication(new PublicationSaveViewModel() 
            {
                TopicId = topicId
            });
            
            Assert.IsType<PublicationViewModel>(result.Value);
            Assert.Equal(topicId, result.Value.TopicId);
        }
        
        [Fact] 
        public async void CreatePublication_ValidationFailure()
        {
            var publicationService = new Mock<IPublicationService>();

            var validationResponse =
                new Either<ActionResult, PublicationViewModel>(
                    ValidationUtils.ValidationActionResult(ValidationErrorMessages.SlugNotUnique));
            
            publicationService
                .Setup(s => s.CreatePublication(It.IsAny<PublicationSaveViewModel>()))
                .Returns<PublicationSaveViewModel>(p => Task.FromResult(validationResponse));
            
            var controller = new PublicationController(publicationService.Object);

            var topicId = Guid.NewGuid();

            // Method under test
            var result = await controller.CreatePublication(new PublicationSaveViewModel()
            {
                TopicId = topicId
            });

            var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var value = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);

            Assert.Contains("SLUG_NOT_UNIQUE", value.Errors.First().Value);
        }
    }
}