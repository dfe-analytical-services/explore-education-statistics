#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository
{
    public class MethodologyNoteRepository : IMethodologyNoteRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public MethodologyNoteRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<MethodologyNote> AddNote(
            Guid methodologyVersionId,
            Guid createdByUserId,
            string content,
            DateTime displayDate)
        {
            var added = (await _contentDbContext.MethodologyNotes.AddAsync(new MethodologyNote
            {
                Content = content,
                Created = DateTime.UtcNow,
                CreatedById = createdByUserId,
                DisplayDate = displayDate,
                MethodologyVersionId = methodologyVersionId
            })).Entity;
            await _contentDbContext.SaveChangesAsync();

            return added;
        }

        public async Task DeleteNote(Guid methodologyNoteId)
        {
            var methodologyNote = await _contentDbContext.MethodologyNotes.FindAsync(methodologyNoteId);
            _contentDbContext.Remove(methodologyNote);
            await _contentDbContext.SaveChangesAsync();
        }

        public async Task<MethodologyNote> UpdateNote(
            Guid methodologyNoteId,
            Guid updatedByUserId,
            string content,
            DateTime displayDate)
        {
            var methodologyNote = await _contentDbContext.MethodologyNotes.FindAsync(methodologyNoteId);
            _contentDbContext.Update(methodologyNote);

            methodologyNote.Content = content;
            methodologyNote.DisplayDate = displayDate;
            methodologyNote.Updated = DateTime.UtcNow;
            methodologyNote.UpdatedById = updatedByUserId;

            await _contentDbContext.SaveChangesAsync();

            return methodologyNote;
        }
    }
}
