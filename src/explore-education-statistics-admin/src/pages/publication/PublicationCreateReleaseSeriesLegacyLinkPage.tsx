import Link from '@admin/components/Link';
import ReleaseSeriesLegacyLinkForm from '@admin/pages/legacy-releases/components/ReleaseSeriesLegacyLinkForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import { publicationReleaseSeriesRoute } from '@admin/routes/publicationRoutes';
import React from 'react';
import { generatePath, useHistory } from 'react-router';
import publicationService from "@admin/services/publicationService";

const PublicationCreateReleaseSeriesLegacyLinkPage = () => {
  const { publicationId } = usePublicationContext();
  const history = useHistory();

  const publicationEditPath = generatePath(
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
          <Link unvisited to={publicationEditPath}>
            Cancel
          </Link>
        }
        onSubmit={async values => {
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

export default PublicationCreateReleaseSeriesLegacyLinkPage;
