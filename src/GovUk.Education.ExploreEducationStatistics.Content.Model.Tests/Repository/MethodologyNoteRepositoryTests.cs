#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Repository
{
    public class MethodologyNoteRepositoryTests
    {
        [Fact]
        public async Task AddNote()
        {
            var methodologyVersion = new MethodologyVersion();

            const string content = "Adding note";
            var createdById = Guid.NewGuid();
            var displayDate = DateTime.Today.ToUniversalTime();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyNoteRepository(contentDbContext);

                var result = await service.AddNote(
                    methodologyVersionId: methodologyVersion.Id,
                    createdByUserId: createdById,
                    content: content,
                    displayDate: displayDate);

                Assert.NotNull(result);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                Assert.Single(contentDbContext.MethodologyNotes);

                var addedNote =
                    await contentDbContext.MethodologyNotes.SingleAsync(n =>
                        n.MethodologyVersionId == methodologyVersion.Id);

                Assert.NotEqual(Guid.Empty, addedNote.Id);
                Assert.Equal(content, addedNote.Content);
                Assert.InRange(DateTime.UtcNow.Subtract(addedNote.Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, addedNote.CreatedById);
                Assert.Equal(displayDate, addedNote.DisplayDate);
                Assert.Equal(methodologyVersion.Id, addedNote.MethodologyVersionId);
                Assert.Null(addedNote.Updated);
                Assert.Null(addedNote.UpdatedById);
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyNoteRepository(contentDbContext);
                await service.DeleteNote(methodologyVersion.Notes[0].Id);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                Assert.Single(contentDbContext.MethodologyNotes);

                Assert.Null(await contentDbContext.MethodologyNotes.FindAsync(methodologyVersion.Notes[0].Id));
                Assert.NotNull(await contentDbContext.MethodologyNotes.FindAsync(methodologyVersion.Notes[1].Id));
            }
        }

        [Fact]
        public async Task UpdateNote()
        {
            var methodologyNote = new MethodologyNote
            {
                Content = "Original note",
                Created = DateTime.UtcNow,
                CreatedById = Guid.NewGuid(),
                DisplayDate = DateTime.Today.ToUniversalTime(),
                MethodologyVersion = new MethodologyVersion()
            };

            const string content = "Updating note";
            var updatedById = Guid.NewGuid();
            var displayDate = DateTime.Today.AddDays(-7).ToUniversalTime();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyNotes.AddAsync(methodologyNote);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyNoteRepository(contentDbContext);

                var result = await service.UpdateNote(
                    methodologyNoteId: methodologyNote.Id,
                    updatedByUserId: updatedById,
                    content: content,
                    displayDate: displayDate);

                Assert.NotNull(result);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                Assert.Single(contentDbContext.MethodologyNotes);

                var updatedNote = await contentDbContext.MethodologyNotes.SingleAsync(n => n.Id == methodologyNote.Id);

                Assert.Equal(content, updatedNote.Content);
                Assert.Equal(methodologyNote.Created, updatedNote.Created);
                Assert.Equal(methodologyNote.CreatedById, updatedNote.CreatedById);
                Assert.Equal(displayDate, updatedNote.DisplayDate);
                Assert.Equal(methodologyNote.MethodologyVersion.Id, updatedNote.MethodologyVersionId);
                Assert.NotNull(updatedNote.Updated);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedNote.Updated!.Value).Milliseconds, 0, 1500);
                Assert.Equal(updatedById, updatedNote.UpdatedById);
            }
        }

        private static MethodologyNoteRepository BuildMethodologyNoteRepository(ContentDbContext contentDbContext)
        {
            return new(contentDbContext);
        }
    }
}
