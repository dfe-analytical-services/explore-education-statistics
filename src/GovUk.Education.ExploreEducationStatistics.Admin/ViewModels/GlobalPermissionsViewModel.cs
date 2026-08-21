namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record GlobalPermissionsViewModel(
    bool CanAccessSystem,
    bool CanAccessAnalystPages,
    bool CanAccessAllImports,
    bool CanManageAllTaxonomy,
    bool CanManagePublicApiDataSets,
    bool IsBauUser,
    bool IsApprover
);
