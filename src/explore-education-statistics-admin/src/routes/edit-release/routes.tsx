import ReleaseContentPage from '@admin/pages/release/edit-release/content/ReleaseContentPage';
import ReleaseDataPage from '@admin/pages/release/edit-release/data/ReleaseDataPage';
import ReleaseManageDataBlocksPage, {
  ReleaseManageDataBlocksPageParams,
} from '@admin/pages/release/edit-release/manage-datablocks/ReleaseManageDataBlocksPage';
import ReleasePublishStatusPage from '@admin/pages/release/edit-release/ReleaseStatusPage';
import ReleaseSummaryEditPage from '@admin/pages/release/edit-release/summary/ReleaseSummaryEditPage';
import ReleaseSummaryPage from '@admin/pages/release/edit-release/summary/ReleaseSummaryPage';
import permissionService from '@admin/services/permissions/permissionService';
import Gate from '@common/components/Gate';
import React, { ComponentType } from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

type ReleaseRouteParams = { publicationId: string; releaseId: string };

export interface ReleaseRoute<Params extends ReleaseRouteParams> {
  path: string;
  component: ComponentType<RouteComponentProps<Params>>;
  title: string;
  generateLink: (params: Params) => string;
}

const createReadonlyRoute = <
  Params extends ReleaseRouteParams = ReleaseRouteParams
>(
  section: string,
  title: string,
  component: ComponentType<RouteComponentProps<Params>>,
): ReleaseRoute<Params> => {
  const path = `/publication/:publicationId/release/:releaseId/${section}`;
  return {
    path,
    component,
    title,
    generateLink: params => generatePath(path, params),
  };
};

const createEditRoute = <
  Params extends ReleaseRouteParams = ReleaseRouteParams
>(
  section: string,
  title: string,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  component: ComponentType<any>,
): ReleaseRoute<Params> => {
  const path = `/publication/:publicationId/release/:releaseId/${section}/edit`;
  return {
    path,
    component,
    title,
    generateLink: params => generatePath(path, params),
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
export const manageDataBlocksRoute = createReadonlyRoute<
  ReleaseManageDataBlocksPageParams
>('datablocks/:dataBlockId?', 'Manage data blocks', props => {
  const { releaseId } = props.match.params;

  return (
    <Gate
      condition={() => permissionService.canUpdateRelease(releaseId)}
      fallback={<p>This release is currently not editable.</p>}
    >
      <ReleaseManageDataBlocksPage {...props} />
    </Gate>
  );
});

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
      generatePath(`/publication/:publicationId/create-release`, {
        publicationId,
      }),
  },
};
