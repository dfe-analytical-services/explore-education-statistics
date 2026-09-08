import { NavRouteProps } from '@admin/routes/types';

export type MethodologyRouteProps = NavRouteProps;

export type MethodologyRouteParams = {
  methodologyId: string;
};

export const methodologySummaryRoute: MethodologyRouteProps = {
  path: '/methodology/:methodologyId/summary',
  title: 'Summary',
};

export const methodologySummaryEditRoute: MethodologyRouteProps = {
  path: '/methodology/:methodologyId/summary/edit',
  title: 'Edit summary',
};

export const methodologyContentRoute: MethodologyRouteProps = {
  path: '/methodology/:methodologyId/content',
  title: 'Manage content',
};

export const methodologyStatusRoute: MethodologyRouteProps = {
  path: '/methodology/:methodologyId/status',
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
