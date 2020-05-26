import ReleaseServiceStatus from '@admin/components/ReleaseServiceStatus';
import {
  getReleaseStatusLabel,
  getReleaseSummaryLabel,
} from '@admin/pages/release/util/releaseSummaryUtil';
import { AdminDashboardRelease } from '@admin/services/dashboard/types';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import {
  formatDayMonthYear,
  isValidDayMonthYear,
} from '@common/utils/date/dayMonthYear';
import React, { ReactNode } from 'react';
import LazyLoad from 'react-lazyload';

interface Props {
  release: AdminDashboardRelease;
  actions: ReactNode;
  secondaryActions?: ReactNode;
  children?: ReactNode;
}

const ReleaseSummary = ({
  release,
  actions,
  secondaryActions,
  children,
}: Props) => {
  return (
    <Details
      className="govuk-!-margin-bottom-0"
      summary={getReleaseSummaryLabel(release)}
      tag={[
        getReleaseStatusLabel(release.status),
        release.amendment && 'Amendment',
        release.status === 'Approved' && (
          <LazyLoad
            scroll={false}
            placeholder={
              <LoadingSpinner className="govuk-!-margin-0" inline size="sm" />
            }
            once
          >
            <ReleaseServiceStatus exclude="details" releaseId={release.id} />
          </LazyLoad>
        ),
      ]}
    >
      <SummaryList additionalClassName="govuk-!-margin-bottom-3">
        <SummaryListItem term="Publish date">
          <FormattedDate>
            {release.published || release.publishScheduled}
          </FormattedDate>
        </SummaryListItem>

        {isValidDayMonthYear(release.nextReleaseDate) && (
          <SummaryListItem term="Next release date">
            <time>{formatDayMonthYear(release.nextReleaseDate)}</time>
          </SummaryListItem>
        )}
        {release.status === 'Approved' && (
          <SummaryListItem term="Release process status">
            <ReleaseServiceStatus releaseId={release.id} />
          </SummaryListItem>
        )}
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
        {release.internalReleaseNote && (
          <SummaryListItem term="Internal release note">
            <span className="dfe-multiline-content">
              {release.internalReleaseNote}
            </span>
          </SummaryListItem>
        )}
      </SummaryList>
      {children}

      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">{actions}</div>
        <div className="govuk-grid-column-one-third dfe-align--right">
          {secondaryActions}
        </div>
      </div>
    </Details>
  );
};

export default ReleaseSummary;
