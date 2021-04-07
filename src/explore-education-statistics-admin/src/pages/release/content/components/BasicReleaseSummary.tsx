import { getReleaseStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import metaService from '@admin/services/metaService';
import { EditableRelease } from '@admin/services/releaseContentService';
import FormattedDate from '@common/components/FormattedDate';
import Tag from '@common/components/Tag';
import { Dictionary } from '@common/types';
import {
  formatPartialDate,
  isValidPartialDate,
} from '@common/utils/date/partialDate';
import { parseISO } from 'date-fns';
import React, { useEffect, useState } from 'react';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ReleaseNotesSection from './ReleaseNotesSection';

interface ReleaseTypeIcon {
  url: string;
  altText: string;
}

const nationalStatisticsLogo: ReleaseTypeIcon = {
  url: '/assets/images/UKSA-quality-mark.jpg',
  altText: 'UK statistics authority quality mark',
};

interface Props {
  release: EditableRelease;
}

const BasicReleaseSummary = ({ release }: Props) => {
  const [releaseTypeIdsToIcons, setReleaseTypeIdsToIcons] = useState<
    Dictionary<ReleaseTypeIcon>
  >();

  useEffect(() => {
    metaService.getReleaseTypes().then(types => {
      const icons: Dictionary<ReleaseTypeIcon> = {};

      // TODO would be nicer to control this via some metadata from the back end rather than
      // trust this matching on title
      const nationalStatisticsType = types.find(
        type => type.title === 'National Statistics',
      );

      if (nationalStatisticsType) {
        icons[nationalStatisticsType.id] = nationalStatisticsLogo;
      }

      setReleaseTypeIdsToIcons(icons);
    });
  }, []);

  return (
    <>
      {releaseTypeIdsToIcons && (
        <>
          <div className="dfe-flex dfe-align-items--center dfe-justify-content--space-between">
            <div className="dfe-flex govuk-!-margin-bottom-3">
              <Tag>{getReleaseStatusLabel(release.status)}</Tag>
            </div>
            {releaseTypeIdsToIcons[release.type.id] && (
              <div className="dfe-flex">
                <img
                  src={releaseTypeIdsToIcons[release.type.id].url}
                  alt={releaseTypeIdsToIcons[release.type.id].altText}
                  height="120"
                  width="120"
                />
              </div>
            )}
          </div>

          <SummaryList>
            <SummaryListItem term="Publish date">
              {release.publishScheduled ? (
                <FormattedDate>
                  {parseISO(release.publishScheduled)}
                </FormattedDate>
              ) : (
                <p>TBA</p>
              )}
            </SummaryListItem>
            {isValidPartialDate(release.nextReleaseDate) && (
              <SummaryListItem term="Next update">
                <time>{formatPartialDate(release.nextReleaseDate)}</time>
              </SummaryListItem>
            )}
            {release.updates && release.updates.length > 0 && (
              <SummaryListItem term="Last updated">
                <FormattedDate>{release.updates[0].on}</FormattedDate>

                <ReleaseNotesSection release={release} />
              </SummaryListItem>
            )}
            <SummaryListItem term="Receive updates">
              <a className="dfe-print-hidden govuk-!-font-weight-bold" href="#">
                Sign up for email alerts
              </a>
            </SummaryListItem>
          </SummaryList>
        </>
      )}
    </>
  );
};

export default BasicReleaseSummary;
