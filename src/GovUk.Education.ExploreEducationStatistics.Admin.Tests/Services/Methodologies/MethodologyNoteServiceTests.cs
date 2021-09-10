#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyNoteServiceTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        [Fact]
        public async Task AddNote()
        {
            var methodologyVersion = new MethodologyVersion();

            var request = new MethodologyNoteAddRequest
            {
                Content = "Adding note",
                DisplayDate = DateTime.Today.ToUniversalTime()
            };

            var expectedResult = new MethodologyNote
            {
                Id = Guid.NewGuid(),
                Content = request.Content,
                DisplayDate = request.DisplayDate
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyNoteRepository = new Mock<IMethodologyNoteRepository>(Strict);

            methodologyNoteRepository.Setup(mock => mock.AddNote(
                    methodologyVersion.Id,
                    UserId,
                    request.Content,
                    request.DisplayDate
                ))
                .ReturnsAsync(expectedResult);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyNoteService(contentDbContext: contentDbContext,
                    methodologyNoteRepository: methodologyNoteRepository.Object);

                var result = (await service.AddNote(
                    methodologyVersion.Id,
                    request)).AssertRight();

                VerifyAllMocks(methodologyNoteRepository);

                Assert.Equal(expectedResult.Id, result.Id);
                Assert.Equal(expectedResult.Content, result.Content);
                Assert.Equal(expectedResult.DisplayDate, result.DisplayDate);
            }
        }

        [Fact]
        public async Task AddNote_MethodologyVersionNotFound()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var service = SetupMethodologyNoteService(contentDbContext: contentDbContext);

            var result = await service.AddNote(
                Guid.NewGuid(),
                new MethodologyNoteAddRequest
                {
                    Content = "Adding note",
                    DisplayDate = DateTime.Today.ToUniversalTime()
                });

            result.AssertNotFound();
        }

        [Fact]
        public async Task DeleteNote()
        {
            var methodologyVersion = new MethodologyVersion();

            var methodologyNote = new MethodologyNote
            {
                MethodologyVersion = methodologyVersion
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.MethodologyNotes.AddAsync(methodologyNote);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyNoteRepository = new Mock<IMethodologyNoteRepository>(Strict);

            methodologyNoteRepository.Setup(mock => mock.DeleteNote(
                methodologyNote.Id
            )).Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyNoteService(contentDbContext: contentDbContext,
                    methodologyNoteRepository: methodologyNoteRepository.Object);

                var result = await service.DeleteNote(
                    methodologyVersion.Id,
                    methodologyNote.Id);

                VerifyAllMocks(methodologyNoteRepository);

                result.AssertRight();
            }
        }

        [Fact]
        public async Task DeleteNote_DifferentMethodologyVersion()
        {
            var methodologyNote = new MethodologyNote
            {
                MethodologyVersion = new MethodologyVersion()
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyNotes.AddAsync(methodologyNote);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyNoteService(contentDbContext: contentDbContext);

                // Attempt to delete the note using a different methodology version
                var result = await service.DeleteNote(
                    Guid.NewGuid(),
                    methodologyNote.Id);

                // No interaction with the repository is expected
                // since a note matching the id and methodology version id won't be found

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task DeleteNote_MethodologyNoteNotFound()
        {
            var methodologyVersion = new MethodologyVersion();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyNoteService(contentDbContext: contentDbContext);

                var result = await service.DeleteNote(
                    methodologyVersion.Id,
                    Guid.NewGuid());

                // No interaction with the repository is expected
                // since a note matching the id and methodology version id won't be found

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task UpdateNote()
        {
            var methodologyVersion = new MethodologyVersion();

            var methodologyNote = new MethodologyNote
            {
                Id = Guid.NewGuid(),
                MethodologyVersion = methodologyVersion
            };

            var request = new MethodologyNoteUpdateRequest
            {
                Content = "Updating note",
                DisplayDate = DateTime.Today.ToUniversalTime()
            };

            var expectedResult = new MethodologyNote
            {
                Id = methodologyNote.Id,
                Content = request.Content,
                DisplayDate = request.DisplayDate
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyNotes.AddAsync(methodologyNote);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyNoteRepository = new Mock<IMethodologyNoteRepository>(Strict);

            methodologyNoteRepository.Setup(mock => mock.UpdateNote(
                    methodologyNote.Id,
                    UserId,
                    request.Content,
                    request.DisplayDate
                ))
                .ReturnsAsync(expectedResult);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyNoteService(contentDbContext: contentDbContext,
                    methodologyNoteRepository: methodologyNoteRepository.Object);

                var result = (await service.UpdateNote(
                    methodologyVersion.Id,
                    methodologyNote.Id,
                    request)).AssertRight();

                VerifyAllMocks(methodologyNoteRepository);

                Assert.Equal(expectedResult.Id, result.Id);
                Assert.Equal(expectedResult.Content, result.Content);
                Assert.Equal(expectedResult.DisplayDate, result.DisplayDate);
            }
        }

        [Fact]
        public async Task UpdateNote_DifferentMethodologyVersion()
        {
            var methodologyNote = new MethodologyNote
            {
                MethodologyVersion = new MethodologyVersion()
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyNotes.AddAsync(methodologyNote);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyNoteService(contentDbContext: contentDbContext);

                // Attempt to update the note using a different methodology version
                var result = await service.UpdateNote(
                    Guid.NewGuid(),
                    methodologyNote.Id,
                    new MethodologyNoteUpdateRequest
                    {
                        Content = "Updating note",
                        DisplayDate = DateTime.Today.ToUniversalTime()
                    });

                // No interaction with the repository is expected
                // since a note matching the id and methodology version id won't be found

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task UpdateNote_MethodologyNoteNotFound()
        {
            var methodologyVersion = new MethodologyVersion();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyNoteService(contentDbContext: contentDbContext);

                var result = await service.UpdateNote(
                    methodologyVersion.Id,
                    Guid.NewGuid(),
                    new MethodologyNoteUpdateRequest
                    {
                        Content = "Updating note",
                        DisplayDate = DateTime.Today.ToUniversalTime()
                    });

                // No interaction with the repository is expected
                // since a note matching the id and methodology version id won't be found

                result.AssertNotFound();
            }
        }

        private static MethodologyNoteService SetupMethodologyNoteService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IMethodologyNoteRepository? methodologyNoteRepository = null,
            IUserService? userService = null)
        {
            return new(
                AdminMapper(),
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                methodologyNoteRepository ?? Mock.Of<IMethodologyNoteRepository>(),
                userService ?? AlwaysTrueUserService(UserId).Object);
        }
    }
}
