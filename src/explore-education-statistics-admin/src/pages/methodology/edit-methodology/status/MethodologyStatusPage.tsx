import StatusBlock from '@admin/components/StatusBlock';
import { useConfig } from '@admin/contexts/ConfigContext';
import methodologyService from '@admin/services/methodologyService';
import Button from '@common/components/Button';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import { Dictionary } from '@common/types';
import { useMethodologyContext } from '@admin/pages/methodology/contexts/MethodologyContext';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import MethodologyStatusEditPage from '@admin/pages/methodology/edit-methodology/status/MethodologyStatusEditPage';
import { MethodologyStatusFormValues } from '@admin/pages/methodology/edit-methodology/status/components/MethodologyStatusForm';
import React from 'react';
import UrlContainer from '@common/components/UrlContainer';
import FormattedDate from '@common/components/FormattedDate';
import { useQuery } from '@tanstack/react-query';
import methodologyQueries from '@admin/queries/methodologyQueries';
import permissionQueries from '@admin/queries/permissionQueries';

const statusMap: Dictionary<string> = {
  Draft: 'In Draft',
  HigherLevelReview: 'Awaiting higher review',
  Approved: 'Approved',
};

const MethodologyStatusPage = () => {
  const {
    methodologyId,
    methodology: currentMethodology,
    onMethodologyChange,
  } = useMethodologyContext();

  const { publicAppUrl } = useConfig();

  const [isEditing, toggleForm] = useToggle(false);

  const { data: methodologyStatuses, refetch: refreshMethodologyStatuses } =
    useQuery(methodologyQueries.getMethodologyStatuses(currentMethodology.id));

  const {
    data: permissions,
    refetch: refreshPermissions,
    isLoading,
  } = useQuery(
    permissionQueries.getMethodologyApprovalPermissions(currentMethodology.id),
  );

  const handleSubmit = async ({
    latestInternalReleaseNote,
    publishingStrategy,
    status,
    withReleaseId,
  }: MethodologyStatusFormValues) => {
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

    await refreshPermissions();
    await refreshMethodologyStatuses();

    toggleForm.off();
  };

  const isEditable =
    permissions?.canMarkApproved ||
    permissions?.canMarkHigherLevelReview ||
    permissions?.canMarkDraft;

  return (
    <LoadingSpinner loading={isLoading}>
      {currentMethodology ? (
        <>
          {!isEditing ? (
            <>
              <h2>Sign off</h2>

              <p>
                The <strong>public methodology</strong> will be accessible at:
              </p>

              <UrlContainer
                className="govuk-!-margin-bottom-4"
                id="public-methodology-url"
                url={`${publicAppUrl}/methodology/${currentMethodology.slug}`}
              />

              <SummaryList>
                <SummaryListItem term="Status">
                  <StatusBlock text={statusMap[currentMethodology.status]} />
                </SummaryListItem>
                {currentMethodology.status === 'Approved' && (
                  <>
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

              {methodologyStatuses && methodologyStatuses.length > 0 && (
                <>
                  <h3>Methodology status history</h3>
                  <LoadingSpinner
                    loading={!methodologyStatuses}
                    text="Loading methodology status history"
                  >
                    <table data-testid="methodology-status-history">
                      <thead>
                        <tr>
                          <th scope="col">Date</th>
                          <th scope="col">Status</th>
                          <th scope="col">Internal note</th>
                          <th scope="col">Methodology version</th>
                          <th scope="col">By user</th>
                        </tr>
                      </thead>
                      <tbody>
                        {methodologyStatuses.map(status => (
                          <tr key={status.methodologyStatusId}>
                            <td>
                              {status.created ? (
                                <FormattedDate format="d MMMM yyyy HH:mm">
                                  {status.created}
                                </FormattedDate>
                              ) : (
                                'Not available'
                              )}
                            </td>
                            <td>{status.approvalStatus}</td>
                            <td>{status.internalReleaseNote}</td>
                            <td>{`${status.methodologyVersion + 1}`}</td>{' '}
                            {/* +1 because version starts from 0 in DB */}
                            <td>
                              {status.createdByEmail ? (
                                <a href={`mailto:${status.createdByEmail}`}>
                                  {status.createdByEmail}
                                </a>
                              ) : (
                                'Not available'
                              )}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </LoadingSpinner>
                </>
              )}
            </>
          ) : (
            <MethodologyStatusEditPage
              methodology={currentMethodology}
              statusPermissions={permissions}
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
