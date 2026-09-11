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
import { generatePath, useNavigate } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';

export const mapToReleaseSeriesItemUpdateRequest = (
  releaseSeries: ReleaseSeriesTableEntry[],
): ReleaseSeriesItemUpdateRequest[] => {
  return releaseSeries.map(seriesItem => ({
    releaseId: seriesItem.releaseId,
    legacyLinkDescription: seriesItem.isLegacyLink
      ? seriesItem.description
      : undefined,
    legacyLinkUrl: seriesItem.legacyLinkUrl,
  }));
};

export default function PublicationEditReleaseSeriesLegacyLinkPage() {
  const { releaseSeriesItemId } =
    useParams<PublicationEditReleaseSeriesLegacyLinkRouteParams>() as PublicationEditReleaseSeriesLegacyLinkRouteParams;
  const { publicationId } = usePublicationContext();

  const navigate = useNavigate();

  const { data: releaseSeries = [], isLoading } = useQuery(
    publicationQueries.getReleaseSeries(publicationId),
  );

  const legacyRelease = releaseSeries.find(
    release => release.id === releaseSeriesItemId,
  );

  const publicationReleaseSeriesPath = generatePath(
    publicationReleaseSeriesRoute.fullPath,
    {
      publicationId,
    },
  );

  return (
    <LoadingSpinner loading={isLoading}>
      <h2>Edit legacy release</h2>

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

            navigate(publicationReleaseSeriesPath);
          }}
        />
      )}
    </LoadingSpinner>
  );
}
