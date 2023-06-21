import ExternalMethodologyForm from '@admin/pages/methodology/external-methodology/components/ExternalMethodologyForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  PublicationRouteParams,
  publicationMethodologiesRoute,
} from '@admin/routes/publicationRoutes';
import publicationService, {
  ExternalMethodology,
} from '@admin/services/publicationService';
import React from 'react';
import { generatePath, useHistory } from 'react-router';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import LoadingSpinner from '@common/components/LoadingSpinner';

const PublicationExternalMethodologyPage = () => {
  const history = useHistory();
  const { publicationId, publication, onReload } = usePublicationContext();
  const { value: externalMethodology, isLoading } = useAsyncHandledRetry<
    ExternalMethodology | undefined
  >(async () => publicationService.getExternalMethodology(publicationId), [
    publicationId,
  ]);

  const returnRoute = generatePath<PublicationRouteParams>(
    publicationMethodologiesRoute.path,
    {
      publicationId,
    },
  );

  const handleExternalMethodologySubmit = async (
    values: ExternalMethodology,
  ) => {
    if (!publication) {
      return;
    }
    const updatedExternalMethodology: ExternalMethodology = {
      title: values.title,
      url: values.url,
    };

    await publicationService.updateExternalMethodology(
      publicationId,
      updatedExternalMethodology,
    );
    onReload();
    history.push(returnRoute);
  };

  return (
    <LoadingSpinner loading={isLoading}>
      <h2>
        {externalMethodology
          ? 'Edit external methodology link'
          : 'Link to an externally hosted methodology'}
      </h2>
      <ExternalMethodologyForm
        initialValues={externalMethodology}
        onCancel={() => history.push(returnRoute)}
        onSubmit={handleExternalMethodologySubmit}
      />
    </LoadingSpinner>
  );
};

export default PublicationExternalMethodologyPage;
