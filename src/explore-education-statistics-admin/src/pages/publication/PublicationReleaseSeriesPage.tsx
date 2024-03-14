import ReleaseSeriesTable from '@admin/pages/publication/components/ReleaseSeriesTable';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import publicationService  from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import {ReleaseSeriesItem} from "@common/services/publicationService";

const PublicationReleaseSeriesPage = () => {
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
      <ReleaseSeriesTable // @MarkFix rename to ReleaseSeriesTable?
        canManageReleaseSeries={
          publication.permissions.canManageReleaseSeries // @MarkFix rename to canManageReleaseSeries?
        }
        releaseSeries={releaseSeries}
        publicationId={publicationId}
      />
    </>
  );
};

export default PublicationReleaseSeriesPage;
