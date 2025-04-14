import ButtonLink from '@admin/components/ButtonLink';
import PublicationPublishedReleases from '@admin/pages/publication/components/PublicationPublishedReleases';
import PublicationUnpublishedReleases from '@admin/pages/publication/components/PublicationUnpublishedReleases';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import { PublicationRouteParams } from '@admin/routes/publicationRoutes';
import { releaseCreateRoute } from '@admin/routes/routes';
import releaseService from '@admin/services/releaseService';
import { useQueryClient } from '@tanstack/react-query';
import noop from 'lodash/noop';
import React, { useRef } from 'react';
import { generatePath } from 'react-router';
import publicationQueries from '@admin/queries/publicationQueries';
import { ReleaseLabelFormValues } from './components/ReleaseLabelEditModal';

const PublicationReleasesPage = () => {
  const { publicationId, publication } = usePublicationContext();

  const publishedReleasesRefetchRef = useRef<() => void>(noop);

  const queryClient = useQueryClient();

  const onEditingPublishedRelease = async (
    releaseId: string,
    releaseDetailsFormValues: ReleaseLabelFormValues,
  ) => {
    await releaseService.updateRelease(releaseId, {
      label: releaseDetailsFormValues.label,
    });

    queryClient.invalidateQueries(
      publicationQueries.listUnpublishedReleaseVersionsWithPermissions(
        publicationId,
      ).queryKey,
    );
    queryClient.invalidateQueries(
      publicationQueries.listPublishedReleaseVersionsWithPermissions(
        publicationId,
      ).queryKey,
    );
  };

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
        publication={publication}
        refetchRef={publishedReleasesRefetchRef}
        onEdit={onEditingPublishedRelease}
      />
    </>
  );
};

export default PublicationReleasesPage;
