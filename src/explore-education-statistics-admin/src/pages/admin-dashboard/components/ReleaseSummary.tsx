import ReleaseServiceStatus from '@admin/components/ReleaseServiceStatus';
import {
  getReleaseStatusLabel,
  getReleaseSummaryLabel,
} from '@admin/pages/release/util/releaseSummaryUtil';
import { AdminDashboardRelease } from '@admin/services/dashboard/types';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import {
  dayMonthYearIsComplete,
  dayMonthYearToDate,
} from '@common/services/publicationService';
import React, { ReactNode } from 'react';

interface Props {
  release: AdminDashboardRelease;
  actions: ReactNode;
  children?: ReactNode;
}

const ReleaseSummary = ({ release, actions, children }: Props) => {
  return (
    <Details
      className="govuk-!-margin-bottom-0"
      summary={getReleaseSummaryLabel(release)}
      tag={[
        getReleaseStatusLabel(release.status),
        // eslint-disable-next-line react/jsx-key
        release.status !== 'Draft' &&
          release.status !== 'HigherLevelReview' && (
            <ReleaseServiceStatus exclude="details" releaseId={release.id} />
          ),
      ]}
    >
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
        <SummaryListItem term="Release process status">
          <ReleaseServiceStatus releaseId={release.id} />
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
        <div className="govuk-grid-column-one-half">{actions}</div>
        <div className="govuk-grid-column-one-half dfe-align--right" />
      </div>
    </Details>
  );
};

export default ReleaseSummary;
