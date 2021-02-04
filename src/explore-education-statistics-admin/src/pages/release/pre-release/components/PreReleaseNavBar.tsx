import NavBar from '@admin/components/NavBar';
import { preReleaseNavRoutes } from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import React from 'react';
import { generatePath } from 'react-router';
import { useParams } from 'react-router-dom';

const PreReleaseNavBar = () => {
  const { publicationId, releaseId } = useParams<ReleaseRouteParams>();

  return (
    <NavBar
      className="govuk-!-margin-top-0"
      routes={preReleaseNavRoutes.map(route => ({
        title: route.title,
        to: generatePath<ReleaseRouteParams>(route.path, {
          publicationId,
          releaseId,
        }),
      }))}
    />
  );
};

export default PreReleaseNavBar;
