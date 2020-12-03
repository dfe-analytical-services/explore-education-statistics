import ReleaseServiceStatus from '@admin/components/ReleaseServiceStatus';
import StatusBlock from '@admin/components/StatusBlock';
import { useConfig } from '@admin/contexts/ConfigContext';
import ReleaseStatusForm from '@admin/pages/release/components/ReleaseStatusForm';
import { useManageReleaseContext } from '@admin/pages/release/contexts/ManageReleaseContext';
import permissionService, {
  ReleaseStatusPermissions,
} from '@admin/services/permissionService';
import releaseService from '@admin/services/releaseService';
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
import { formatISO, parseISO } from 'date-fns';
import React from 'react';

const statusMap: {
  [keyof: string]: string;
} = {
  Draft: 'In Draft',
  HigherLevelReview: 'Awaiting higher review',
  Approved: 'Approved',
};

const ReleaseStatusPage = () => {
  const [showForm, toggleShowForm] = useToggle(false);

  const { releaseId, onChangeReleaseStatus } = useManageReleaseContext();

  const { value: release, setState: setRelease } = useAsyncHandledRetry(
    () => releaseService.getRelease(releaseId),
    [showForm],
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

  if (showForm && statusPermissions) {
    return (
      <ReleaseStatusForm
        release={release}
        statusPermissions={statusPermissions}
        onCancel={toggleShowForm.off}
        onSubmit={async values => {
          const nextRelease = await releaseService.updateRelease(releaseId, {
            ...release,
            typeId: release.type.id,
            ...values,
            publishScheduled: values.publishScheduled
              ? formatISO(values.publishScheduled, {
                  representation: 'date',
                })
              : undefined,
          });

          setRelease({ value: nextRelease });

          toggleShowForm.off();
          onChangeReleaseStatus();
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
            text={statusMap[release.status]}
            id={`CurrentReleaseStatus-${statusMap[release.status]}`}
          />
        </SummaryListItem>
        {release.status === 'Approved' && (
          <SummaryListItem term="Release process status">
            <ReleaseServiceStatus releaseId={releaseId} />
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
        <Button className="govuk-!-margin-top-2" onClick={toggleShowForm.on}>
          Edit release status
        </Button>
      )}
    </>
  );
};

export default ReleaseStatusPage;
