import ReleaseSetupPage from '../pages/edit-release/ReleaseSetupPage';
import ReleaseDataPage from '../pages/edit-release/ReleaseDataPage';
import ReleaseBuildTablesPage from '../pages/edit-release/ReleaseBuildTablesPage';
import ReleaseTablesPage from '../pages/edit-release/ReleaseTablesPage';
import ReleaseContentPage from '../pages/edit-release/ReleaseContentPage';
import ReleasePublishStatusPage from '../pages/edit-release/ReleasePublishStatusPage';

export interface ReleaseRoute {
  path: string;
  component: (props: any) => JSX.Element;
  title: string;
  generateLink: (releaseId: string) => string;
}

const createReadonlyRoute = (
  section: string,
  title: string,
  component: (props: any) => JSX.Element,
): ReleaseRoute => {
  const path = `/edit-release/:releaseId/${section}`;
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
  component: (props: any) => JSX.Element,
): ReleaseRoute => {
  const readOnlyRoute = createReadonlyRoute(section, title, component);
  return {
    ...readOnlyRoute,
    path: `${readOnlyRoute}/edit`,
  };
};

export const setupRoute = createReadonlyRoute(
  'setup',
  'Release setup',
  ReleaseSetupPage,
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
// export const setupEditRoute = createEditRoute(
//   'setup',
//   'Release setup',
//   ReleaseSetupEditPage,
// );
// export const dataEditRoute = createEditRoute(
//   'data',
//   'Add / edit data',
//   ReleaseDataPage,
// );
// export const buildTablesEditRoute = createEditRoute(
//   'build-tables',
//   'Build tables',
//   ReleaseBuildTablesPage,
// );
// export const tablesEditRoute = createEditRoute(
//   'tables',
//   'View / edit tables',
//   ReleaseTablesPage,
// );
// export const contentEditRoute = createReadonlyRoute(
//   'content',
//   'Add / edit content',
//   ReleaseContentPage,
// );
// export const publishStatusEditRoute = createReadonlyRoute(
//   'publish-status',
//   'Set publish status',
//   ReleasePublishStatusPage,
// );

export default [
  setupRoute,
  dataRoute,
  buildTablesRoute,
  tablesRoute,
  contentRoute,
  publishStatusRoute,
];
