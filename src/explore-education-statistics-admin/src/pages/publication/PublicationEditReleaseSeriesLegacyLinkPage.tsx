import Link from '@admin/components/Link';
import ReleaseSeriesLegacyLinkForm from '@admin/pages/legacy-releases/components/ReleaseSeriesLegacyLinkForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  PublicationEditReleaseSeriesLegacyLinkRouteParams,
  publicationReleaseSeriesRoute,
} from '@admin/routes/publicationRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps, useHistory } from 'react-router';
import publicationService, {
  ReleaseSeriesItemUpdateRequest, ReleaseSeriesTableEntry,
} from '@admin/services/publicationService';

export const mapToReleaseSeriesItemUpdateRequest = (
  releaseSeries: ReleaseSeriesTableEntry[],
): ReleaseSeriesItemUpdateRequest[] => {
  return releaseSeries.map(seriesItem => ({
    id: seriesItem.id,
    releaseId: !seriesItem.isLegacyLink ? seriesItem.releaseId : undefined,
    legacyLinkDescription: seriesItem.isLegacyLink
      ? seriesItem.description
      : undefined,
    legacyLinkUrl: seriesItem.isLegacyLink
      ? seriesItem.legacyLinkUrl
      : undefined,
  }));
};

const PublicationEditReleaseSeriesLegacyLinkPage = ({
  match,
}: RouteComponentProps<PublicationEditReleaseSeriesLegacyLinkRouteParams>) => {
  const { releaseSeriesItemId } = match.params;
  const { publicationId } = usePublicationContext();
  const history = useHistory();

  const { value: releaseSeries = [], isLoading } = useAsyncHandledRetry(() =>
    publicationService.getReleaseSeries(publicationId),
  );

  const itemIndex = releaseSeries?.findIndex(
    rsi => rsi.id === releaseSeriesItemId,
  );

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (itemIndex === -1) {
    return <p>Release series item not found.</p>;
  }

  if (releaseSeries[itemIndex].releaseId !== undefined) {
    return <p>Release series item isn't a legacy link.</p>;
  }

  const publicationEditPath = generatePath(publicationReleaseSeriesRoute.path, {
    publicationId,
  });

  return (
    <>
      <h2>Edit legacy release</h2>
      {releaseSeries && (
        <ReleaseSeriesLegacyLinkForm
          initialValues={{
            description: releaseSeries[itemIndex].description,
            url: releaseSeries[itemIndex].legacyLinkUrl ?? '',
          }}
          cancelButton={
            <Link unvisited to={publicationEditPath}>
              Cancel
            </Link>
          }
          onSubmit={async values => {
            releaseSeries[itemIndex].description = values.description;
            releaseSeries[itemIndex].legacyLinkUrl = values.url;
            await publicationService.updateReleaseSeries(
              publicationId,
              mapToReleaseSeriesItemUpdateRequest(releaseSeries),
            );

            history.push(publicationEditPath);
          }}
        />
      )}
    </>
  );
};

export default PublicationEditReleaseSeriesLegacyLinkPage;
