import Link from '@admin/components/Link';
import LegacyReleaseForm from '@admin/pages/legacy-releases/components/LegacyReleaseForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import { publicationLegacyReleasesRoute } from '@admin/routes/publicationRoutes';
import React from 'react';
import { generatePath, useHistory } from 'react-router';
import publicationService from "@admin/services/publicationService";

const PublicationLegacyReleaseCreatePage = () => {
  const { publicationId, publication } = usePublicationContext();
  const history = useHistory();

  const publicationEditPath = generatePath(
    publicationLegacyReleasesRoute.path,
    {
      publicationId,
    },
  );

  return (
    <>
      <h2>Create legacy release</h2>
      <LegacyReleaseForm
        cancelButton={
          <Link unvisited to={publicationEditPath}>
            Cancel
          </Link>
        }
        onSubmit={async values => {
          // @MarkFix maybe we want to fetch the ReleaseSeries from frontend context and just use UpdateReleaseSeries instead?
          await publicationService.addReleaseSeriesLegacyLink(publicationId, {
            description: values.description,
            url: values.url,
          });

          history.push(publicationEditPath);
        }}
      />
    </>
  );
};

export default PublicationLegacyReleaseCreatePage;
