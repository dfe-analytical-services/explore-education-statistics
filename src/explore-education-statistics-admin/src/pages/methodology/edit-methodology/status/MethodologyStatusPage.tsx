import StatusBlock from '@admin/components/StatusBlock';
import { useConfig } from '@admin/contexts/ConfigContext';
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
import { useMethodologyContext } from '@admin/pages/methodology/contexts/MethodologyContext';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import MethodologyStatusEditPage from '@admin/pages/methodology/edit-methodology/status/MethodologyStatusEditPage';
import UrlContainer from '@common/components/UrlContainer';
import React from 'react';

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

const MethodologyStatusPage = () => {
  const {
    methodologyId,
    methodology: currentMethodology,
    onMethodologyChange,
  } = useMethodologyContext();

  const { PublicAppUrl } = useConfig();

  const [isEditing, toggleForm] = useToggle(false);

  const {
    value: permissions,
    retry: refreshPermissions,
    isLoading,
  } = useAsyncRetry(async () => {
    const [canApprove, canMarkAsDraft] = await Promise.all([
      permissionService.canApproveMethodology(methodologyId),
      permissionService.canMarkMethodologyAsDraft(methodologyId),
    ]);

    return {
      canApprove,
      canMarkAsDraft,
    };
  }, [methodologyId]);

  const handleSubmit = async ({
    latestInternalReleaseNote,
    publishingStrategy,
    status,
    withReleaseId,
  }: FormValues) => {
    if (!currentMethodology) {
      return;
    }

    const nextSummary = await methodologyService.updateMethodology(
      methodologyId,
      {
        latestInternalReleaseNote,
        publishingStrategy,
        status,
        title: currentMethodology.title,
        withReleaseId:
          publishingStrategy === 'WithRelease' ? withReleaseId : undefined,
      },
    );

    onMethodologyChange(nextSummary);

    refreshPermissions();

    toggleForm.off();
  };

  const isEditable = permissions?.canApprove || permissions?.canMarkAsDraft;

  return (
    <LoadingSpinner loading={isLoading}>
      {currentMethodology ? (
        // eslint-disable-next-line react/jsx-no-useless-fragment
        <>
          {!isEditing ? (
            <>
              <h2>Sign off</h2>

              <p>
                The <strong>public methodology</strong> will be accessible at:
              </p>

              <p>
                <UrlContainer
                  data-testid="public-methodology-url"
                  url={`${PublicAppUrl}/methodology/${currentMethodology.slug}`}
                />
              </p>

              <SummaryList>
                <SummaryListItem term="Status">
                  <StatusBlock text={statusMap[currentMethodology.status]} />
                </SummaryListItem>
                {currentMethodology.status === 'Approved' && (
                  <>
                    <SummaryListItem term="Internal note">
                      {currentMethodology.internalReleaseNote}
                    </SummaryListItem>
                    <SummaryListItem term="When to publish">
                      {currentMethodology.publishingStrategy === 'WithRelease'
                        ? 'With a specific release'
                        : currentMethodology.publishingStrategy}
                    </SummaryListItem>
                    {currentMethodology.publishingStrategy ===
                      'WithRelease' && (
                      <SummaryListItem term="Publish with release">
                        {currentMethodology.scheduledWithRelease?.title}
                      </SummaryListItem>
                    )}
                  </>
                )}
                <SummaryListItem term="Owning publication">
                  {currentMethodology.owningPublication.title}
                </SummaryListItem>
                {currentMethodology.otherPublications &&
                  currentMethodology.otherPublications.length > 0 && (
                    <SummaryListItem term="Other publications">
                      <ul className="govuk-!-margin-top-0">
                        {currentMethodology.otherPublications?.map(
                          publication => (
                            <li
                              key={publication.id}
                              data-testid="other-publication-item"
                            >
                              {publication.title}
                            </li>
                          ),
                        )}
                      </ul>
                    </SummaryListItem>
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
            <MethodologyStatusEditPage
              methodology={currentMethodology}
              onCancel={toggleForm.off}
              onSubmit={handleSubmit}
            />
          )}
        </>
      ) : (
        <WarningMessage>Could not load methodology status</WarningMessage>
      )}
    </LoadingSpinner>
  );
};

export default MethodologyStatusPage;
