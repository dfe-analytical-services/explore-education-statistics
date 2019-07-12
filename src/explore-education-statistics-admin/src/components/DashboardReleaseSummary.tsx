import ButtonLink from '@admin/components/ButtonLink';
import {
  dayMonthYearIsComplete,
  dayMonthYearToDate,
} from '@admin/services/api/common/types/types';
import {
  AdminDashboardRelease,
  ReleaseApprovalStatus,
} from '@admin/services/api/dashboard/types';
import FormattedDate from '@common/components/FormattedDate';
import React, { useContext } from 'react';
import { format } from 'date-fns';
import Details from '@common/components/Details';
import { LoginContext } from '@admin/components/Login';
import { setupRoute } from '@admin/routes/releaseRoutes';

const getLiveLatestLabel = (isLive: boolean, isLatest: boolean) => {
  if (isLive && isLatest) {
    return '(Live - Latest release)';
  }
  if (isLive) {
    return '(Live)';
  }
  return undefined;
};

const getTag = (approvalStatus: ReleaseApprovalStatus) => {
  if (ReleaseApprovalStatus.ReadyToReview === approvalStatus) {
    return 'Ready to review';
  }
  return undefined;
};

interface Props {
  release: AdminDashboardRelease;
}

const DashboardReleaseSummary = ({ release }: Props) => {
  const authentication = useContext(LoginContext);

  const editorName =
    authentication.user && authentication.user.id === release.lastEditedUser.id
      ? 'me'
      : release.lastEditedUser.name;

  const releaseSummaryLabel = `${release.timePeriodCoverage.label}, ${
    release.dateRangeLabel
  } 
     ${getLiveLatestLabel(release.live, release.latestRelease)}`;

  return (
    <Details
      className="govuk-!-margin-bottom-0"
      summary={releaseSummaryLabel}
      tag={getTag(release.status)}
    >
      <ButtonLink to={setupRoute.generateLink(release.id)}>
        Edit this release
      </ButtonLink>

      <dl className="govuk-summary-list govuk-!-margin-bottom-3">
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Publish date</dt>
          <dd className="govuk-summary-list__value">
            <FormattedDate>{release.publishedDate}</FormattedDate>
          </dd>
          <dd className="govuk-summary-list__actions" />
        </div>
        {dayMonthYearIsComplete(release.nextReleaseExpectedDate) && (
          <div className="govuk-summary-list__row">
            <dt className="govuk-summary-list__key">Next release date</dt>
            <dd className="govuk-summary-list__value">
              <FormattedDate>
                {dayMonthYearToDate(release.nextReleaseExpectedDate)}
              </FormattedDate>
            </dd>
            <dd className="govuk-summary-list__actions" />
          </div>
        )}
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Lead statistician</dt>
          <dd className="govuk-summary-list__value">
            {release.leadStatistician && (
              <span>
                {release.leadStatistician.name}
                <br />
                <a href="mailto:{lead.teamEmail}">
                  {release.leadStatistician.email}
                </a>
                <br />
                {release.leadStatistician.telNo}
              </span>
            )}
          </dd>
          <dd className="govuk-summary-list__actions" />
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Last edited</dt>
          <dd className="govuk-summary-list__value dfe-details-no-margin">
            <FormattedDate>{release.lastEditedDateTime}</FormattedDate>
            {' at '}
            {format(new Date(release.lastEditedDateTime), 'HH:mm')} by{' '}
            <a href="#">{editorName}</a>
          </dd>
        </div>
      </dl>
    </Details>
  );
};

export default DashboardReleaseSummary;
