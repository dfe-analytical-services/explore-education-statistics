import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import { EditableRelease } from '@admin/services/releaseContentService';
import FormattedDate from '@common/components/FormattedDate';
import { releaseTypes } from '@common/services/types/releaseType';
import Tag from '@common/components/Tag';
import { Dictionary } from '@common/types';
import {
  formatPartialDate,
  isValidPartialDate,
} from '@common/utils/date/partialDate';
import { parseISO } from 'date-fns';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React from 'react';
import ReleaseNotesSection from './ReleaseNotesSection';

interface ReleaseTypeIcon {
  url: string;
  altText: string;
}

const nationalStatisticsLogo: ReleaseTypeIcon = {
  url: '/assets/images/UKSA-quality-mark.jpg',
  altText: 'UK statistics authority quality mark',
};

const releaseTypesToIcons: Dictionary<ReleaseTypeIcon> = {
  [releaseTypes.NationalStatistics]: nationalStatisticsLogo,
};

interface Props {
  release: EditableRelease;
}

const BasicReleaseSummary = ({ release }: Props) => {
  const releaseDate = release.published ?? release.publishScheduled;
  return (
    <>
      <div className="dfe-flex dfe-align-items--center dfe-justify-content--space-between govuk-!-margin-bottom-3">
        <div>
          <Tag className="govuk-!-margin-right-3 govuk-!-margin-bottom-3">
            {getReleaseApprovalStatusLabel(release.approvalStatus)}
          </Tag>
          <Tag>{releaseTypes[release.type]}</Tag>
        </div>
        {releaseTypesToIcons[release.type] && (
          <img
            src={releaseTypesToIcons[release.type].url}
            alt={releaseTypesToIcons[release.type].altText}
            height="60"
            width="60"
          />
        )}
      </div>

      <SummaryList>
        <SummaryListItem term="Published">
          {releaseDate ? (
            <FormattedDate>{parseISO(releaseDate)}</FormattedDate>
          ) : (
            <p>TBA</p>
          )}
        </SummaryListItem>
        {isValidPartialDate(release.nextReleaseDate) && (
          <SummaryListItem term="Next update">
            <time>{formatPartialDate(release.nextReleaseDate)}</time>
          </SummaryListItem>
        )}
        <SummaryListItem term="Last updated">
          {release.updates && release.updates.length > 0 ? (
            <FormattedDate>release.updates[0].on</FormattedDate>
          ) : (
            'TBA'
          )}
          <ReleaseNotesSection release={release} />
        </SummaryListItem>
        <SummaryListItem term="Receive updates">
          <a className="dfe-print-hidden govuk-!-font-weight-bold" href="#">
            Sign up for email alerts
          </a>
        </SummaryListItem>
      </SummaryList>
    </>
  );
};

export default BasicReleaseSummary;
