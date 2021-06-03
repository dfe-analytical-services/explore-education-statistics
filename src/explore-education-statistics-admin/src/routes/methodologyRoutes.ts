import MethodologyContentPage from '@admin/pages/methodology/edit-methodology/content/MethodologyContentPage';
import MethodologyStatusPage from '@admin/pages/methodology/edit-methodology/status/MethodologyStatusPage';
import MethodologySummaryEditPage from '@admin/pages/methodology/edit-methodology/summary/MethodologySummaryEditPage';
import MethodologySummaryPage from '@admin/pages/methodology/edit-methodology/summary/MethodologySummaryPage';
import { RouteProps } from 'react-router';

export interface MethodologyRouteProps extends RouteProps {
  path: string;
  title: string;
}

export type MethodologyRouteParams = {
  methodologyId: string;
  publicationId: string;
};

export const methodologySummaryRoute: MethodologyRouteProps = {
  path: '/publication/:publicationId/methodology/:methodologyId/summary',
  title: 'Summary',
  component: MethodologySummaryPage,
};

export const methodologySummaryEditRoute: MethodologyRouteProps = {
  path: '/publication/:publicationId/methodology/:methodologyId/summary/edit',
  title: 'Edit summary',
  component: MethodologySummaryEditPage,
};

export const methodologyContentRoute: MethodologyRouteProps = {
  path: '/publication/:publicationId/methodology/:methodologyId/content',
  title: 'Manage content',
  component: MethodologyContentPage,
};

export const methodologyStatusRoute: MethodologyRouteProps = {
  path: '/publication/:publicationId/methodology/:methodologyId/status',
  title: 'Sign off',
  component: MethodologyStatusPage,
};
