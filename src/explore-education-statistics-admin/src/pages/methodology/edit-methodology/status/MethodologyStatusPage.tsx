import StatusBlock from '@admin/components/StatusBlock';
import { MethodologyRouteParams } from '@admin/routes/methodologyRoutes';
import methodologyService, {
  MethodologyStatus,
} from '@admin/services/methodologyService';
import permissionService from '@admin/services/permissionService';
import Button from '@common/components/Button';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import useFormSubmit from '@common/hooks/useFormSubmit';
import useToggle from '@common/hooks/useToggle';
import { Dictionary } from '@common/types';
import MethodologyStatusForm from '@admin/pages/methodology/edit-methodology/status/components/MethodolodyStatusForm';
import React from 'react';
import { RouteComponentProps } from 'react-router';

interface FormValues {
  status: MethodologyStatus;
  latestInternalReleaseNote: string;
  publishingStrategy?: 'WithRelease' | 'Immediately';
  withReleaseId?: string;
}

const statusMap: Dictionary<string> = {
  Draft: 'In Draft',
  Approved: 'Approved',
};

const MethodologyStatusPage = ({
  match,
}: RouteComponentProps<MethodologyRouteParams>) => {
  const { methodologyId } = match.params;

  const [showForm, toggleForm] = useToggle(false);

  const {
    value: model,
    setState: setModel,
    isLoading,
  } = useAsyncRetry(async () => {
    const [summary, canApprove, canMarkAsDraft] = await Promise.all([
      methodologyService.getMethodology(methodologyId),
      permissionService.canApproveMethodology(methodologyId),
      permissionService.canMarkMethodologyAsDraft(methodologyId),
    ]);

    return {
      summary,
      canApprove,
      canMarkAsDraft,
    };
  }, [methodologyId]);

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    if (!model) {
      return;
    }

    const nextSummary = await methodologyService.updateMethodology(
      methodologyId,
      {
        ...model.summary,
        ...values,
      },
    );

    setModel({
      isLoading: false,
      value: {
        ...model,
        summary: {
          ...nextSummary,
        },
      },
    });

    toggleForm.off();
  });

  const isEditable = model?.canApprove || model?.canMarkAsDraft;
  const isPublished = model?.summary.published;

  return (
    <>
      <LoadingSpinner loading={isLoading}>
        {model ? (
          <>
            {!showForm ? (
              <>
                <h2>Methodology status</h2>

                <div className="govuk-!-margin-bottom-6">
                  The current methodology status is:{' '}
                  <StatusBlock text={statusMap[model.summary.status]} />
                </div>

                {isEditable && (
                  <Button
                    className="govuk-!-margin-top-2"
                    onClick={toggleForm.on}
                  >
                    Edit status
                  </Button>
                )}
              </>
            ) : (
              <MethodologyStatusForm
                isPublished={isPublished}
                methodologySummary={model.summary}
                onCancel={toggleForm.off}
                onSubmit={(values, actions) => handleSubmit(values, actions)}
              />
            )}
          </>
        ) : (
          <WarningMessage>Could not load methodology status</WarningMessage>
        )}
      </LoadingSpinner>
    </>
  );
};

export default MethodologyStatusPage;
