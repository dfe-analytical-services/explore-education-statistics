import ReleaseServiceStatus from '@admin/components/ReleaseServiceStatus';
import {
  getReleaseStatusLabel,
  getReleaseSummaryLabel,
} from '@admin/pages/release/utils/releaseSummaryUtil';
import { Release } from '@admin/services/releaseService';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import TagGroup from '@common/components/TagGroup';
import {
  formatPartialDate,
  isValidPartialDate,
} from '@common/utils/date/partialDate';
import React, { ReactNode } from 'react';
import LazyLoad from 'react-lazyload';

interface Props {
  release: Release;
  actions: ReactNode;
  secondaryActions?: ReactNode;
  open?: boolean;
  children?: ReactNode;
}

const ReleaseSummary = ({
  release,
  actions,
  secondaryActions,
  open = false,
  children,
}: Props) => {
  return (
    <Details
      open={open}
      className="govuk-!-margin-bottom-0"
      summary={getReleaseSummaryLabel(release)}
      summaryAfter={
        <TagGroup className="govuk-!-margin-left-2">
          <Tag>{getReleaseStatusLabel(release.status)}</Tag>

          {release.amendment && <Tag>Amendment</Tag>}

          {release.status === 'Approved' && (
            <LazyLoad
              once
              scroll={false}
              placeholder={
                <LoadingSpinner className="govuk-!-margin-0" inline size="sm" />
              }
            >
              <ReleaseServiceStatus exclude="details" releaseId={release.id} />
            </LazyLoad>
          )}
        </TagGroup>
      }
    >
      <SummaryList className="govuk-!-margin-bottom-3">
        <SummaryListItem term="Publish date">
          <FormattedDate>
            {release.published || release.publishScheduled || ''}
          </FormattedDate>
        </SummaryListItem>

        {isValidPartialDate(release.nextReleaseDate) && (
          <SummaryListItem term="Next release date">
            <time>{formatPartialDate(release.nextReleaseDate)}</time>
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
