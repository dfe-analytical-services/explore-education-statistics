#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record GlobalPermissionsViewModel(bool CanAccessSystem,
    bool CanAccessAnalystPages,
    bool CanAccessAllImports,
    bool CanAccessPrereleasePages,
    bool CanAccessUserAdministrationPages,
    bool CanManageAllTaxonomy);
