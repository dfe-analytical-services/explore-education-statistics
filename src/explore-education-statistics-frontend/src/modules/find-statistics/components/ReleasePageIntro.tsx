import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import WarningMessage from '@common/components/WarningMessage';
import { useMobileMedia } from '@common/hooks/useMedia';
import ReleaseSummaryBlock from '@common/modules/release/components/ReleaseSummaryBlock';
import { ReleaseVersion } from '@common/services/publicationService';
import {
  formatPartialDate,
  isValidPartialDate,
} from '@common/utils/date/partialDate';
import Link from '@frontend/components/Link';
import styles from '@frontend/modules/find-statistics/components/ReleasePageIntro.module.scss';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React, { Fragment } from 'react';

interface Props {
  releaseVersion: ReleaseVersion;
}

const ReleasePageIntro = ({ releaseVersion }: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();
  const {
    publication,
    title,
    nextReleaseDate,
    latestRelease,
    id,
    published,
    publishingOrganisations,
    updates,
    type,
  } = releaseVersion;
  const latestReleaseSeriesItem = publication.releaseSeries.find(
    rsi => !rsi.isLegacyLink,
  );

  return (
    <>
      {publication.isSuperseded && (
        <WarningMessage testId="superseded-warning">
          This publication has been superseded by{' '}
          <Link
            testId="superseded-by-link"
            to={`/find-statistics/${releaseVersion.publication.supersededBy?.slug}`}
          >
            {releaseVersion.publication.supersededBy?.title}
          </Link>
        </WarningMessage>
      )}

      <div className={styles.container}>
        {!publication.isSuperseded && releaseVersion.latestRelease ? (
          <Tag>Latest release</Tag>
        ) : (
          <>
            <Tag colour="orange">Not the latest release</Tag>
            <Link
              className="govuk-!-display-none-print"
              unvisited
              to={`/find-statistics/${publication.slug}/${latestReleaseSeriesItem?.releaseSlug}?redesign=true`} // TODO EES-6449 remove query param when live
            >
              View latest release: {latestReleaseSeriesItem?.description}
            </Link>
          </>
        )}

        {latestRelease && isValidPartialDate(nextReleaseDate) && (
          <p className="govuk-!-margin-bottom-0">
            Next release{' '}
            <time className="govuk-!-font-weight-bold">
              {formatPartialDate(nextReleaseDate)}
            </time>
          </p>
        )}

        <Link
          to={`/find-statistics/${publication.slug}/releases`}
          className="govuk-!-display-none-print"
        >
          All releases in this series
        </Link>

        <Link
          to={`${process.env.CONTENT_API_BASE_URL}/releases/${id}/files?fromPage=ReleaseDownloads`}
          className="govuk-!-display-none-print"
          onClick={() => {
            logEvent({
              category: 'Downloads',
              action: `Release page all files, Release: ${title}, File: All files`,
            });
          }}
        >
          Download all data (ZIP)
        </Link>
      </div>

      {!isMobileMedia && (
        <ReleaseSummaryBlock
          lastUpdated={updates[0]?.on} // TODO change to `published` once API changes are made
          latestRelease={latestRelease}
          releaseDate={published}
          releaseType={type}
          renderProducerLink={
            publishingOrganisations?.length ? (
              <span>
                {publishingOrganisations.map((org, index) => (
                  <Fragment key={org.id}>
                    {index > 0 && ' and '}
                    <Link
                      unvisited
                      to={org.url}
                      className="govuk-link--no-underline"
                    >
                      {org.title}
                    </Link>
                  </Fragment>
                ))}
              </span>
            ) : (
              <Link
                unvisited
                className="govuk-link--no-underline"
                to="https://www.gov.uk/government/organisations/department-for-education"
              >
                Department for Education
              </Link>
            )
          }
          renderUpdatesLink={
            updates.length > 0 ? (
              <Link
                to={`/find-statistics/${publication.slug}/${latestReleaseSeriesItem?.releaseSlug}/updates`}
              >
                {updates.length} update{updates.length > 1 ? 's' : ''}{' '}
                <VisuallyHidden>for `${title}`</VisuallyHidden>
              </Link>
            ) : undefined
          }
          onShowReleaseTypeModal={() =>
            logEvent({
              category: `${publication.title} release page`,
              action: 'Release type clicked',
              label: window.location.pathname,
            })
          }
        />
      )}
    </>
  );
};

export default ReleasePageIntro;
