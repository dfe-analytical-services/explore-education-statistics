import MethodologyContentPage from '@admin/pages/methodology/edit-methodology/content/MethodologyContentPage';
import MethodologySummaryEditPage from '@admin/pages/methodology/edit-methodology/summary/MethodologySummaryEditPage';
import MethodologyStatusPage from '@admin/pages/methodology/edit-methodology/status/MethodologyStatusPage';
import MethodologySummaryPage from '@admin/pages/methodology/edit-methodology/summary/MethodologySummaryPage';
import { ComponentType } from 'react';
import { generatePath } from 'react-router';

export interface MethodologyRoute {
  path: string;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  component: ComponentType<any>;
  title: string;
  generateLink: (methodologyId: string) => string;
}

export interface MethodologyRouteParams {
  methodologyId: string;
}

const createRoute = (
  section: string,
  title: string,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  component: ComponentType<any>,
): MethodologyRoute => {
  const path = `/methodologies/:methodologyId/${section}`;
  return {
    path,
    component,
    title,
    generateLink: (methodologyId: string) =>
      generatePath(path, { methodologyId }),
  };
};

export const summaryRoute = createRoute(
  'summary',
  'Summary',
  MethodologySummaryPage,
);

export const summaryEditRoute = createRoute(
  'summary/edit',
  'Edit summary',
  MethodologySummaryEditPage,
);

export const contentRoute = createRoute(
  'content',
  'Manage content',
  MethodologyContentPage,
);

export const publishStatusRoute = createRoute(
  'status',
  'Release status',
  MethodologyStatusPage,
);

const routes: MethodologyRoute[] = [
  summaryRoute,
  summaryEditRoute,
  contentRoute,
  publishStatusRoute,
];

export const navRoutes: MethodologyRoute[] = [
  summaryRoute,
  contentRoute,
  publishStatusRoute,
];

export default routes;
