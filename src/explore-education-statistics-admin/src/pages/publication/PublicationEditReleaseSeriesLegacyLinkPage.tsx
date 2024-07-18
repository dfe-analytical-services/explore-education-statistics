import Link from '@admin/components/Link';
import ReleaseSeriesLegacyLinkForm from '@admin/pages/legacy-releases/components/ReleaseSeriesLegacyLinkForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  PublicationEditReleaseSeriesLegacyLinkRouteParams,
  publicationReleaseSeriesRoute,
} from '@admin/routes/publicationRoutes';
import publicationQueries from '@admin/queries/publicationQueries';
import publicationService, {
  ReleaseSeriesItemUpdateRequest,
  ReleaseSeriesTableEntry,
} from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React from 'react';
import { generatePath, RouteComponentProps, useHistory } from 'react-router';
import { useQuery } from '@tanstack/react-query';

export const mapToReleaseSeriesItemUpdateRequest = (
  releaseSeries: ReleaseSeriesTableEntry[],
): ReleaseSeriesItemUpdateRequest[] => {
  return releaseSeries.map(seriesItem => ({
    id: seriesItem.id,
    releaseVersionId: seriesItem.releaseId,
    legacyLinkDescription: seriesItem.isLegacyLink
      ? seriesItem.description
      : undefined,
    legacyLinkUrl: seriesItem.legacyLinkUrl,
  }));
};

export default function PublicationEditReleaseSeriesLegacyLinkPage({
  match,
}: RouteComponentProps<PublicationEditReleaseSeriesLegacyLinkRouteParams>) {
  const { releaseSeriesItemId } = match.params;
  const { publicationId } = usePublicationContext();
  const history = useHistory();

  const { data: releaseSeries = [], isLoading } = useQuery(
    publicationQueries.getReleaseSeries(publicationId),
  );

  const legacyRelease = releaseSeries.find(
    release => release.id === releaseSeriesItemId,
  );

  const publicationReleaseSeriesPath = generatePath(
    publicationReleaseSeriesRoute.path,
    {
      publicationId,
    },
  );

  return (
    <LoadingSpinner loading={isLoading}>
      <h2>Edit legacy release</h2>

      {/* TODO rename to releaseVersionId */}
      {!legacyRelease || legacyRelease.releaseId !== undefined ? (
        <>
          <p>Legacy release not found.</p>
          <Link to={publicationReleaseSeriesPath}>Go back</Link>
        </>
      ) : (
        <ReleaseSeriesLegacyLinkForm
          initialValues={{
            description: legacyRelease.description,
            url: legacyRelease.legacyLinkUrl ?? '',
          }}
          cancelButton={
            <Link unvisited to={publicationReleaseSeriesPath}>
              Cancel
            </Link>
          }
          onSubmit={async values => {
            const updatedReleaseSeries = releaseSeries.map(release => {
              return release.id === releaseSeriesItemId
                ? {
                    ...release,
                    description: values.description,
                    legacyLinkUrl: values.url,
                  }
                : release;
            });

            await publicationService.updateReleaseSeries(
              publicationId,
              mapToReleaseSeriesItemUpdateRequest(updatedReleaseSeries),
            );

            history.push(publicationReleaseSeriesPath);
          }}
        />
      )}
    </LoadingSpinner>
  );
}
