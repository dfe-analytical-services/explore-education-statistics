import ReleaseDataPage from '@admin/pages/release/edit-release/data/ReleaseDataPage';
import ReleaseContentPage from '@admin/pages/release/edit-release/ReleaseContentPage';
import ReleaseDataBlocksPage from '@admin/pages/release/edit-release/ReleaseDataBlocksPage';
import ReleasePublishStatusPage from '@admin/pages/release/edit-release/ReleaseStatusPage';
import ReleaseSummaryEditPage from '@admin/pages/release/edit-release/summary/ReleaseSummaryEditPage';
import ReleaseSummaryPage from '@admin/pages/release/edit-release/summary/ReleaseSummaryPage';

export interface ReleaseRoute {
  path: string;
  /* eslint-disable-next-line @typescript-eslint/no-explicit-any */
  component: (props: any) => JSX.Element;
  title: string;
  generateLink: (publicationId: string, releaseId: string) => string;
}

const createReadonlyRoute = (
  section: string,
  title: string,
  /* eslint-disable-next-line @typescript-eslint/no-explicit-any */
  component: (props: any) => JSX.Element,
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
  /* eslint-disable-next-line @typescript-eslint/no-explicit-any */
  component: (props: any) => JSX.Element,
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
export const dataBlocksRoute = createReadonlyRoute(
  'data-blocks',
  'Manage data blocks',
  ReleaseDataBlocksPage,
);
export const contentRoute = createReadonlyRoute(
  'content',
  'Manage content',
  ReleaseContentPage,
);
export const publishStatusRoute = createReadonlyRoute(
  'status',
  'Update release status',
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
  dataBlocksRoute,
  contentRoute,
  publishStatusRoute,
];

export const editRoutes = [
  summaryEditRoute,
];

export default {
  manageReleaseRoutes: [...viewRoutes, ...editRoutes],
  createReleaseRoute: {
    route: '/publication/:publicationId/create-release',
    generateLink: (publicationId: string) =>
      `/publication/${publicationId}/create-release`,
  },
};
