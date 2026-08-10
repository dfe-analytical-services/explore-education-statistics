import { RouteProps } from 'react-router';

export interface MethodologyRouteProps extends RouteProps {
  path: string;
  title: string;
}

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
