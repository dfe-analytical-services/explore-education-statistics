import ButtonLink from '@admin/components/ButtonLink';
import {
  dayMonthYearIsComplete,
  dayMonthYearToDate,
} from '@admin/services/common/types';
import {
  AdminDashboardRelease,
  ReleaseApprovalStatus,
} from '@admin/services/dashboard/types';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React, { useContext } from 'react';
import { format } from 'date-fns';
import Details from '@common/components/Details';
import { LoginContext } from '@admin/components/Login';
import { summaryRoute } from '@admin/routes/edit-release/routes';

const getLiveLatestLabel = (isLive: boolean, isLatest: boolean) => {
  if (isLive && isLatest) {
    return '(Live - Latest release)';
  }
  if (isLive) {
    return '(Live)';
  }
  return '(not Live)';
};

const getTag = (approvalStatus: ReleaseApprovalStatus) => {
  if (ReleaseApprovalStatus.ReadyToReview === approvalStatus) {
    return 'Ready to review';
  }
  return undefined;
};

interface Props {
  publicationId: string;
  release: AdminDashboardRelease;
}

const DashboardReleaseSummary = ({ publicationId, release }: Props) => {
  const authentication = useContext(LoginContext);

  const editorName =
    authentication.user && authentication.user.id === release.lastEditedUser.id
      ? 'me'
      : release.lastEditedUser.name;

  const releaseSummaryLabel = `${release.timePeriodCoverage.label}, ${
    release.releaseName
  } 
     ${getLiveLatestLabel(release.live, release.latestRelease)}`;

  return (
    <Details
      className="govuk-!-margin-bottom-0"
      summary={releaseSummaryLabel}
      tag={getTag(release.status)}
    >
      <ButtonLink to={summaryRoute.generateLink(publicationId, release.id)}>
        Edit this release
      </ButtonLink>

      <SummaryList additionalClassName="govuk-!-margin-bottom-3">
        <SummaryListItem term="Publish date">
          <FormattedDate>
            {release.published || release.publishScheduled}
          </FormattedDate>
        </SummaryListItem>
        <SummaryListItem term="Next release date">
          {dayMonthYearIsComplete(release.nextReleaseDate) && (
            <FormattedDate>
              {dayMonthYearToDate(release.nextReleaseDate)}
            </FormattedDate>
          )}
        </SummaryListItem>
        <SummaryListItem term="Lead statistician">
          {release.contact && (
            <span>
              {release.contact.contactName}
              <br />
              <a href="mailto:{lead.teamEmail}">{release.contact.teamEmail}</a>
              <br />
              {release.contact.contactTelNo}
            </span>
          )}
        </SummaryListItem>
        <SummaryListItem term="Last edited" detailsNoMargin>
          <FormattedDate>{release.lastEditedDateTime}</FormattedDate>
          {' at '}
          {format(new Date(release.lastEditedDateTime), 'HH:mm')} by{' '}
          <a href="#">{editorName}</a>
        </SummaryListItem>
        {release.internalReleaseNote && (
          <SummaryListItem term="Internal release note" breakNewLines>
            {release.internalReleaseNote}
          </SummaryListItem>
        )}
      </SummaryList>
    </Details>
  );
};

export default DashboardReleaseSummary;
