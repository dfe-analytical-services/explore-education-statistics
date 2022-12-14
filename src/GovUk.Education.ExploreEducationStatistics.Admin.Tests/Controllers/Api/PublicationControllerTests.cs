#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api;
 
public class PublicationControllerTests
{
    [Fact]
    public async Task CreatePublication_Ok()
    {
        var publicationService = new Mock<IPublicationService>(Strict);

        publicationService
            .Setup(s => s.CreatePublication(It.IsAny<PublicationCreateRequest>()))
            .Returns<PublicationCreateRequest>(p =>
                Task.FromResult(new Either<ActionResult, PublicationCreateViewModel>(
                    new PublicationCreateViewModel
                    {
                        Topic = new IdTitleViewModel
                        {
                            Id = p.TopicId,
                        }
                    })
                )
            );

        var controller = BuildController(publicationService: publicationService.Object);

        var topicId = Guid.NewGuid();

        // Method under test
        var result = await controller.CreatePublication(new PublicationCreateRequest()
        {
            TopicId = topicId
        });

        Assert.IsType<PublicationCreateViewModel>(result.Value);
        Assert.Equal(topicId, result.Value.Topic.Id);
    }

    [Fact]
    public async Task CreatePublication_ValidationFailure()
    {
        var publicationService = new Mock<IPublicationService>();

        var validationResponse =
            new Either<ActionResult, PublicationCreateViewModel>(
                ValidationUtils.ValidationActionResult(SlugNotUnique));

        publicationService
            .Setup(s => s.CreatePublication(It.IsAny<PublicationCreateRequest>()))
            .Returns<PublicationCreateRequest>(p => Task.FromResult(validationResponse));

        var controller = BuildController(publicationService: publicationService.Object);

        var topicId = Guid.NewGuid();

        // Method under test
        var result = await controller.CreatePublication(new PublicationCreateRequest()
        {
            TopicId = topicId
        });

        result.Result!.AssertBadRequest(SlugNotUnique);
    }
    
    [Fact]
    public async Task GetRoles()
    {
        var publicationId = Guid.NewGuid();

        var rolesForPublication = ListOf(new UserPublicationRoleViewModel
        {
            Id = Guid.NewGuid()
        });
    
        var roleService = new Mock<IUserRoleService>(Strict);

        roleService
            .Setup(s => s.GetPublicationRolesForPublication(publicationId))
            .ReturnsAsync(rolesForPublication);
    
        var controller = BuildController(roleService: roleService.Object);

        // Method under test
        var result = await controller.GetRoles(publicationId);
        Assert.Equal(rolesForPublication, result.Value);
    }

    private PublicationController BuildController(
        IPublicationService? publicationService = null,
        IUserRoleService? roleService = null)
    {
        return new PublicationController(
            publicationService ?? Mock.Of<IPublicationService>(Strict),
            roleService ?? Mock.Of<IUserRoleService>(Strict));
    }
}