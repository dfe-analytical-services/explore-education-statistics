using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ValidationTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyServiceTests
    {
        [Fact]
        public async Task CreateAsync()
        {
            var request = new MethodologyCreateRequest
            {
                Title = "Pupil absence statistics: methodology"
            };

            await using (var context = DbUtils.InMemoryApplicationDbContext())
            {
                var viewModel = (await BuildMethodologyService(
                        context,
                        Mocks())
                    .CreateMethodologyAsync(request)).Right;

                var model = await context.Methodologies.FindAsync(viewModel.Id);

                Assert.False(model.Live);
                Assert.Equal("pupil-absence-statistics-methodology", model.Slug);
                Assert.False(model.Updated.HasValue);

                Assert.Null(viewModel.InternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(Draft, viewModel.Status);
                Assert.Equal(request.Title, viewModel.Title);
            }
        }

        [Fact]
        public async Task CreateAsync_SlugNotUnique()
        {
            var request = new MethodologyCreateRequest
            {
                Title = "Pupil absence statistics: methodology"
            };

            await using (var context = DbUtils.InMemoryApplicationDbContext("CreateSlugNotUnique"))
            {
                var createResult = await BuildMethodologyService(
                        context,
                        Mocks())
                    .CreateMethodologyAsync(request);

                Assert.True(createResult.IsRight);

                var repeatedCreateResult = await BuildMethodologyService(
                        context,
                        Mocks())
                    .CreateMethodologyAsync(request);

                Assert.True(repeatedCreateResult.IsLeft);
                AssertValidationProblem(repeatedCreateResult.Left, SlugNotUnique);
            }
        }

        [Fact]
        public async Task ListAsync()
        {
            var methodology1 = new Methodology
            {
                Id = Guid.NewGuid(),
                Title = "Methodology 1",
                Status = Approved
            };

            var methodology2 = new Methodology
            {
                Id = Guid.NewGuid(),
                Title = "Methodology 2",
                Status = Draft
            };

            using (var context = DbUtils.InMemoryApplicationDbContext("List"))
            {
                await context.AddRangeAsync(new List<Methodology>
                {
                    methodology1, methodology2
                });
                await context.SaveChangesAsync();
            }

            using (var context = DbUtils.InMemoryApplicationDbContext("List"))
            {
                var methodologies = (await BuildMethodologyService(
                        context,
                        Mocks())
                    .ListAsync()).Right;

                Assert.Contains(methodologies,
                    m => m.Id == methodology1.Id
                         && m.Title == methodology1.Title
                         && m.Status == methodology1.Status);

                Assert.Contains(methodologies,
                    m => m.Id == methodology2.Id
                         && m.Title == methodology2.Title
                         && m.Status == methodology2.Status);
            }
        }

        [Fact]
        public async Task GetSummaryAsync()
        {
            var methodology = new Methodology
            {
                Id = Guid.NewGuid(),
                InternalReleaseNote = "Test approval",
                Published = new DateTime(2020, 5, 25),
                Slug = "pupil-absence-statistics-methodology",
                Status = Approved,
                Title = "Pupil absence statistics: methodology"
            };

            await using (var context = DbUtils.InMemoryApplicationDbContext("GetSummary"))
            {
                context.Add(methodology);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext("GetSummary"))
            {
                var viewModel = (await BuildMethodologyService(
                        context,
                        Mocks())
                    .GetSummaryAsync(methodology.Id)).Right;

                Assert.Equal(methodology.Id, viewModel.Id);
                Assert.Equal(methodology.InternalReleaseNote, viewModel.InternalReleaseNote);
                Assert.Equal(methodology.Published, viewModel.Published);
                Assert.Equal(methodology.Status, viewModel.Status);
                Assert.Equal(methodology.Title, viewModel.Title);
            }
        }

        [Fact]
        public async Task UpdateAsync()
        {
            var methodology = new Methodology
            {
                Id = Guid.NewGuid(),
                Slug = "pupil-absence-statistics-methodology",
                Status = Draft,
                Title = "Pupil absence statistics: methodology"
            };

            var request = new MethodologyUpdateRequest
            {
                InternalReleaseNote = null,
                Status = Draft,
                Title = "Pupil absence statistics (updated): methodology"
            };

            await using (var context = DbUtils.InMemoryApplicationDbContext("Update"))
            {
                context.Add(methodology);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext("Update"))
            {
                var viewModel = (await BuildMethodologyService(context,
                        Mocks())
                    .UpdateMethodologyAsync(methodology.Id, request)).Right;

                var model = await context.Methodologies.FindAsync(methodology.Id);

                Assert.False(model.Live);
                Assert.Equal("pupil-absence-statistics-updated-methodology", model.Slug);
                Assert.True(model.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(model.Updated.Value).Milliseconds, 0, 1500);

                Assert.Equal(methodology.Id, viewModel.Id);
                Assert.Null(viewModel.InternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(request.Status, viewModel.Status);
                Assert.Equal(request.Title, viewModel.Title);
            }
        }

        [Fact]
        public async Task UpdateAsync_AlreadyPublished()
        {
            var methodology = new Methodology
            {
                Id = Guid.NewGuid(),
                InternalReleaseNote = "Test approval",
                Published = new DateTime(2020, 5, 25),
                Slug = "pupil-absence-statistics-methodology",
                Status = Draft,
                Title = "Pupil absence statistics: methodology"
            };

            var request = new MethodologyUpdateRequest
            {
                InternalReleaseNote = null,
                Status = Draft,
                Title = "Pupil absence statistics (updated): methodology"
            };

            await using (var context = DbUtils.InMemoryApplicationDbContext("Update"))
            {
                context.Add(methodology);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext("Update"))
            {
                var viewModel = (await BuildMethodologyService(context,
                        Mocks())
                    .UpdateMethodologyAsync(methodology.Id, request)).Right;

                var model = await context.Methodologies.FindAsync(methodology.Id);
                
                Assert.True(model.Live);
                // Slug remains unchanged
                Assert.Equal("pupil-absence-statistics-methodology", model.Slug);
                Assert.True(model.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(model.Updated.Value).Milliseconds, 0, 1500);
                
                Assert.Equal(methodology.Id, viewModel.Id);
                // TODO EES-331 is this correct?
                // Original release note is not cleared if the update is not altering it
                Assert.Equal(methodology.InternalReleaseNote, viewModel.InternalReleaseNote);
                Assert.Equal(methodology.Published, viewModel.Published);
                Assert.Equal(request.Status, viewModel.Status);
                Assert.Equal(request.Title, viewModel.Title);
            }
        }
        
        [Fact]
        public async Task UpdateAsync_SlugNotUnique()
        {
            var methodology1 = new Methodology
            {
                Id = Guid.NewGuid(),
                Slug = "pupil-absence-statistics-methodology",
                Status = Draft,
                Title = "Methodology title"
            };

            var methodology2 = new Methodology
            {
                Id = Guid.NewGuid(),
                InternalReleaseNote = "Test approval",
                Published = new DateTime(2020, 5, 25),
                Slug = "pupil-exclusion-statistics-methodology",
                Status = Draft,
                Title = "Pupil exclusion statistics: methodology"
            };

            var request = new MethodologyUpdateRequest
            {
                InternalReleaseNote = null,
                Status = Draft,
                Title = "Pupil exclusion statistics: methodology"
            };

            await using (var context = DbUtils.InMemoryApplicationDbContext("UpdateSlugNotUnique"))
            {
                await context.AddRangeAsync(new List<Methodology>
                {
                    methodology1, methodology2
                });
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext("UpdateSlugNotUnique"))
            {
                var result = await BuildMethodologyService(
                        context,
                        Mocks())
                    .UpdateMethodologyAsync(methodology1.Id, request);

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, SlugNotUnique);
            }
        }
        
        private static MethodologyService BuildMethodologyService(ContentDbContext context,
            (Mock<IPublishingService> publishingService,
                Mock<IUserService> userService) mocks)
        {
            var (publishingService, userService) = mocks;

            return new MethodologyService(
                context,
                AdminMapper(),
                publishingService.Object,
                userService.Object,
                new MethodologyRepository(context),
                new PersistenceHelper<ContentDbContext>(context));
        }

        private static (Mock<IPublishingService> PublishingService,
            Mock<IUserService> UserService) Mocks()
        {
            return (new Mock<IPublishingService>(),
                MockUtils.AlwaysTrueUserService());
        }
    }
}
