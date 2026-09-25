import MethodologySummaryForm from '@admin/pages/methodology/components/MethodologySummaryForm';
import { methodologySummaryRoute } from '@admin/routes/methodologyRoutes';
import methodologyService from '@admin/services/methodologyService';
import { useMethodologyContext } from '@admin/pages/methodology/contexts/MethodologyContext';
import React from 'react';
import { generatePath, useNavigate } from 'react-router';

const MethodologySummaryEditPage = () => {
  const navigate = useNavigate();
  const { methodologyId, methodology, onMethodologyChange } =
    useMethodologyContext();

  const handleSubmit = async (title: string) => {
    if (!methodology) {
      throw new Error('Could not update missing methodology');
    }

    const nextMethodology = await methodologyService.updateMethodology(
      methodologyId,
      {
        latestInternalReleaseNote: methodology.internalReleaseNote,
        publishingStrategy: methodology.publishingStrategy,
        status: methodology.status,
        title,
        withReleaseId: methodology.scheduledWithRelease?.id,
      },
    );

    onMethodologyChange(nextMethodology);

    navigate(
      generatePath(methodologySummaryRoute.fullPath, {
        methodologyId,
      }),
    );
  };

  return (
    <>
      <h2>Edit methodology summary</h2>
      {methodology && (
        <MethodologySummaryForm
          id="updateMethodologyForm"
          initialValues={{
            title: methodology.title,
            titleType:
              methodology.title !== methodology.owningPublication.title
                ? 'alternative'
                : 'default',
          }}
          defaultTitle={methodology.owningPublication.title}
          submitText="Update methodology"
          onSubmit={handleSubmit}
          onCancel={() => navigate(-1)}
        />
      )}
    </>
  );
};

export default MethodologySummaryEditPage;
