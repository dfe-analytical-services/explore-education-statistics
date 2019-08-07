import ReleaseSummaryEditPage from '@admin/pages/release/edit-release/summary/ReleaseSummaryEditPage';
import ReleaseSummaryPage from '@admin/pages/release/edit-release/summary/ReleaseSummaryPage';
import ReleaseDataPage from '@admin/pages/release/edit-release/data/ReleaseDataPage';
import ReleaseBuildTablesPage from '@admin/pages/release/edit-release/ReleaseBuildTablesPage';
import ReleaseTablesPage from '@admin/pages/release/edit-release/ReleaseTablesPage';
import ReleaseContentPage from '@admin/pages/release/edit-release/ReleaseContentPage';
import ReleasePublishStatusPage from '@admin/pages/release/edit-release/ReleasePublishStatusPage';

export interface ReleaseRoute {
  path: string;
  /* eslint-disable-next-line @typescript-eslint/no-explicit-any */
  component: (props: any) => JSX.Element;
  title: string;
  generateLink: (releaseId: string) => string;
}

const createReadonlyRoute = (
  section: string,
  title: string,
  /* eslint-disable-next-line @typescript-eslint/no-explicit-any */
  component: (props: any) => JSX.Element,
): ReleaseRoute => {
  const path = `/release/:releaseId/${section}`;
  return {
    path,
    component,
    title,
    generateLink: (releaseId: string) => path.replace(':releaseId', releaseId),
  };
};

const createEditRoute = (
  section: string,
  title: string,
  /* eslint-disable-next-line @typescript-eslint/no-explicit-any */
  component: (props: any) => JSX.Element,
): ReleaseRoute => {
  const path = `/release/:releaseId/${section}/edit`;
  return {
    path,
    component,
    title,
    generateLink: (releaseId: string) => path.replace(':releaseId', releaseId),
  };
};

export const setupRoute = createReadonlyRoute(
  'setup',
  'Release setup',
  ReleaseSummaryPage,
);
export const dataRoute = createReadonlyRoute(
  'data',
  'Add / edit data',
  ReleaseDataPage,
);
export const buildTablesRoute = createReadonlyRoute(
  'build-tables',
  'Build tables',
  ReleaseBuildTablesPage,
);
export const tablesRoute = createReadonlyRoute(
  'tables',
  'View / edit tables',
  ReleaseTablesPage,
);
export const contentRoute = createReadonlyRoute(
  'content',
  'Add / edit content',
  ReleaseContentPage,
);
export const publishStatusRoute = createReadonlyRoute(
  'publish-status',
  'Set publish status',
  ReleasePublishStatusPage,
);
export const setupEditRoute = createEditRoute(
  'setup',
  'Release setup',
  ReleaseSummaryEditPage,
);
export const dataEditRoute = createEditRoute(
  'data',
  'Add / edit data',
  ReleaseDataPage,
);
export const buildTablesEditRoute = createEditRoute(
  'build-tables',
  'Build tables',
  ReleaseBuildTablesPage,
);
export const tablesEditRoute = createEditRoute(
  'tables',
  'View / edit tables',
  ReleaseTablesPage,
);
export const contentEditRoute = createEditRoute(
  'content',
  'Add / edit content',
  ReleaseContentPage,
);
export const publishStatusEditRoute = createEditRoute(
  'publish-status',
  'Set publish status',
  ReleasePublishStatusPage,
);

export const viewRoutes = [
  setupRoute,
  dataRoute,
  buildTablesRoute,
  tablesRoute,
  contentRoute,
  publishStatusRoute,
];

export const editRoutes = [
  setupEditRoute,
  dataEditRoute,
  buildTablesEditRoute,
  tablesEditRoute,
  contentEditRoute,
  publishStatusEditRoute,
];

export default {
  manageReleaseRoutes: [...viewRoutes, ...editRoutes],
  createReleaseRoute: {
    route: '/publication/:publicationId/create-release',
    generateLink: (publicationId: string) =>
      `/publication/${publicationId}/create-release`,
  },
};
