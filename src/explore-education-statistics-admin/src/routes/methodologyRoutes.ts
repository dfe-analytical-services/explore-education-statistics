import { NavRouteProps } from '@admin/routes/types';

export type MethodologyRouteProps = NavRouteProps;

export type MethodologyRouteParams = {
  methodologyId: string;
};

export const methodologySummaryRoute: MethodologyRouteProps = {
  fullPath: '/methodology/:methodologyId/summary',
  path: 'summary',
  title: 'Summary',
};

export const methodologySummaryEditRoute: MethodologyRouteProps = {
  fullPath: '/methodology/:methodologyId/summary/edit',
  path: 'summary/edit',
  title: 'Edit summary',
};

export const methodologyContentRoute: MethodologyRouteProps = {
  fullPath: '/methodology/:methodologyId/content',
  path: 'content',
  title: 'Manage content',
};

export const methodologyStatusRoute: MethodologyRouteProps = {
  fullPath: '/methodology/:methodologyId/status',
  path: 'status',
  title: 'Sign off',
};

/**
 * The routes shown in the methodology nav bar, in the order they are stepped
 * through by the previous/next links.
 */
export const methodologyNavRoutes: MethodologyRouteProps[] = [
  methodologySummaryRoute,
  methodologyContentRoute,
  methodologyStatusRoute,
];
