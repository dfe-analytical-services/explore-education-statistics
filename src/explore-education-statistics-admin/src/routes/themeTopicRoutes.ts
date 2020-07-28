import { ProtectedRouteProps } from '@admin/components/ProtectedRoute';
import PublicationAssignMethodologyPage from '@admin/pages/theme/topic/publication/PublicationAssignMethodologyPage';
import PublicationCreatePage from '@admin/pages/theme/topic/PublicationCreatePage';

export type ThemeTopicParams = {
  themeId: string;
  topicId: string;
};

export type ThemeTopicPublicationParams = ThemeTopicParams & {
  publicationId: string;
};

export const publicationCreateRoute: ProtectedRouteProps = {
  path: '/theme/:themeId/topic/:topicId/create-publication',
  component: PublicationCreatePage,
  protectionAction: user => user.permissions.canAccessUserAdministrationPages,
  exact: true,
};

export const publicationAssignMethodologyRoute: ProtectedRouteProps = {
  path: `/theme/:themeId/topic/:topicId/publication/:publicationId/assign-methodology`,
  component: PublicationAssignMethodologyPage,
  protectionAction: user => user.permissions.canAccessUserAdministrationPages,
  exact: true,
};
