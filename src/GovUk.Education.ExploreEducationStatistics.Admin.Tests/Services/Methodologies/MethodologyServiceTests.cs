using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
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

            await using (var context = InMemoryApplicationDbContext())
            {
                var service = SetupMethodologyService(contentDbContext: context);

                var viewModel = (await service.CreateMethodologyAsync(request)).Right;

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

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context);

                var createResult = await service.CreateMethodologyAsync(request);

                Assert.True(createResult.IsRight);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context);

                var repeatedCreateResult = await service.CreateMethodologyAsync(request);

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

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.AddRangeAsync(new List<Methodology>
                {
                    methodology1, methodology2
                });
                await context.SaveChangesAsync();
            }

            using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context);

                var methodologies = (await service.ListAsync()).Right;

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

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.Add(methodology);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context);

                var viewModel = (await service.GetSummaryAsync(methodology.Id)).Right;

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

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.Add(methodology);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context);

                var viewModel = (await service.UpdateMethodologyAsync(methodology.Id, request)).Right;

                Assert.Equal(methodology.Id, viewModel.Id);
                Assert.Null(viewModel.InternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(request.Status, viewModel.Status);
                Assert.Equal(request.Title, viewModel.Title);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var model = await context.Methodologies.FindAsync(methodology.Id);

                Assert.False(model.Live);
                Assert.Equal("pupil-absence-statistics-updated-methodology", model.Slug);
                Assert.True(model.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(model.Updated.Value).Milliseconds, 0, 1500);
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

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                context.Add(methodology);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context);

                var viewModel = (await service.UpdateMethodologyAsync(methodology.Id, request)).Right;

                Assert.Equal(methodology.Id, viewModel.Id);
                // TODO EES-331 is this correct?
                // Original release note is not cleared if the update is not altering it
                Assert.Equal(methodology.InternalReleaseNote, viewModel.InternalReleaseNote);
                Assert.Equal(methodology.Published, viewModel.Published);
                Assert.Equal(request.Status, viewModel.Status);
                Assert.Equal(request.Title, viewModel.Title);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var model = await context.Methodologies.FindAsync(methodology.Id);

                Assert.True(model.Live);
                // Slug remains unchanged
                Assert.Equal("pupil-absence-statistics-methodology", model.Slug);
                Assert.True(model.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(model.Updated.Value).Milliseconds, 0, 1500);
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

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.AddRangeAsync(new List<Methodology>
                {
                    methodology1, methodology2
                });
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context);

                var result = (await service.UpdateMethodologyAsync(methodology1.Id, request));

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, SlugNotUnique);
            }
        }

        private static MethodologyService SetupMethodologyService(
            ContentDbContext contentDbContext,
            IMethodologyContentService methodologyContentService = null,
            IMethodologyRepository methodologyRepository = null,
            IMethodologyFileRepository methodologyFileRepository = null,
            IMethodologyImageService methodologyImageService = null,
            IPublishingService publishingService = null,
            ITestService testService = null,
            IUserService userService = null)
        {
            return new MethodologyService(
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                contentDbContext,
                AdminMapper(),
                methodologyContentService ?? new Mock<IMethodologyContentService>().Object,
                methodologyRepository ?? new MethodologyRepository(contentDbContext),
                methodologyFileRepository ?? new MethodologyFileRepository(contentDbContext),
                methodologyImageService ?? new Mock<IMethodologyImageService>().Object,
                publishingService ?? new Mock<IPublishingService>().Object,
                testService ?? new Mock<ITestService>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object);
        }
    }
}
