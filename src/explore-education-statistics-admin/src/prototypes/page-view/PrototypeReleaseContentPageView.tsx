import PageTitle from '@admin/components/PageTitle';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import { ReleaseRouteProps } from '@admin/routes/releaseRoutes';
import NavBar from '@admin/components/NavBar';
import ReleaseContentPageViewTab from '@admin/prototypes/page-view/PrototypeReleaseContentPageViewTab';
import PrototypeReleasePageTab from '@admin/prototypes/admin-api/PrototypeReleasePageTab';
import { generatePath, Route, RouteComponentProps, Switch } from 'react-router';
import React from 'react';

const releaseSummaryRoute: ReleaseRouteProps = {
  path: '/prototypes/example#1',
  title: 'Summary',
  component: PrototypeReleasePageTab,
};
const releaseDataRoute: ReleaseRouteProps = {
  path: '/prototypes/example#2',
  title: 'Data and files',
  component: PrototypeReleasePageTab,
};
const releaseFootnotesRoute: ReleaseRouteProps = {
  path: '/prototypes/example#3',
  title: 'Footnotes',
  component: PrototypeReleasePageTab,
};
const releaseDataBlocksRoute: ReleaseRouteProps = {
  path: '/prototypes/example#4',
  title: 'Data blocks',
  component: PrototypeReleasePageTab,
};
const releaseContentRoute: ReleaseRouteProps = {
  path: '',
  title: 'Content',
  component: ReleaseContentPageViewTab,
};
const releaseStatusRoute: ReleaseRouteProps = {
  path: '/prototypes/example#5',
  title: 'Sign-off',
  component: PrototypeReleasePageTab,
};
const releasePreReleaseAccessRoute: ReleaseRouteProps = {
  path: '/prototypes/example#6',
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

const routes = [...navRoutes];

interface MatchProps {
  id: string;
}

const PrototypeReleaseContentPageView = ({
  match,
}: RouteComponentProps<MatchProps>) => {
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
            caption="Edit release for 2023/24"
            title="Initial Teacher Training Census"
          />
        </div>
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

export default PrototypeReleaseContentPageView;
