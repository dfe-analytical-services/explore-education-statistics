import ReleaseSeriesTable from '@admin/pages/publication/components/ReleaseSeriesTable';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import publicationQueries from '@admin/queries/publicationQueries';
import {
  PublicationRouteParams,
  publicationCreateReleaseSeriesLegacyLinkRoute,
} from '@admin/routes/publicationRoutes';
import publicationService from '@admin/services/publicationService';
import { mapToReleaseSeriesItemUpdateRequest } from '@admin/pages/publication/PublicationEditReleaseSeriesLegacyLinkPage';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ButtonGroup from '@common/components/ButtonGroup';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import { generatePath, useHistory } from 'react-router';
import React from 'react';
import { useQuery } from '@tanstack/react-query';

export default function PublicationReleaseSeriesPage() {
  const history = useHistory();
  const { publicationId, publication } = usePublicationContext();
  const [isReordering, toggleReordering] = useToggle(false);

  const {
    data: releaseSeries = [],
    isLoading,
    refetch,
  } = useQuery(publicationQueries.getReleaseSeries(publicationId));

  const { canManageReleaseSeries } = publication.permissions;

  return (
    <LoadingSpinner loading={isLoading}>
      <h2>Release order</h2>

      <p>
        Releases will be shown in the order below on the publication and can be
        reordered.
      </p>
      <p>
        Reorder releases order using drag and drop or by pressing the up and
        down arrows next to each release.
      </p>
      <p>
        Legacy releases which link to extenal pages outside the service can be
        created, edited, deleted, and are included in the release order.
      </p>
      <p>
        Only releases with a published version and legacy releases will appear
        in the publication.
      </p>

      {canManageReleaseSeries && !isReordering && (
        <ButtonGroup>
          <ModalConfirm
            confirmText="OK"
            title="Create legacy release"
            triggerButton={<Button>Create legacy release</Button>}
            onConfirm={() => {
              history.push(
                generatePath<PublicationRouteParams>(
                  publicationCreateReleaseSeriesLegacyLinkRoute.path,
                  {
                    publicationId,
                  },
                ),
              );
            }}
          >
            <WarningMessage>
              All changes made to legacy releases appear immediately on the
              public website.
            </WarningMessage>
          </ModalConfirm>

          {releaseSeries.length > 1 && (
            <ModalConfirm
              confirmText="OK"
              title="Reorder releases"
              triggerButton={
                <Button variant="secondary">Reorder releases</Button>
              }
              onConfirm={toggleReordering.on}
            >
              <WarningMessage>
                All changes made to releases appear immediately on the public
                website.
              </WarningMessage>
            </ModalConfirm>
          )}
        </ButtonGroup>
      )}

      {releaseSeries.length > 0 ? (
        <ReleaseSeriesTable
          canManageReleaseSeries={
            publication.permissions.canManageReleaseSeries
          }
          releaseSeries={releaseSeries}
          publicationId={publicationId}
          publicationSlug={publication.slug}
          isReordering={isReordering}
          onCancelReordering={toggleReordering.off}
          onConfirmReordering={async nextSeries => {
            await publicationService.updateReleaseSeries(
              publicationId,
              mapToReleaseSeriesItemUpdateRequest(nextSeries),
            );
            await refetch();
            toggleReordering.off();
          }}
          onDelete={async id => {
            const nextReleaseSeries = releaseSeries.filter(
              item => item.id !== id,
            );
            await publicationService.updateReleaseSeries(
              publicationId,
              mapToReleaseSeriesItemUpdateRequest(nextReleaseSeries),
            );
            await refetch();
          }}
        />
      ) : (
        <p>No releases for this publication.</p>
      )}
    </LoadingSpinner>
  );
}
