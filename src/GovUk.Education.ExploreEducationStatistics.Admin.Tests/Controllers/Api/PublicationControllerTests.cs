using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class PublicationControllerTests
    {
        [Fact]
        public async Task CreatePublication_Ok()
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
        public async Task CreatePublication_ValidationFailure()
        {
            var publicationService = new Mock<IPublicationService>();

            var validationResponse =
                new Either<ActionResult, PublicationViewModel>(
                    ValidationUtils.ValidationActionResult(SlugNotUnique));
            
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

            result.Result.AssertBadRequest(SlugNotUnique);
        }
    }
}
