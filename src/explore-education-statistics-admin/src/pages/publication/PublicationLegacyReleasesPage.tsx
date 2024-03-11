import LegacyReleasesTable from '@admin/pages/publication/components/LegacyReleasesTable';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import publicationService, { ReleaseSeriesItem } from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';

const PublicationLegacyReleasesPage = () => {
  const { publicationId, publication } = usePublicationContext();

  const { value: releaseSeries = [], isLoading } = useAsyncHandledRetry<
    ReleaseSeriesItem[]
  >(
    async () => publicationService.getReleaseSeriesView(publicationId),
    [publicationId],
  );

  if (isLoading) {
    return <LoadingSpinner />;
  }

  return (
    <>
      <h2>Legacy releases</h2>
      <LegacyReleasesTable // @MarkFix rename to ReleaseSeriesTable?
        canManageLegacyReleases={
          publication.permissions.canManageLegacyReleases // @MarkFix rename to canManageReleaseSeries?
        }
        releaseSeries={releaseSeries}
        publicationId={publicationId}
      />
    </>
  );
};

export default PublicationLegacyReleasesPage;
