import MethodologySummaryForm from '@admin/pages/methodology/components/MethodologySummaryForm';
import {
  MethodologyRouteParams,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import methodologyService from '@admin/services/methodologyService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

const MethodologySummaryEditPage = ({
  history,
  match,
}: RouteComponentProps<MethodologyRouteParams>) => {
  const { methodologyId } = match.params;

  const { value: methodology, isLoading } = useAsyncHandledRetry(async () => {
    return methodologyService.getMethodology(methodologyId);
  });

  return (
    <>
      <h2>Edit methodology summary</h2>

      <LoadingSpinner loading={isLoading}>
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
            onSubmit={async values => {
              await methodologyService.updateMethodology(methodologyId, {
                latestInternalReleaseNote:
                  methodology.latestInternalReleaseNote,
                publishingStrategy: methodology.publishingStrategy,
                status: methodology.status,
                title: values.title,
                withReleaseId: methodology.scheduledWithRelease?.id,
              });

              history.push(
                generatePath<MethodologyRouteParams>(
                  methodologySummaryRoute.path,
                  {
                    methodologyId,
                  },
                ),
              );
            }}
            onCancel={history.goBack}
          />
        )}
      </LoadingSpinner>
    </>
  );
};

export default MethodologySummaryEditPage;
