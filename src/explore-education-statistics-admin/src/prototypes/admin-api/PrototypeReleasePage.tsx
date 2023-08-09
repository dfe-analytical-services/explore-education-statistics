import PageTitle from '@admin/components/PageTitle';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import { ReleaseRouteProps } from '@admin/routes/releaseRoutes';
import NavBar from '@admin/components/NavBar';
import { generatePath, Route, RouteComponentProps, Switch } from 'react-router';
import React from 'react';
import PrototypeReleaseDataPage from './PrototypeReleaseDataPage';
import PrototypeReleasePageTab from './PrototypeReleasePageTab';
import PrototypeSignOffPage from './PrototypeSignOffPage';
import PrototypePrepareNextSubjectPage from './PrototypePrepareNextSubjectPage';

const releaseSummaryRoute: ReleaseRouteProps = {
  path: '/prototypes/admin-api/summary/:id',
  title: 'Summary',
  component: PrototypeReleasePageTab,
};
const releaseDataRoute: ReleaseRouteProps = {
  path: '/prototypes/admin-api/data/:id',
  title: 'Data and files',
  component: PrototypeReleaseDataPage,
};
const releasePrepareSubjectRoute: ReleaseRouteProps = {
  path: '/prototypes/admin-api/data/:id/prepare-subject/:psid',
  title: 'Data and files',
  component: PrototypePrepareNextSubjectPage,
};
const releaseFootnotesRoute: ReleaseRouteProps = {
  path: '/prototypes/admin-api/footnotes/:id',
  title: 'Footnotes',
  component: PrototypeReleasePageTab,
};
const releaseDataBlocksRoute: ReleaseRouteProps = {
  path: '/prototypes/admin-api/data-blocks/:id',
  title: 'Data blocks',
  component: PrototypeReleasePageTab,
};
const releaseContentRoute: ReleaseRouteProps = {
  path: '/prototypes/admin-api/content/:id',
  title: 'Content',
  component: PrototypeReleasePageTab,
};
const releaseStatusRoute: ReleaseRouteProps = {
  path: '/prototypes/admin-api/status/:id',
  title: 'Sign-off',
  component: PrototypeSignOffPage,
};
const releasePreReleaseAccessRoute: ReleaseRouteProps = {
  path: '/prototypes/admin-api/pre-release/:id',
  title: 'Pre-release access',
  component: PrototypeReleasePageTab,
};
type RouteParams = {
  id: string;
};

const navRoutes = [
  releaseSummaryRoute,
  releaseDataRoute,
  releaseFootnotesRoute,
  releaseDataBlocksRoute,
  releaseContentRoute,
  releaseStatusRoute,

  releasePreReleaseAccessRoute,
];

const routes = [...navRoutes, releasePrepareSubjectRoute];

interface MatchProps {
  id: string;
}

const PrototypeReleasePage = ({ match }: RouteComponentProps<MatchProps>) => {
  const release =
    match.params.id === '2022-23'
      ? 'Academic Year 2022/23'
      : 'Academic Year 2021/22';

  return (
    <PrototypePage
      wide
      breadcrumbs={[
        { name: 'Publication', link: '#' },
        { name: 'Edit release' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageTitle
            caption={`Edit release for ${release}`}
            title="Characteristics of children in need"
          />
        </div>
        {/* <Tag>{getReleaseApprovalStatusLabel(release.approvalStatus)}</Tag>
      {release.amendment && (
        <Tag className="govuk-!-margin-left-2">Amendment</Tag>
      )}
      {release.live && <Tag className="govuk-!-margin-left-2">Live</Tag>} */}
      </div>

      <NavBar
        routes={navRoutes.map(route => ({
          title: route.title,
          to: generatePath<RouteParams>(route.path, {
            id: match.params.id,
          }),
        }))}
        label="Release"
      />

      <Switch>
        {routes.map(route => (
          <Route exact key={route.path} {...route} />
        ))}
      </Switch>
    </PrototypePage>
  );
};

export default PrototypeReleasePage;
