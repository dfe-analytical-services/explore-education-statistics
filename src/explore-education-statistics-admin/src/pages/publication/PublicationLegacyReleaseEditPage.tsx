import Link from '@admin/components/Link';
import LegacyReleaseForm from '@admin/pages/legacy-releases/components/LegacyReleaseForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  PublicationEditLegacyReleaseRouteParams,
  publicationLegacyReleasesRoute,
} from '@admin/routes/publicationRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps, useHistory } from 'react-router';
import publicationService from '@admin/services/publicationService';

const PublicationLegacyReleaseEditPage = ({
  match,
}: RouteComponentProps<PublicationEditLegacyReleaseRouteParams>) => {
  const { legacyReleaseId } = match.params;
  const { publicationId } = usePublicationContext();
  const history = useHistory();

  const { value: releaseSeries, isLoading } = useAsyncHandledRetry(() =>
    publicationService.getReleaseSeriesView(publicationId),
  );

  let itemIndex = releaseSeries?.findIndex(rsi => rsi.id === legacyReleaseId);
  if (isLoading || releaseSeries === undefined || itemIndex === undefined || itemIndex === -1) { // @MarkFix
    return <LoadingSpinner />;
  }

  if (releaseSeries[itemIndex!].releaseParentId !== undefined) {
    // @MarkFix
    return <p>Cannot edit this release series item!</p>;
  }

  const publicationEditPath = generatePath(
    publicationLegacyReleasesRoute.path,
    {
      publicationId,
    },
  );

  return (
    <>
      <h2>Edit legacy release</h2>
      {releaseSeries && (
        <LegacyReleaseForm
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
                // @MarkFix abstract out mapping (as similar happens in LegacyReleasesTable)
                id: seriesItem.id,
                releaseParentId: !seriesItem.isLegacyLink
                  ? seriesItem.releaseParentId
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

export default PublicationLegacyReleaseEditPage;
