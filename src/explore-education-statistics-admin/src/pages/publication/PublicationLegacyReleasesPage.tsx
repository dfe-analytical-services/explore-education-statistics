import LegacyReleasesTable from '@admin/pages/publication/components/LegacyReleasesTable';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import legacyReleaseService, {
  CombinedRelease,
} from '@admin/services/legacyReleaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';

const PublicationLegacyReleasesPage = () => {
  const { publicationId, publication } = usePublicationContext();

  const { value: combinedReleases = [], isLoading } = useAsyncHandledRetry<
    CombinedRelease[]
  >(
    async () => legacyReleaseService.listCombinedReleases(publicationId),
    [publicationId],
  );

  if (isLoading) {
    return <LoadingSpinner />;
  }

  return (
    <>
      <h2>Legacy releases</h2>
      <LegacyReleasesTable
        canManageLegacyReleases={
          publication.permissions.canManageLegacyReleases
        }
        combinedReleases={combinedReleases}
        publicationId={publicationId}
      />
    </>
  );
};

export default PublicationLegacyReleasesPage;
