using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyAmendmentServiceTests
    {
        [Fact]
        // TODO SOW4 EES-2156 - test copying Methodology Files
        public async Task CreateMethodologyAmendment()
        {
            var originalMethodology = new Methodology
            {
                Id = Guid.NewGuid(),
                Status = Approved,
                Published = DateTime.Today,
                MethodologyParent = new MethodologyParent
                {
                    Slug = "methodology-slug",
                    OwningPublicationTitle = "Owning Publication Title"
                },
                Content = AsList(new ContentSection
                {
                    Content = AsList<ContentBlock>(new HtmlBlock
                    {
                        Body = "Content!"
                    })
                })
            };
            
            var contextId = Guid.NewGuid().ToString();
            await using(var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Methodologies.AddAsync(originalMethodology);
                await context.SaveChangesAsync();
            }

            Guid amendmentId;

            // Call the method under test
            await using(var context = InMemoryApplicationDbContext(contextId))
            {
                var methodologyService = new Mock<IMethodologyService>();
                var service = BuildService(context, methodologyService: methodologyService.Object);
                
                var amendmentIdCapture = new List<Guid>();
                var summaryViewModel = new MethodologySummaryViewModel();
                
                methodologyService
                    .Setup(s => s.GetSummary(Capture.In(amendmentIdCapture)))
                    .ReturnsAsync(summaryViewModel);
                
                var result = await service.CreateMethodologyAmendment(originalMethodology.Id);
                var resultingViewModel = result.AssertRight();
                Assert.Same(summaryViewModel, resultingViewModel);
                amendmentId = Assert.Single(amendmentIdCapture);
            }

            // Check that the amendment was successfully saved.  More detailed field-by-field testing is available in
            // the MethodologyTests class.
            await using(var context = InMemoryApplicationDbContext(contextId))
            {
                var amendment = await context.Methodologies.SingleAsync(m => m.Id == amendmentId);
                Assert.Equal(originalMethodology.Id, amendment.PreviousVersionId);

                var contentSection = Assert.Single(amendment.Content);
                Assert.NotNull(contentSection);
                var contentBlock = Assert.Single(contentSection.Content) as HtmlBlock;
                Assert.NotNull(contentBlock);
                Assert.Equal("Content!", contentBlock.Body);
            }
        }

        private static MethodologyAmendmentService BuildService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IMethodologyService methodologyService = null,
            IUserService userService = null)
        {
            return new MethodologyAmendmentService(
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                userService ?? AlwaysTrueUserService().Object,
                methodologyService ?? new Mock<IMethodologyService>().Object,
                contentDbContext
            );
        }
    }
}