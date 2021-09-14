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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

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

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyNoteService(contentDbContext);

                var result = (await service.AddNote(
                    methodologyVersionId: methodologyVersion.Id,
                    request: request)).AssertRight();

                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(request.Content, result.Content);
                Assert.Equal(request.DisplayDate, result.DisplayDate);
            }
        }

        [Fact]
        public async Task AddNote_MethodologyVersionNotFound()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var service = SetupMethodologyNoteService(contentDbContext);

            var result = await service.AddNote(
                methodologyVersionId: Guid.NewGuid(),
                request: new MethodologyNoteAddRequest
                {
                    Content = "Adding note",
                    DisplayDate = DateTime.Today.ToUniversalTime()
                });

            result.AssertNotFound();
        }

        [Fact]
        public async Task DeleteNote()
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
                var service = SetupMethodologyNoteService(contentDbContext);

                var result = await service.DeleteNote(
                    methodologyVersionId: methodologyNote.MethodologyVersionId,
                    methodologyNoteId: methodologyNote.Id);

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
                var service = SetupMethodologyNoteService(contentDbContext);

                // Attempt to delete the note using a different methodology version
                var result = await service.DeleteNote(
                    methodologyVersionId: Guid.NewGuid(),
                    methodologyNoteId: methodologyNote.Id);

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
                var service = SetupMethodologyNoteService(contentDbContext);

                var result = await service.DeleteNote(
                    methodologyVersionId: methodologyVersion.Id,
                    methodologyNoteId: Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task UpdateNote()
        {
            var methodologyNote = new MethodologyNote
            {
                MethodologyVersion = new MethodologyVersion()
            };

            var request = new MethodologyNoteUpdateRequest
            {
                Content = "Updating note",
                DisplayDate = DateTime.Today.ToUniversalTime()
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyNotes.AddAsync(methodologyNote);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyNoteService(contentDbContext);

                var result = (await service.UpdateNote(
                    methodologyVersionId: methodologyNote.MethodologyVersionId,
                    methodologyNoteId: methodologyNote.Id,
                    request: request)).AssertRight();

                Assert.Equal(methodologyNote.Id, result.Id);
                Assert.Equal(request.Content, result.Content);
                Assert.Equal(request.DisplayDate, result.DisplayDate);
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
                var service = SetupMethodologyNoteService(contentDbContext);

                // Attempt to update the note using a different methodology version
                var result = await service.UpdateNote(
                    methodologyVersionId: Guid.NewGuid(),
                    methodologyNoteId: methodologyNote.Id,
                    request: new MethodologyNoteUpdateRequest
                    {
                        Content = "Updating note",
                        DisplayDate = DateTime.Today.ToUniversalTime()
                    });

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
                var service = SetupMethodologyNoteService(contentDbContext);

                var result = await service.UpdateNote(
                    methodologyVersionId: methodologyVersion.Id,
                    methodologyNoteId: Guid.NewGuid(),
                    request: new MethodologyNoteUpdateRequest
                    {
                        Content = "Updating note",
                        DisplayDate = DateTime.Today.ToUniversalTime()
                    });

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
                methodologyNoteRepository ?? new MethodologyNoteRepository(contentDbContext),
                userService ?? AlwaysTrueUserService(UserId).Object);
        }
    }
}
