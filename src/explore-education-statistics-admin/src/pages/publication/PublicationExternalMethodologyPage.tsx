import ExternalMethodologyForm from '@admin/pages/methodology/external-methodology/components/ExternalMethodologyForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import { publicationMethodologiesRoute } from '@admin/routes/publicationRoutes';
import publicationService, {
  ExternalMethodology,
} from '@admin/services/publicationService';
import React from 'react';
import { generatePath, useNavigate } from 'react-router';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import LoadingSpinner from '@common/components/LoadingSpinner';
import PageMetaTitle from '@admin/components/PageMetaTitle';

const PublicationExternalMethodologyPage = () => {
  const navigate = useNavigate();
  const { publicationId, publication, onReload } = usePublicationContext();
  const { value: externalMethodology, isLoading } = useAsyncHandledRetry<
    ExternalMethodology | undefined
  >(
    async () => publicationService.getExternalMethodology(publicationId),
    [publicationId],
  );

  const returnRoute = generatePath(publicationMethodologiesRoute.fullPath, {
    publicationId,
  });

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
    navigate(returnRoute);
  };

  if (isLoading) {
    return <LoadingSpinner />;
  }

  const title = externalMethodology
    ? 'Edit external methodology link'
    : 'Link to an externally hosted methodology';

  return (
    <>
      <PageMetaTitle title={`${title} - ${publication.title}`} />
      <h2>{title}</h2>
      <ExternalMethodologyForm
        initialValues={externalMethodology}
        onCancel={() => navigate(returnRoute)}
        onSubmit={handleExternalMethodologySubmit}
      />
    </>
  );
};

export default PublicationExternalMethodologyPage;
