import useReleasePublishingStatus from '@admin/pages/release/hooks/useReleasePublishingStatus';
import publicationSummaryStyles from '@admin/pages/admin-dashboard/components/PublicationSummary.module.scss';
import ReleasePublishingStatus from '@admin/pages/release/components/ReleasePublishingStatus';
import ReleasePublishingStatusTag from '@admin/pages/release/components/ReleasePublishingStatusTag';
import {
  getReleaseApprovalStatusLabel,
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
import classNames from 'classnames';

interface Props {
  release: Release;
  actions: ReactNode;
  open?: boolean;
  children?: ReactNode;
}

const ReleaseSummary = ({
  release,
  actions,
  open = false,
  children,
}: Props) => {
  const { currentStatus, currentStatusDetail } = useReleasePublishingStatus({
    releaseId: release.id,
  });
  return (
    <Details
      open={open}
      className="govuk-!-margin-bottom-0"
      summary={getReleaseSummaryLabel(release)}
      summaryAfter={
        <TagGroup className="govuk-!-margin-left-2">
          {release.approvalStatus !== 'Approved' && (
            <>
              {' '}
              <Tag>{getReleaseApprovalStatusLabel(release.approvalStatus)}</Tag>
            </>
          )}
          {release.approvalStatus === 'Approved' && (
            <LazyLoad
              once
              placeholder={
                <LoadingSpinner className="govuk-!-margin-0" inline size="sm" />
              }
            >
              <>
                {' '}
                <ReleasePublishingStatusTag
                  currentStatus={currentStatus}
                  color={currentStatusDetail.color}
                  isApproved
                />
              </>
            </LazyLoad>
          )}
          {release.amendment && (
            <>
              {' '}
              <Tag>Amendment</Tag>
            </>
          )}
        </TagGroup>
      }
    >
      <div className={publicationSummaryStyles.detailsInner}>
        <div className={publicationSummaryStyles.sectionContent}>
          <SummaryList className="govuk-!-margin-bottom-3">
            <SummaryListItem term="Publish date">
              {release.published || release.publishScheduled ? (
                <FormattedDate>
                  {release.published || release.publishScheduled || ''}
                </FormattedDate>
              ) : (
                'Not yet published'
              )}
            </SummaryListItem>

            {isValidPartialDate(release.nextReleaseDate) && (
              <SummaryListItem term="Next release date">
                <time>{formatPartialDate(release.nextReleaseDate)}</time>
              </SummaryListItem>
            )}
            {release.approvalStatus === 'Approved' && (
              <SummaryListItem term="Release process status">
                <ReleasePublishingStatus releaseId={release.id} />
              </SummaryListItem>
            )}
            <SummaryListItem term="Lead statistician">
              {release.contact && (
                <span>
                  {release.contact.contactName}
                  <br />
                  <a href="mailto:{lead.teamEmail}">
                    {release.contact.teamEmail}
                  </a>
                  <br />
                  {release.contact.contactTelNo}
                </span>
              )}
            </SummaryListItem>
            {release.latestInternalReleaseNote && (
              <SummaryListItem term="Internal note">
                <span className="dfe-multiline-content">
                  {release.latestInternalReleaseNote}
                </span>
              </SummaryListItem>
            )}
          </SummaryList>
          {children}
        </div>
        <div
          className={classNames(
            publicationSummaryStyles.sectionActions,
            publicationSummaryStyles.detailsActions,
          )}
        >
          {actions}
        </div>
      </div>
    </Details>
  );
};

export default ReleaseSummary;
