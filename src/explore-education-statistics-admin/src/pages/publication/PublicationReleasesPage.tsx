import ButtonLink from '@admin/components/ButtonLink';
import PublicationPublishedReleases from '@admin/pages/publication/components/PublicationPublishedReleases';
import PublicationUnpublishedReleases from '@admin/pages/publication/components/PublicationUnpublishedReleases';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import { PublicationRouteParams } from '@admin/routes/publicationRoutes';
import { releaseCreateRoute } from '@admin/routes/routes';
import releaseService from '@admin/services/releaseService';
import { useQueryClient } from '@tanstack/react-query';
import noop from 'lodash/noop';
import React, { useCallback, useMemo, useRef, useState } from 'react';
import { generatePath } from 'react-router';
import publicationQueries from '@admin/queries/publicationQueries';
import { ReleaseLabelFormValues } from './components/ReleaseLabelEditModal';

const PublicationReleasesPage = () => {
  const { publicationId, publication } = usePublicationContext();

  const publishedReleasesRefetchRef = useRef<() => void>(noop);
  const [unpublishedVisibleCount, setUnpublishedVisibleCount] = useState(0);
  const [publishedVisibleCount, setPublishedVisibleCount] = useState(0);
  const queryClient = useQueryClient();

  const itemsCount = useMemo(
    () => unpublishedVisibleCount + publishedVisibleCount,
    [unpublishedVisibleCount, publishedVisibleCount],
  );
  const onEditingPublishedRelease = async (
    releaseId: string,
    releaseDetailsFormValues: ReleaseLabelFormValues,
  ) => {
    await releaseService.updateRelease(releaseId, {
      label: releaseDetailsFormValues.label,
    });

    queryClient.invalidateQueries(
      publicationQueries.listUnpublishedReleaseVersions(publicationId).queryKey,
    );
    queryClient.invalidateQueries(
      publicationQueries.listPublishedReleaseVersions(publicationId).queryKey,
    );
  };
  const handleUnpublishedCountChange = useCallback((count: number) => {
    setUnpublishedVisibleCount(count);
  }, []);

  const handlePublishedCountChange = useCallback((count: number) => {
    setPublishedVisibleCount(count);
  }, []);

  const showBackToTopLink = itemsCount > 10;
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
        addItemsCount={handleUnpublishedCountChange}
        showBackToTopLink={showBackToTopLink}
      />
      <PublicationPublishedReleases
        publication={publication}
        refetchRef={publishedReleasesRefetchRef}
        onEdit={onEditingPublishedRelease}
        addItemsCount={handlePublishedCountChange}
        showBackToTopLink={showBackToTopLink}
      />
    </>
  );
};

export default PublicationReleasesPage;
