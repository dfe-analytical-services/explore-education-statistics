#nullable enable
using System;
using System.Collections.Generic;
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
using Microsoft.EntityFrameworkCore;
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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Single(contentDbContext.MethodologyVersions);

                var addedNote =
                    await contentDbContext.MethodologyNotes.SingleAsync(n =>
                        n.MethodologyVersionId == methodologyVersion.Id);

                Assert.Equal(request.Content, addedNote.Content);
                Assert.Equal(request.DisplayDate, addedNote.DisplayDate);
            }
        }

        [Fact]
        public async Task AddNote_NoDisplayDateDefaultsToToday()
        {
            var methodologyVersion = new MethodologyVersion();

            var request = new MethodologyNoteAddRequest
            {
                Content = "Adding note",
                DisplayDate = null
            };

            var today = DateTime.Today.ToUniversalTime();

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
                Assert.Equal(today, result.DisplayDate);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Single(contentDbContext.MethodologyVersions);

                var addedNote =
                    await contentDbContext.MethodologyNotes.SingleAsync(n =>
                        n.MethodologyVersionId == methodologyVersion.Id);

                Assert.Equal(request.Content, addedNote.Content);
                Assert.Equal(today, addedNote.DisplayDate);
            }
        }

        [Fact]
        public async Task AddNote_MethodologyVersionNotFound()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Empty(contentDbContext.MethodologyNotes);
            }
        }

        [Fact]
        public async Task DeleteNote()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Notes = new List<MethodologyNote>
                {
                    new()
                    {
                        Content = "Note 1"
                    },
                    new()
                    {
                        Content = "Note 2"
                    }
                }
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

                var result = await service.DeleteNote(
                    methodologyVersionId: methodologyVersion.Id,
                    methodologyNoteId: methodologyVersion.Notes[0].Id);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Single(contentDbContext.MethodologyNotes);
                Assert.Null(await contentDbContext.MethodologyNotes.FindAsync(methodologyVersion.Notes[0].Id));
                Assert.NotNull(await contentDbContext.MethodologyNotes.FindAsync(methodologyVersion.Notes[1].Id));
            }
        }

        [Fact]
        public async Task DeleteNote_DifferentMethodologyVersion()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Notes = new List<MethodologyNote>
                {
                    new()
                }
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

                // Attempt to delete the note using a different methodology version
                var result = await service.DeleteNote(
                    methodologyVersionId: Guid.NewGuid(),
                    methodologyNoteId: methodologyVersion.Notes[0].Id);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Single(contentDbContext.MethodologyNotes);
                Assert.NotNull(await contentDbContext.MethodologyNotes.FindAsync(methodologyVersion.Notes[0].Id));
            }
        }

        [Fact]
        public async Task DeleteNote_MethodologyNoteNotFound()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Notes = new List<MethodologyNote>
                {
                    new()
                }
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

                var result = await service.DeleteNote(
                    methodologyVersionId: methodologyVersion.Id,
                    methodologyNoteId: Guid.NewGuid());

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Single(contentDbContext.MethodologyNotes);
                Assert.NotNull(await contentDbContext.MethodologyNotes.FindAsync(methodologyVersion.Notes[0].Id));
            }
        }

        [Fact]
        public async Task UpdateNote()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Notes = new List<MethodologyNote>
                {
                    new()
                    {
                        Content = "Original note",
                        DisplayDate = DateTime.Today.AddDays(-7).ToUniversalTime()
                    }
                }
            };

            var request = new MethodologyNoteUpdateRequest
            {
                Content = "Updating note",
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

                var result = (await service.UpdateNote(
                    methodologyVersionId: methodologyVersion.Id,
                    methodologyNoteId: methodologyVersion.Notes[0].Id,
                    request: request)).AssertRight();

                Assert.Equal(methodologyVersion.Notes[0].Id, result.Id);
                Assert.Equal(request.Content, result.Content);
                Assert.Equal(request.DisplayDate, result.DisplayDate);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedNote =
                    await contentDbContext.MethodologyNotes.SingleAsync(n => n.Id == methodologyVersion.Notes[0].Id);

                Assert.Equal(request.Content, updatedNote.Content);
                Assert.Equal(request.DisplayDate, updatedNote.DisplayDate);
            }
        }

        [Fact]
        public async Task UpdateNote_NoDisplayDateDefaultsToToday()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Notes = new List<MethodologyNote>
                {
                    new()
                    {
                        Content = "Original note",
                        DisplayDate = DateTime.Today.AddDays(-7).ToUniversalTime()
                    }
                }
            };

            var request = new MethodologyNoteUpdateRequest
            {
                Content = "Updating note",
                DisplayDate = null
            };

            var today = DateTime.Today.ToUniversalTime();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyNoteService(contentDbContext);

                var result = (await service.UpdateNote(
                    methodologyVersionId: methodologyVersion.Id,
                    methodologyNoteId: methodologyVersion.Notes[0].Id,
                    request: request)).AssertRight();

                Assert.Equal(methodologyVersion.Notes[0].Id, result.Id);
                Assert.Equal(request.Content, result.Content);
                Assert.Equal(today, result.DisplayDate);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedNote =
                    await contentDbContext.MethodologyNotes.SingleAsync(n => n.Id == methodologyVersion.Notes[0].Id);

                Assert.Equal(request.Content, updatedNote.Content);
                Assert.Equal(today, updatedNote.DisplayDate);
            }
        }

        [Fact]
        public async Task UpdateNote_DifferentMethodologyVersion()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Notes = new List<MethodologyNote>
                {
                    new()
                    {
                        Content = "Original note",
                        DisplayDate = DateTime.Today.AddDays(-7).ToUniversalTime()
                    }
                }
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

                // Attempt to update the note using a different methodology version
                var result = await service.UpdateNote(
                    methodologyVersionId: Guid.NewGuid(),
                    methodologyNoteId: methodologyVersion.Notes[0].Id,
                    request: new MethodologyNoteUpdateRequest
                    {
                        Content = "Updating note",
                        DisplayDate = DateTime.Today.ToUniversalTime()
                    });

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyNote =
                    await contentDbContext.MethodologyNotes.SingleAsync(n => n.Id == methodologyVersion.Notes[0].Id);

                Assert.Equal("Original note", methodologyNote.Content);
                Assert.Equal(DateTime.Today.AddDays(-7).ToUniversalTime(), methodologyNote.DisplayDate);
            }
        }

        [Fact]
        public async Task UpdateNote_MethodologyNoteNotFound()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Notes = new List<MethodologyNote>
                {
                    new()
                    {
                        Content = "Original note",
                        DisplayDate = DateTime.Today.AddDays(-7).ToUniversalTime()
                    }
                }
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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyNote =
                    await contentDbContext.MethodologyNotes.SingleAsync(n => n.Id == methodologyVersion.Notes[0].Id);

                Assert.Equal("Original note", methodologyNote.Content);
                Assert.Equal(DateTime.Today.AddDays(-7).ToUniversalTime(), methodologyNote.DisplayDate);
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
