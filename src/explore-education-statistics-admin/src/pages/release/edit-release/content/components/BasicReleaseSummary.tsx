import { getReleaseStatusLabel } from '@admin/pages/release/util/releaseSummaryUtil';
import commonService from '@admin/services/common/service';
import {
  dayMonthYearIsComplete,
  dayMonthYearToDate,
} from '@admin/services/common/types';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
import FormattedDate from '@common/components/FormattedDate';
import { Dictionary } from '@common/types';
import React, { useEffect, useState } from 'react';

interface ReleaseTypeIcon {
  url: string;
  altText: string;
}

const nationalStatisticsLogo: ReleaseTypeIcon = {
  url: '/static/images/UKSA-quality-mark.jpg',
  altText: 'UK statistics authority quality mark',
};

interface Props {
  release: ReleaseSummaryDetails;
}

const BasicReleaseSummary = ({ release }: Props) => {
  const [releaseTypeIdsToIcons, setReleaseTypeIdsToIcons] = useState<
    Dictionary<ReleaseTypeIcon>
  >();

  useEffect(() => {
    commonService.getReleaseTypes().then(types => {
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
            <span className="govuk-tag">
              {getReleaseStatusLabel(release.status)}
            </span>

            <dl className="dfe-meta-content">
              <dt className="govuk-caption-m">Publish date: </dt>
              <dd>
                <strong>
                  <FormattedDate>{release.publishScheduled}</FormattedDate>
                </strong>
              </dd>
              <div>
                <dt className="govuk-caption-m">Next update: </dt>
                <dd>
                  {dayMonthYearIsComplete(release.nextReleaseDate) && (
                    <strong>
                      <FormattedDate>
                        {dayMonthYearToDate(release.nextReleaseDate)}
                      </FormattedDate>
                    </strong>
                  )}
                </dd>
              </div>
            </dl>
          </div>

          {releaseTypeIdsToIcons[release.typeId] && (
            <div className="govuk-grid-column-one-quarter">
              <img
                src={releaseTypeIdsToIcons[release.typeId].url}
                alt={releaseTypeIdsToIcons[release.typeId].altText}
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
