#nullable enable
using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces
{
    public interface IMethodologyNoteRepository
    {
        Task<MethodologyNote> AddNote(
            Guid methodologyVersionId,
            Guid createdByUserId,
            string content,
            DateTime displayDate);

        Task DeleteNote(Guid methodologyNoteId);

        Task<MethodologyNote> UpdateNote(
            Guid methodologyNoteId,
            Guid updatedByUserId,
            string content,
            DateTime displayDate);
    }
}
