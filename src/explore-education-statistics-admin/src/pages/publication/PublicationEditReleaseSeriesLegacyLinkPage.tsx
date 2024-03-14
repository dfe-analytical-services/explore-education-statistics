import Link from '@admin/components/Link';
import ReleaseSeriesLegacyLinkForm from '@admin/pages/legacy-releases/components/ReleaseSeriesLegacyLinkForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  PublicationEditLegacyReleaseRouteParams,
  publicationReleaseSeriesRoute,
} from '@admin/routes/publicationRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps, useHistory } from 'react-router';
import publicationService from '@admin/services/publicationService';

const PublicationEditReleaseSeriesLegacyLinkPage = ({
  match,
}: RouteComponentProps<PublicationEditLegacyReleaseRouteParams>) => {
  const { legacyReleaseId } = match.params;
  const { publicationId } = usePublicationContext();
  const history = useHistory();

  const { value: releaseSeries, isLoading } = useAsyncHandledRetry(() =>
    publicationService.getReleaseSeriesView(publicationId),
  );

  const itemIndex = releaseSeries?.findIndex(rsi => rsi.id === legacyReleaseId);
  if (isLoading || releaseSeries === undefined || itemIndex === undefined || itemIndex === -1) { // @MarkFix
    return <LoadingSpinner />;
  }

  if (releaseSeries[itemIndex!].releaseId !== undefined) {
    // @MarkFix
    return <p>Cannot edit this release series item!</p>;
  }

  const publicationEditPath = generatePath(
    publicationReleaseSeriesRoute.path,
    {
      publicationId,
    },
  );

  return (
    <>
      <h2>Edit legacy release</h2>
      {releaseSeries && (
        <ReleaseSeriesLegacyLinkForm
          initialValues={{
            description: releaseSeries[itemIndex!].description,
            url: releaseSeries[itemIndex!].legacyLinkUrl!,
          }}
          cancelButton={
            <Link unvisited to={publicationEditPath}>
              Cancel
            </Link>
          }
          onSubmit={async values => {
            releaseSeries[itemIndex!].description = values.description;
            releaseSeries[itemIndex!].legacyLinkUrl = values.url;
            await publicationService.updateReleaseSeriesView(
              publicationId,
              releaseSeries.map(seriesItem => ({
                // @MarkFix abstract out mapping (as similar happens in ReleaseSeriesTable)
                id: seriesItem.id,
                releaseId: !seriesItem.isLegacyLink
                  ? seriesItem.releaseId
                  : undefined,
                legacyLinkDescription: seriesItem.isLegacyLink
                  ? seriesItem.description
                  : undefined,
                legacyLinkUrl: seriesItem.isLegacyLink
                  ? seriesItem.legacyLinkUrl
                  : undefined,
              })),
            );

            history.push(publicationEditPath);
          }}
        />
      )}
    </>
  );
};

export default PublicationEditReleaseSeriesLegacyLinkPage;
