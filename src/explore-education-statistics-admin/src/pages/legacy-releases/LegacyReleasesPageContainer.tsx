import ProtectedRoute from '@admin/components/ProtectedRoute';
import { PublicationContextProvider } from '@admin/contexts/PublicationContext';
import {
  legacyReleaseCreateRoute,
  legacyReleaseEditRoute,
} from '@admin/routes/legacyReleaseRoutes';
import { PublicationRouteParams } from '@admin/routes/routes';
import React from 'react';
import { RouteComponentProps, Switch } from 'react-router';

const routes = [legacyReleaseCreateRoute, legacyReleaseEditRoute];

const LegacyReleasesPageContainer = ({
  match,
}: RouteComponentProps<PublicationRouteParams>) => {
  const { publicationId } = match.params;

  return (
    <PublicationContextProvider publicationId={publicationId}>
      <Switch>
        {routes.map(({ ...route }) => (
          <ProtectedRoute key={route.path} {...route} />
        ))}
      </Switch>
    </PublicationContextProvider>
  );
};

export default LegacyReleasesPageContainer;
