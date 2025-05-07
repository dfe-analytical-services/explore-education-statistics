#nullable enable

using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Utils;

public class PublicationArchiveStatusTransitionResolver
{
    /// <summary>
    /// Represents the possible transitions of a publication's archive status.
    /// </summary>
    public enum PublicationArchiveStatusTransition
    {
        NotArchivedToNotArchived,
        NotArchivedToArchived,
        ArchivedToNotArchived,
        ArchivedToArchived
    }

    /// <summary>
    /// Determines the archive status transition of a publication based on its superseding publication
    /// before and after an update.
    /// </summary>
    /// <param name="before">The publication that superseded the current one before the update, or null if none.</param>
    /// <param name="after">The publication that superseded the current one after the update, or null if none.</param>
    /// <returns>
    /// A <see cref="PublicationArchiveStatusTransition"/> value representing the transition of the publication's archive status.
    /// </returns>
    internal static PublicationArchiveStatusTransition GetTransition(Publication? before, Publication? after) =>
        (IsSupersededByLivePublication(before), IsSupersededByLivePublication(after)) switch
        {
            (false, false) => PublicationArchiveStatusTransition.NotArchivedToNotArchived,
            (false, true) => PublicationArchiveStatusTransition.NotArchivedToArchived,
            (true, false) => PublicationArchiveStatusTransition.ArchivedToNotArchived,
            (true, true) => PublicationArchiveStatusTransition.ArchivedToArchived
        };

    private static bool IsSupersededByLivePublication(Publication? publication) =>
        publication?.Live ?? false;
}
