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
import useToggle from '@common/hooks/useToggle';
import { Dictionary } from '@common/types';
import MethodologyStatusForm from '@admin/pages/methodology/edit-methodology/status/components/MethodolodyStatusForm';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';

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
    const [
      summary,
      unpublishedReleases,
      canApprove,
      canMarkAsDraft,
    ] = await Promise.all([
      methodologyService.getMethodology(methodologyId),
      methodologyService.getUnpublishedReleases(methodologyId),
      permissionService.canApproveMethodology(methodologyId),
      permissionService.canMarkMethodologyAsDraft(methodologyId),
    ]);

    return {
      summary,
      unpublishedReleases,
      canApprove,
      canMarkAsDraft,
    };
  }, [methodologyId]);

  const handleSubmit = async (values: FormValues) => {
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
  };

  const isEditable = model?.canApprove || model?.canMarkAsDraft;
  const isPublished = model?.summary.published;

  return (
    <>
      <LoadingSpinner loading={isLoading}>
        {model ? (
          <>
            {!showForm ? (
              <>
                <h2>Sign off</h2>

                <SummaryList>
                  <SummaryListItem term="Status">
                    <StatusBlock text={statusMap[model.summary.status]} />
                  </SummaryListItem>
                  {model.summary.status === 'Approved' && (
                    <>
                      <SummaryListItem term="When to publish">
                        {model.summary.publishingStrategy === 'WithRelease'
                          ? 'With a specific release'
                          : model.summary.publishingStrategy}
                      </SummaryListItem>
                      {model.summary.publishingStrategy === 'WithRelease' && (
                        <SummaryListItem term="Publish with release">
                          {model.summary.scheduledWithRelease?.title}
                        </SummaryListItem>
                      )}
                    </>
                  )}
                </SummaryList>

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
                unpublishedReleases={model.unpublishedReleases}
                onCancel={toggleForm.off}
                onSubmit={handleSubmit}
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
