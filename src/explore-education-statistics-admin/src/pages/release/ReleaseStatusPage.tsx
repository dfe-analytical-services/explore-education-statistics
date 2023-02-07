import StatusBlock from '@admin/components/StatusBlock';
import { useConfig } from '@admin/contexts/ConfigContext';
import { useLastLocation } from '@admin/contexts/LastLocationContext';
import ReleasePublishingStatus from '@admin/pages/release/components/ReleasePublishingStatus';
import { useReleaseContext } from '@admin/pages/release/contexts/ReleaseContext';
import ReleaseStatusEditPage from '@admin/pages/release/ReleaseStatusEditPage';
import permissionService, {
  ReleaseStatusPermissions,
} from '@admin/services/permissionService';
import releaseService, { ReleaseStatus } from '@admin/services/releaseService';
import Button from '@common/components/Button';
import FormattedDate from '@common/components/FormattedDate';
import LoadingSpinner from '@common/components/LoadingSpinner';
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
import { parseISO } from 'date-fns';
import React from 'react';
import { useLocation } from 'react-router';

const statusMap: {
  [keyof: string]: string;
} = {
  Draft: 'In Draft',
  HigherLevelReview: 'Awaiting higher review',
  Approved: 'Approved',
};

const ReleaseStatusPage = () => {
  const [isEditing, toggleEditing] = useToggle(false);

  const location = useLocation();
  const lastLocation = useLastLocation();

  const {
    releaseId,
    release: contextRelease,
    onReleaseChange,
  } = useReleaseContext();

  const {
    value: release,
    setState: setRelease,
  } = useAsyncHandledRetry(
    async () =>
      lastLocation && lastLocation !== location
        ? releaseService.getRelease(releaseId)
        : contextRelease,
    [releaseId],
  );

  const { value: releaseStatuses } = useAsyncRetry<ReleaseStatus[]>(
    () => releaseService.getReleaseStatuses(releaseId),
    [releaseId, release],
  );

  const { value: statusPermissions } = useAsyncRetry<ReleaseStatusPermissions>(
    () => permissionService.getReleaseStatusPermissions(releaseId),
  );

  const { PublicAppUrl } = useConfig();

  if (!release) {
    return <LoadingSpinner />;
  }

  const isEditable =
    !!statusPermissions &&
    Object.values(statusPermissions).some(permission => permission);

  if (isEditing && statusPermissions) {
    return (
      <ReleaseStatusEditPage
        release={release}
        statusPermissions={statusPermissions}
        onCancel={toggleEditing.off}
        onUpdate={nextRelease => {
          setRelease({ value: nextRelease });
          onReleaseChange(nextRelease);
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

      <p>
        <UrlContainer
          data-testid="public-release-url"
          url={`${PublicAppUrl}/find-statistics/${release.publicationSlug}/${release.slug}`}
        />
      </p>

      <SummaryList>
        <SummaryListItem term="Current status">
          <StatusBlock
            text={statusMap[release.approvalStatus]}
            id={`CurrentReleaseStatus-${statusMap[release.approvalStatus]}`}
          />
        </SummaryListItem>
        {release.approvalStatus === 'Approved' && (
          <SummaryListItem term="Release process status">
            <ReleasePublishingStatus
              releaseId={releaseId}
              refreshPeriod={1000}
            />
          </SummaryListItem>
        )}
        <SummaryListItem term="Scheduled release">
          {release.publishScheduled ? (
            <FormattedDate>{parseISO(release.publishScheduled)}</FormattedDate>
          ) : (
            'Not scheduled'
          )}
        </SummaryListItem>
        <SummaryListItem term="Next release expected">
          {isValidPartialDate(release.nextReleaseDate) ? (
            <time>{formatPartialDate(release.nextReleaseDate)}</time>
          ) : (
            'Not set'
          )}
        </SummaryListItem>
      </SummaryList>

      {isEditable && (
        <Button className="govuk-!-margin-top-2" onClick={toggleEditing.on}>
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
              {releaseStatuses && (
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
                      <td>{`${status.releaseVersion + 1}`}</td>{' '}
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
              )}
            </table>
          </LoadingSpinner>
        </>
      )}
    </>
  );
};

export default ReleaseStatusPage;
