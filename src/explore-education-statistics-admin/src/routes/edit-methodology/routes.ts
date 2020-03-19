import MethodologyContentPage from '@admin/pages/methodology/edit-methodology/content/MethodologyContentPage';
import MethodologyStatusPage from '@admin/pages/methodology/edit-methodology/status/MethodologyStatusPage';
import MethodologySummaryPage from '@admin/pages/methodology/edit-methodology/summary/MethodologySummaryPage';
import { ComponentType } from 'react';

export interface MethodologyRoute {
  path: string;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  component: ComponentType<any>;
  title: string;
  generateLink: (methodologyId: string) => string;
}

const createReadonlyRoute = (
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
      path.replace(':methodologyId', methodologyId),
  };
};

export const summaryRoute = createReadonlyRoute(
  'summary',
  'Summary',
  MethodologySummaryPage,
);
export const contentRoute = createReadonlyRoute(
  'content',
  'Manage content',
  MethodologyContentPage,
);
export const publishStatusRoute = createReadonlyRoute(
  'status',
  'Release status',
  MethodologyStatusPage,
);

const routes: MethodologyRoute[] = [contentRoute, publishStatusRoute];

export default routes;
