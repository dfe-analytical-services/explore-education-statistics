import Link from '@admin/components/Link';
import ReleaseSeriesLegacyLinkForm from '@admin/pages/legacy-releases/components/ReleaseSeriesLegacyLinkForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import { publicationReleaseSeriesRoute } from '@admin/routes/publicationRoutes';
import publicationService from '@admin/services/publicationService';
import React from 'react';
import { generatePath, useHistory } from 'react-router';

export default function PublicationCreateReleaseSeriesLegacyLinkPage() {
  const { publicationId } = usePublicationContext();
  const history = useHistory();

  const publicationReleaseSeriesPath = generatePath(
    publicationReleaseSeriesRoute.path,
    {
      publicationId,
    },
  );

  return (
    <>
      <h2>Create legacy release</h2>
      <ReleaseSeriesLegacyLinkForm
        cancelButton={
          <Link unvisited to={publicationReleaseSeriesPath}>
            Cancel
          </Link>
        }
        onSubmit={async values => {
          await publicationService.addReleaseSeriesLegacyLink(publicationId, {
            description: values.description,
            url: values.url,
          });

          history.push(publicationReleaseSeriesPath);
        }}
      />
    </>
  );
}
