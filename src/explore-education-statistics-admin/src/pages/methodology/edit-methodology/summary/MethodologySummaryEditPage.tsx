import MethodologySummaryForm, {
  MethodologySummaryFormValues,
} from '@admin/pages/methodology/components/MethodologySummaryForm';
import {
  MethodologyRouteParams,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import methodologyService from '@admin/services/methodologyService';
import { useMethodologyContext } from '@admin/pages/methodology/contexts/MethodologyContext';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

const MethodologySummaryEditPage = ({
  history,
}: RouteComponentProps<MethodologyRouteParams>) => {
  const {
    methodologyId,
    methodology,
    onMethodologyChange,
  } = useMethodologyContext();

  const handleSubmit = async ({ title }: MethodologySummaryFormValues) => {
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

    history.push(
      generatePath<MethodologyRouteParams>(methodologySummaryRoute.path, {
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
          onCancel={history.goBack}
        />
      )}
    </>
  );
};

export default MethodologySummaryEditPage;
