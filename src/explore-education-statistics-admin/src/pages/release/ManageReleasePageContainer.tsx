import ProtectedRoute from '@admin/components/ProtectedRoute';
import ReleasePageTemplate from '@admin/pages/release/edit-release/components/ReleasePageTemplate';
import releaseRoutes from '@admin/routes/edit-release/routes';
import service from '@admin/services/common/service';
import { BasicPublicationDetails } from '@admin/services/common/types';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import PublicationContext from './PublicationContext';

interface MatchProps {
  publicationId: string;
  releaseId: string;
}

const ManageReleasePageContainer = ({
  match,
}: RouteComponentProps<MatchProps>) => {
  const { publicationId, releaseId } = match.params;

  const [publication, setPublication] = useState<BasicPublicationDetails>();

  useEffect(() => {
    service.getBasicPublicationDetails(publicationId).then(setPublication);
  }, [publicationId, releaseId]);

  return (
    <>
      {publication && (
        <ReleasePageTemplate
          publicationId={publicationId}
          releaseId={releaseId}
          publicationTitle={publication.title}
        >
          <PublicationContext.Provider value={{ publication }}>
            {releaseRoutes.manageReleaseRoutes.map(route => (
              <ProtectedRoute
                exact
                key={route.path}
                path={route.path}
                component={route.component}
              />
            ))}
          </PublicationContext.Provider>
        </ReleasePageTemplate>
      )}
    </>
  );
};

export default ManageReleasePageContainer;
