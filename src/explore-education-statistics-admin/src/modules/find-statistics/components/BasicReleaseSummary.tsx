import { getReleaseStatusLabel } from '@admin/pages/release/util/releaseSummaryUtil';
import metaService from '@admin/services/metaService';
import { EditableContentBlock } from '@admin/services/types/content';
import FormattedDate from '@common/components/FormattedDate';
import Tag from '@common/components/Tag';
import { Release } from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import {
  formatDayMonthYear,
  isValidDayMonthYear,
} from '@common/utils/date/dayMonthYear';
import React, { useEffect, useState } from 'react';
import styles from './BasicReleaseSummary.module.scss';

interface ReleaseTypeIcon {
  url: string;
  altText: string;
}

const nationalStatisticsLogo: ReleaseTypeIcon = {
  url: '/static/images/UKSA-quality-mark.jpg',
  altText: 'UK statistics authority quality mark',
};

interface Props {
  release: Release<EditableContentBlock>;
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
          <div className="govuk-grid-column-three-quarters">
            {release.prerelease ? (
              <Tag className={`${styles.ragStatusRed}`}>Pre Release</Tag>
            ) : (
              <Tag>{getReleaseStatusLabel(release.status)}</Tag>
            )}

            <dl className="dfe-meta-content">
              <dt className="govuk-caption-m">Publish date: </dt>
              <dd>
                {release.publishScheduled && (
                  <strong>
                    <FormattedDate>{release.publishScheduled}</FormattedDate>
                  </strong>
                )}
              </dd>
              {isValidDayMonthYear(release.nextReleaseDate) && (
                <div>
                  <dt className="govuk-caption-m">Next update: </dt>
                  <dd>
                    <strong>
                      <time>{formatDayMonthYear(release.nextReleaseDate)}</time>
                    </strong>
                  </dd>
                </div>
              )}
            </dl>
          </div>

          {releaseTypeIdsToIcons[release.type.id] && (
            <div className="govuk-grid-column-one-quarter">
              <img
                src={releaseTypeIdsToIcons[release.type.id].url}
                alt={releaseTypeIdsToIcons[release.type.id].altText}
                height="120"
                width="120"
              />
            </div>
          )}
        </>
      )}
    </>
  );
};

export default BasicReleaseSummary;
