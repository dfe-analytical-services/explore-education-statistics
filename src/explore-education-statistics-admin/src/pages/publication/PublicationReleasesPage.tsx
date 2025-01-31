import ButtonLink from '@admin/components/ButtonLink';
import PublicationPublishedReleases from '@admin/pages/publication/components/PublicationPublishedReleases';
import PublicationUnpublishedReleases from '@admin/pages/publication/components/PublicationUnpublishedReleases';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import { PublicationRouteParams } from '@admin/routes/publicationRoutes';
import { releaseCreateRoute } from '@admin/routes/routes';
import noop from 'lodash/noop';
import React, { useRef } from 'react';
import { generatePath } from 'react-router';

const PublicationReleasesPage = () => {
  const { publicationId, publication } = usePublicationContext();

  const publishedReleasesRefetchRef = useRef<() => void>(noop);

  return (
    <>
      <h2>Manage releases</h2>

      <p>View, edit or amend releases contained within this publication.</p>

      {publication?.permissions.canCreateReleases && (
        <ButtonLink
          to={generatePath<PublicationRouteParams>(releaseCreateRoute.path, {
            publicationId,
          })}
        >
          Create new release
        </ButtonLink>
      )}

      <PublicationUnpublishedReleases
        publicationId={publicationId}
        onAmendmentDelete={() => {
          publishedReleasesRefetchRef.current();
        }}
      />

      <PublicationPublishedReleases
        publicationId={publicationId}
        refetchRef={publishedReleasesRefetchRef}
      />
    </>
  );
};

export default PublicationReleasesPage;
