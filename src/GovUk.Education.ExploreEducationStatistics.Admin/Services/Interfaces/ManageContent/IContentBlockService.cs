#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;

public interface IContentBlockService
{
    Task DeleteContentBlockAndReorder(Guid blockToRemoveId);

    Task DeleteSectionContentBlocks(Guid contentSectionId);
}
