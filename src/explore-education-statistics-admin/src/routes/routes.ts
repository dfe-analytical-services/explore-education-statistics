import { ProtectedRouteProps } from '@admin/components/ProtectedRoute';
import { RouteProps } from 'react-router';

interface PublicRouteProps extends RouteProps {
  path: string;
}

export type PublicationRouteParams = {
  publicationId: string;
};

export type ThemeParams = {
  themeId: string;
};

export const signInRoute: PublicRouteProps = {
  path: '/sign-in',
  exact: true,
};

export const signedOutRoute: PublicRouteProps = {
  path: '/signed-out',
  exact: true,
};

export const expiredInviteRoute: PublicRouteProps = {
  path: '/expired-invite',
  exact: true,
};

export const noInvitationRoute: PublicRouteProps = {
  path: '/no-invitation',
  exact: true,
};

export const homeRoute: ProtectedRouteProps = {
  path: '/',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const publishersGuideRoute: ProtectedRouteProps = {
  path: '/publishers-guide',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const dashboardRoute: ProtectedRouteProps = {
  path: '/dashboard',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const contactUsRoute: ProtectedRouteProps = {
  path: '/contact-us',
  exact: true,
};

export const themesRoute: ProtectedRouteProps = {
  path: '/themes',
  protectionAction: permissions => permissions.canManageAllTaxonomy,
  exact: true,
};

export const themeCreateRoute: ProtectedRouteProps = {
  path: '/themes/create',
  protectionAction: permissions => permissions.canManageAllTaxonomy,
  exact: true,
};

export const themeEditRoute: ProtectedRouteProps = {
  path: '/themes/:themeId/edit',
  protectionAction: permissions => permissions.canManageAllTaxonomy,
  exact: true,
};

export const publicationCreateRoute: ProtectedRouteProps = {
  path: '/theme/:themeId/publications/create',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const publicationRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const methodologyRoute: ProtectedRouteProps = {
  path: '/methodology/:methodologyId',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseCreateRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/create-release',
  protectionAction: permissions => permissions.canAccessAnalystPages,
  exact: true,
};

export const preReleaseRoute: ProtectedRouteProps = {
  path: '/publication/:publicationId/release/:releaseVersionId/prerelease',
};

export const educationInNumbersListRoute: ProtectedRouteProps = {
  path: '/education-in-numbers',
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const educationInNumbersCreateRoute: ProtectedRouteProps = {
  path: '/education-in-numbers/create',
  protectionAction: permissions => permissions.isBauUser,
  exact: true,
};

export const educationInNumbersRoute: ProtectedRouteProps = {
  path: '/education-in-numbers/:educationInNumbersPageId',
  protectionAction: permissions => permissions.isBauUser,
};
