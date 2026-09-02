import { ProtectedRouteProps, PublicRouteProps } from './types';

export type ThemeParams = {
  themeId: string;
};

export const signInRoute: PublicRouteProps = {
  fullPath: '/sign-in',
  path: 'sign-in',
};

export const signedOutRoute: PublicRouteProps = {
  fullPath: '/signed-out',
  path: 'signed-out',
};

export const expiredInviteRoute: PublicRouteProps = {
  fullPath: '/expired-invite',
  path: 'expired-invite',
};

export const noInvitationRoute: PublicRouteProps = {
  fullPath: '/no-invitation',
  path: 'no-invitation',
};

export const homeRoute: ProtectedRouteProps = {
  fullPath: '/',
  path: '',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const publishersGuideRoute: ProtectedRouteProps = {
  fullPath: '/publishers-guide',
  path: 'publishers-guide',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const dashboardRoute: ProtectedRouteProps = {
  fullPath: '/dashboard',
  path: 'dashboard',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const contactUsRoute: ProtectedRouteProps = {
  fullPath: '/contact-us',
  path: 'contact-us',
};

export const themesRoute: ProtectedRouteProps = {
  fullPath: '/themes',
  path: 'themes',
  protectionAction: permissions => permissions.canManageAllTaxonomy,
};

export const themeCreateRoute: ProtectedRouteProps = {
  fullPath: '/themes/create',
  path: 'themes/create',
  protectionAction: permissions => permissions.canManageAllTaxonomy,
};

export const themeEditRoute: ProtectedRouteProps = {
  fullPath: '/themes/:themeId/edit',
  path: 'themes/:themeId/edit',
  protectionAction: permissions => permissions.canManageAllTaxonomy,
};

export const publicationCreateRoute: ProtectedRouteProps = {
  fullPath: '/theme/:themeId/publications/create',
  path: 'theme/:themeId/publications/create',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const publicationRoute: ProtectedRouteProps = {
  fullPath: '/publication/:publicationId/*',
  path: 'publication/:publicationId/*',

  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const methodologyRoute: ProtectedRouteProps = {
  fullPath: '/methodology/:methodologyId/*',
  path: 'methodology/:methodologyId/*',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseRoute: ProtectedRouteProps = {
  fullPath: '/publication/:publicationId/release/:releaseVersionId/*',
  path: 'publication/:publicationId/release/:releaseVersionId/*',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const releaseCreateRoute: ProtectedRouteProps = {
  fullPath: '/publication/:publicationId/create-release',
  path: 'publication/:publicationId/create-release',
  protectionAction: permissions => permissions.canAccessAnalystPages,
};

export const preReleaseRoute: ProtectedRouteProps = {
  fullPath: '/publication/:publicationId/release/:releaseVersionId/prerelease',
  path: 'publication/:publicationId/release/:releaseVersionId/prerelease',
};

export const educationInNumbersListRoute: ProtectedRouteProps = {
  fullPath: '/education-in-numbers',
  path: 'education-in-numbers',
  protectionAction: permissions => permissions.isBauUser,
};

export const educationInNumbersCreateRoute: ProtectedRouteProps = {
  fullPath: '/education-in-numbers/create',
  path: 'education-in-numbers/create',
  protectionAction: permissions => permissions.isBauUser,
};

export const educationInNumbersRoute: ProtectedRouteProps = {
  fullPath: '/education-in-numbers/:educationInNumbersPageId',
  path: 'education-in-numbers/:educationInNumbersPageId',
  protectionAction: permissions => permissions.isBauUser,
};
