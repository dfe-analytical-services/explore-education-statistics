import EditReleaseSetupPage from '../pages/edit-release/EditReleaseSetupPage';
import EditReleaseDataPage from '../pages/edit-release/EditReleaseDataPage';
import EditReleaseBuildTablesPage from '../pages/edit-release/EditReleaseBuildTablesPage';
import EditReleaseTablesPage from '../pages/edit-release/EditReleaseTablesPage';
import EditReleaseContentPage from '../pages/edit-release/EditReleaseContentPage';
import EditReleasePublishStatusPage from '../pages/edit-release/EditReleasePublishStatusPage';

export interface EditReleaseRoute {
  path: string;
  /* eslint-disable-next-line @typescript-eslint/no-explicit-any */
  component: (props: any) => JSX.Element;
  title: string;
  generateLink: (releaseId: string) => string;
}

const createRoute = (
  section: string,
  title: string,
  /* eslint-disable-next-line @typescript-eslint/no-explicit-any */
  component: (props: any) => JSX.Element,
): EditReleaseRoute => {
  const path = `/edit-release/:releaseId/${section}`;
  return {
    path,
    component,
    title,
    generateLink: (releaseId: string) => path.replace(':releaseId', releaseId),
  };
};

export const setupRoute = createRoute(
  'setup',
  'Release setup',
  EditReleaseSetupPage,
);
export const dataRoute = createRoute(
  'data',
  'Add / edit data',
  EditReleaseDataPage,
);
export const buildTablesRoute = createRoute(
  'build-tables',
  'Build tables',
  EditReleaseBuildTablesPage,
);
export const tablesRoute = createRoute(
  'tables',
  'View / edit tables',
  EditReleaseTablesPage,
);
export const contentRoute = createRoute(
  'content',
  'Add / edit content',
  EditReleaseContentPage,
);
export const publishStatusRoute = createRoute(
  'publish-status',
  'Set publish status',
  EditReleasePublishStatusPage,
);

export default [
  setupRoute,
  dataRoute,
  buildTablesRoute,
  tablesRoute,
  contentRoute,
  publishStatusRoute,
];
