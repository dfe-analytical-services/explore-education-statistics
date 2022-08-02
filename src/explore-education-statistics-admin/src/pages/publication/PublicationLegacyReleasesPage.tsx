import { useLastLocation } from '@admin/contexts/LastLocationContext';
import LegacyReleasesTable from '@admin/pages/publication/components/LegacyReleasesTable';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import publicationService from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { useLocation } from 'react-router';

const PublicationLegacyReleasesPage = () => {
  const {
    publicationId,
    publication: contextPublication,
  } = usePublicationContext();
  const location = useLocation();
  const lastLocation = useLastLocation();

  const { value: publication } = useAsyncHandledRetry(
    async () =>
      lastLocation && lastLocation !== location
        ? publicationService.getMyPublication(publicationId)
        : contextPublication,
    [publicationId],
  );

  if (!publication) {
    return <LoadingSpinner />;
  }

  return (
    <>
      <h2>Legacy releases</h2>
      <LegacyReleasesTable publication={publication} />
    </>
  );
};

export default PublicationLegacyReleasesPage;
