import { ProtectedRouteProps } from '@admin/components/ProtectedRoute';
import LegacyReleaseCreatePage from '@admin/pages/legacy-releases/LegacyReleaseCreatePage';
import LegacyReleaseEditPage from '@admin/pages/legacy-releases/LegacyReleaseEditPage';

export type LegacyReleaseRouteParams = {
  publicationId: string;
  legacyReleaseId?: string;
};

export const legacyReleaseCreateRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/legacy-releases/create',
  component: LegacyReleaseCreatePage,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};

export const legacyReleaseEditRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/legacy-releases/:legacyReleaseId/edit',
  component: LegacyReleaseEditPage,
  protectionAction: user => user.permissions.canAccessAnalystPages,
  exact: true,
};
