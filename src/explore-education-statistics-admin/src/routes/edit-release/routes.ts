import ReleaseContentPage from '@admin/pages/release/edit-release/content/ReleaseContentPage';
import ReleaseDataPage from '@admin/pages/release/edit-release/data/ReleaseDataPage';
import ReleaseManageDataBlocksPage from '@admin/pages/release/edit-release/manage-datablocks/ReleaseManageDataBlocksPage';
import ReleasePublishStatusPage from '@admin/pages/release/edit-release/status/ReleaseStatusPage';
import ReleaseSummaryEditPage from '@admin/pages/release/edit-release/summary/ReleaseSummaryEditPage';
import ReleaseSummaryPage from '@admin/pages/release/edit-release/summary/ReleaseSummaryPage';
import { ComponentType } from 'react';

export interface ReleaseRoute {
  path: string;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  component: ComponentType<any>;
  title: string;
  generateLink: (publicationId: string, releaseId: string) => string;
}

const createReadonlyRoute = (
  section: string,
  title: string,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  component: ComponentType<any>,
): ReleaseRoute => {
  const path = `/publication/:publicationId/release/:releaseId/${section}`;
  return {
    path,
    component,
    title,
    generateLink: (publicationId: string, releaseId: string) =>
      path
        .replace(':publicationId', publicationId)
        .replace(':releaseId', releaseId),
  };
};

const createEditRoute = (
  section: string,
  title: string,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  component: ComponentType<any>,
): ReleaseRoute => {
  const path = `/publication/:publicationId/release/:releaseId/${section}/edit`;
  return {
    path,
    component,
    title,
    generateLink: (publicationId: string, releaseId: string) =>
      path
        .replace(':publicationId', publicationId)
        .replace(':releaseId', releaseId),
  };
};

export const summaryRoute = createReadonlyRoute(
  'summary',
  'Release summary',
  ReleaseSummaryPage,
);
export const dataRoute = createReadonlyRoute(
  'data',
  'Manage data',
  ReleaseDataPage,
);
export const manageDataBlocksRoute = createReadonlyRoute(
  'manage-datablocks',
  'Manage data blocks',
  ReleaseManageDataBlocksPage,
);
export const contentRoute = createReadonlyRoute(
  'content',
  'Manage content',
  ReleaseContentPage,
);
export const publishStatusRoute = createReadonlyRoute(
  'status',
  'Release status',
  ReleasePublishStatusPage,
);
export const summaryEditRoute = createEditRoute(
  'summary',
  'Release summary',
  ReleaseSummaryEditPage,
);

export const viewRoutes = [
  summaryRoute,
  dataRoute,
  manageDataBlocksRoute,
  contentRoute,
  publishStatusRoute,
];

export const editRoutes = [summaryEditRoute];

export default {
  manageReleaseRoutes: [...viewRoutes, ...editRoutes],
  createReleaseRoute: {
    route: '/publication/:publicationId/create-release',
    generateLink: (publicationId: string) =>
      `/publication/${publicationId}/create-release`,
  },
};
