import Link from '@admin/components/Link';
import StatusBlock from '@admin/components/StatusBlock';
import { useConfig } from '@admin/contexts/ConfigContext';
import { useLastLocation } from '@admin/contexts/LastLocationContext';
import ReleasePublishingStatus from '@admin/pages/release/components/ReleasePublishingStatus';
import { useReleaseVersionContext } from '@admin/pages/release/contexts/ReleaseVersionContext';
import ReleaseStatusEditPage from '@admin/pages/release/ReleaseStatusEditPage';
import {
  releasePreReleaseAccessRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import permissionService, {
  ReleaseStatusPermissions,
} from '@admin/services/permissionService';
import releaseVersionService, {
  ReleaseVersionStageStatus,
  ReleaseStatus,
} from '@admin/services/releaseVersionService';
import Button from '@common/components/Button';
import FormattedDate from '@common/components/FormattedDate';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import UrlContainer from '@common/components/UrlContainer';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import useToggle from '@common/hooks/useToggle';
import {
  formatPartialDate,
  isValidPartialDate,
} from '@common/utils/date/partialDate';
import { parseISO, isAfter, isBefore } from 'date-fns';
import React from 'react';
import { generatePath, useLocation } from 'react-router';

const statusMap: {
  [keyof: string]: string;
} = {
  Draft: 'In Draft',
  HigherLevelReview: 'Awaiting higher review',
  Approved: 'Approved',
};

export default function ReleaseStatusPage() {
  const [isEditing, toggleEditing] = useToggle(false);
  const [showWarning, toggleWarning] = useToggle(false);

  const location = useLocation();
  const lastLocation = useLastLocation();

  const {
    releaseVersionId,
    releaseVersion: contextRelease,
    onReleaseChange,
  } = useReleaseVersionContext();

  const { value: releaseVersion, setState: setRelease } = useAsyncHandledRetry(
    async () =>
      lastLocation && lastLocation !== location
        ? releaseVersionService.getReleaseVersion(releaseVersionId)
        : contextRelease,
    [releaseVersionId],
  );

  const { value: releaseStatuses } = useAsyncRetry<ReleaseStatus[]>(
    () => releaseVersionService.getReleaseVersionStatuses(releaseVersionId),
    [releaseVersionId, releaseVersion],
  );

  const { value: statusPermissions } = useAsyncRetry<ReleaseStatusPermissions>(
    () => permissionService.getReleaseStatusPermissions(releaseVersionId),
  );

  const { publicAppUrl } = useConfig();

  const handlePublishingStatusChange = (status: ReleaseVersionStageStatus) => {
    if (status.overallStage === 'Complete') {
      onReleaseChange();
    }
  };

  if (!releaseVersion) {
    return <LoadingSpinner />;
  }
  const isEditable =
    !!statusPermissions &&
    Object.values(statusPermissions).some(permission => permission);

  if (isEditing && statusPermissions) {
    return (
      <ReleaseStatusEditPage
        releaseVersion={releaseVersion}
        statusPermissions={statusPermissions}
        onCancel={toggleEditing.off}
        onUpdate={nextRelease => {
          setRelease({ value: nextRelease });
          onReleaseChange();
          toggleEditing.off();
        }}
      />
    );
  }

  return (
    <>
      <h2>Sign off</h2>
      <p>
        The <strong>public release</strong> will be accessible at:
      </p>

      <UrlContainer
        className="govuk-!-margin-bottom-4"
        id="public-release-url"
        url={`${publicAppUrl}/find-statistics/${releaseVersion.publicationSlug}/${releaseVersion.slug}`}
      />

      <SummaryList>
        <SummaryListItem term="Current status">
          <StatusBlock
            text={statusMap[releaseVersion.approvalStatus]}
            id={`CurrentReleaseStatus-${
              statusMap[releaseVersion.approvalStatus]
            }`}
          />
        </SummaryListItem>
        {releaseVersion.approvalStatus === 'Approved' && (
          <SummaryListItem term="Release process status">
            <ReleasePublishingStatus
              releaseVersionId={releaseVersionId}
              refreshPeriod={1000}
              onChange={handlePublishingStatusChange}
            />
          </SummaryListItem>
        )}
        <SummaryListItem term="Scheduled release">
          {releaseVersion.publishScheduled ? (
            <FormattedDate>
              {parseISO(releaseVersion.publishScheduled)}
            </FormattedDate>
          ) : (
            'Not scheduled'
          )}
        </SummaryListItem>
        <SummaryListItem term="Next release expected">
          {isValidPartialDate(releaseVersion.nextReleaseDate) ? (
            <time>{formatPartialDate(releaseVersion.nextReleaseDate)}</time>
          ) : (
            'Not set'
          )}
        </SummaryListItem>
      </SummaryList>

      {isEditable && (
        <Button
          className="govuk-!-margin-top-2"
          onClick={
            inPublishingWindow(releaseVersion.publishScheduled)
              ? toggleWarning.on
              : toggleEditing.on
          }
        >
          Edit release status
        </Button>
      )}

      {releaseStatuses && releaseStatuses.length > 0 && (
        <>
          <h3>Release status history</h3>
          <LoadingSpinner
            loading={!releaseStatuses}
            text="Loading release status history"
          >
            <table data-testid="release-status-history">
              <thead>
                <tr>
                  <th scope="col">Date</th>
                  <th scope="col">Status</th>
                  <th scope="col">Internal note</th>
                  <th scope="col">Release version</th>
                  <th scope="col">By user</th>
                </tr>
              </thead>
              <tbody>
                {releaseStatuses.map(status => (
                  <tr key={status.releaseStatusId}>
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
                    <td>{`${status.releaseVersion + 1}`}</td>
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

      <ModalConfirm
        confirmText="Continue"
        title="Important"
        open={showWarning}
        onConfirm={() => {
          toggleWarning.off();
          toggleEditing.on();
        }}
        onExit={toggleWarning.off}
        onCancel={toggleWarning.off}
      >
        <p>
          This is a release scheduled for publication today. If you change the
          status away from approved, you will cancel the publication of this
          release and will not be able to reschedule for today.
        </p>

        <p>
          If you are wanting to add additional emails to pre-release access, you
          can do this without changing the release status on the{' '}
          <Link
            to={generatePath<ReleaseRouteParams>(
              releasePreReleaseAccessRoute.path,
              {
                publicationId: releaseVersion.publicationId,
                releaseVersionId,
              },
            )}
          >
            pre-release tab
          </Link>
          .
        </p>

        <p>
          If you have any issues or questions, please contact{' '}
          <a href="mailto:explore.statistics@education.gov.uk">
            explore.statistics@education.gov.uk
          </a>{' '}
          for support.
        </p>
      </ModalConfirm>
    </>
  );
}

function inPublishingWindow(publishScheduled?: string) {
  if (!publishScheduled) {
    return false;
  }

  const startWindow = new Date(`${publishScheduled} 00:00`);
  const endWindow = new Date(`${publishScheduled} 09:30`);
  const now = new Date();
  return isAfter(now, startWindow) && isBefore(now, endWindow);
}
