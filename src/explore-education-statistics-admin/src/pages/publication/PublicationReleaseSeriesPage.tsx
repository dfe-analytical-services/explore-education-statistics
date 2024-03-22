import ReleaseSeriesTable from '@admin/pages/publication/components/ReleaseSeriesTable';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import publicationService, {
  ReleaseSeriesTableEntry,
} from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';

const PublicationReleaseSeriesPage = () => {
  const { publicationId, publication } = usePublicationContext();

  const { value: releaseSeries = [], isLoading } = useAsyncHandledRetry<
    ReleaseSeriesTableEntry[]
  >(
    async () => publicationService.getReleaseSeries(publicationId),
    [publicationId],
  );

  if (isLoading) {
    return <LoadingSpinner />;
  }

  return (
    <>
      <h2>Legacy releases</h2>
      <ReleaseSeriesTable
        canManageReleaseSeries={publication.permissions.canManageReleaseSeries}
        releaseSeries={releaseSeries}
        publicationId={publicationId}
        publicationSlug={publication.slug}
      />
    </>
  );
};

export default PublicationReleaseSeriesPage;
