import LegacyReleasesTable from '@admin/pages/publication/components/LegacyReleasesTable';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import legacyReleaseService, {
  ReleaseSeriesItem,
} from '@admin/services/legacyReleaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';

const PublicationLegacyReleasesPage = () => {
  const { publicationId, publication } = usePublicationContext();

  const { value: releaseSeries = [], isLoading } = useAsyncHandledRetry<
    ReleaseSeriesItem[]
  >(
    async () => legacyReleaseService.getReleaseSeriesView(publicationId),
    [publicationId],
  );

  if (isLoading) {
    return <LoadingSpinner />;
  }

  return (
    <>
      <h2>Legacy releases</h2>
      <LegacyReleasesTable // @MarkFix rename?
        canManageLegacyReleases={
          publication.permissions.canManageLegacyReleases // @MarkFix rename?
        }
        releaseSeries={releaseSeries}
        publicationId={publicationId}
      />
    </>
  );
};

export default PublicationLegacyReleasesPage;
