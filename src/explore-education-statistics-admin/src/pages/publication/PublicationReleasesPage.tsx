import ButtonLink from '@admin/components/ButtonLink';
import PublicationDraftReleases from '@admin/pages/publication/components/PublicationDraftReleases';
import PublicationPublishedReleases from '@admin/pages/publication/components/PublicationPublishedReleases';
import PublicationScheduledReleases from '@admin/pages/publication/components/PublicationScheduledReleases';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import { PublicationRouteParams } from '@admin/routes/publicationRoutes';
import { releaseCreateRoute } from '@admin/routes/routes';
import publicationService from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React, { useMemo } from 'react';
import { generatePath } from 'react-router';

const PublicationReleasesPage = () => {
  const { publicationId } = usePublicationContext();

  // To ensure the releases are up to date when you switch to this tab
  // we're re-fetching the publication here instead of using it from the context.
  // This is not ideal and will be replaced by a call to just get the publication
  // releases when an endpoint for this is ready - EES-3554 .
  const {
    value: publication,
    isLoading: loadingPublication,
    retry: reloadPublication,
  } = useAsyncHandledRetry(() =>
    publicationService.getMyPublication(publicationId),
  );

  const draftReleases = useMemo(() => {
    return (
      publication?.releases.filter(
        release =>
          release.approvalStatus === 'Draft' ||
          release.approvalStatus === 'HigherLevelReview',
      ) ?? []
    );
  }, [publication?.releases]);

  const publishedReleases = useMemo(() => {
    return publication?.releases.filter(release => release.live) ?? [];
  }, [publication?.releases]);

  const scheduledReleases = useMemo(() => {
    return (
      publication?.releases.filter(
        release => !release.live && release.approvalStatus === 'Approved',
      ) ?? []
    );
  }, [publication?.releases]);

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
      <LoadingSpinner loading={loadingPublication}>
        {publication ? (
          <>
            {publication.releases.length > 0 ? (
              <>
                {scheduledReleases.length > 0 && (
                  <PublicationScheduledReleases releases={scheduledReleases} />
                )}

                {draftReleases.length > 0 && (
                  <PublicationDraftReleases
                    releases={draftReleases}
                    onChange={reloadPublication}
                  />
                )}

                {publishedReleases.length > 0 && (
                  <PublicationPublishedReleases
                    publicationId={publicationId}
                    releases={publishedReleases}
                  />
                )}
              </>
            ) : (
              <p>There are no releases for this publication yet.</p>
            )}
          </>
        ) : (
          <WarningMessage>
            There was a problem loading this publication.
          </WarningMessage>
        )}
      </LoadingSpinner>
    </>
  );
};

export default PublicationReleasesPage;
