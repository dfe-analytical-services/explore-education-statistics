import { ProtectedRouteProps } from '@admin/components/ProtectedRoute';
import LegacyReleaseCreatePage from '@admin/pages/legacy-releases/LegacyReleaseCreatePage';
import LegacyReleaseEditPage from '@admin/pages/legacy-releases/LegacyReleaseEditPage';
import LegacyReleasesPage from '@admin/pages/legacy-releases/LegacyReleasesPage';

export const legacyReleasesIndexRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/legacy-releases',
  component: LegacyReleasesPage,
  protectionAction: user => user.permissions.canAccessUserAdministrationPages,
  exact: true,
};

export const legacyReleaseCreateRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/legacy-releases/create',
  component: LegacyReleaseCreatePage,
  protectionAction: user => user.permissions.canAccessUserAdministrationPages,
  exact: true,
};

export const legacyReleaseEditRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/legacy-releases/:legacyReleaseId/edit',
  component: LegacyReleaseEditPage,
  protectionAction: user => user.permissions.canAccessUserAdministrationPages,
  exact: true,
};
