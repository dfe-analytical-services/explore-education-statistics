import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import WarningMessage from '@common/components/WarningMessage';
import { useMobileMedia } from '@common/hooks/useMedia';
import ReleaseSummaryBlock from '@common/modules/release/components/ReleaseSummaryBlock';
import {
  PublicationSummaryRedesign,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import {
  formatPartialDate,
  isValidPartialDate,
} from '@common/utils/date/partialDate';
import Link from '@frontend/components/Link';
import styles from '@frontend/modules/find-statistics/components/ReleasePageIntro.module.scss';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React, { Fragment } from 'react';

interface Props {
  publicationSummary: PublicationSummaryRedesign;
  releaseVersionSummary: ReleaseVersionSummary;
}

const ReleasePageIntro = ({
  publicationSummary,
  releaseVersionSummary,
}: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();
  const {
    title,
    id,
    lastUpdated,
    published,
    publishingOrganisations,
    updateCount,
    type,
  } = releaseVersionSummary;

  const { nextReleaseDate, latestRelease, supersededByPublication } =
    publicationSummary;

  return (
    <>
      {supersededByPublication && (
        <WarningMessage testId="superseded-warning">
          This publication has been superseded by{' '}
          <Link
            testId="superseded-by-link"
            to={`/find-statistics/${supersededByPublication.slug}`}
          >
            {supersededByPublication.title}
          </Link>
        </WarningMessage>
      )}

      <div className={styles.container}>
        {!supersededByPublication && releaseVersionSummary.latestRelease ? (
          <Tag>Latest release</Tag>
        ) : (
          <>
            <Tag colour="orange">Not the latest release</Tag>
            <Link
              className="govuk-!-display-none-print"
              unvisited
              to={`/find-statistics/${publicationSummary.slug}/${latestRelease.slug}?redesign=true`} // TODO EES-6449 remove query param when live
            >
              View latest release: {latestRelease.title}
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
          to={`/find-statistics/${publicationSummary.slug}/releases`}
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
          lastUpdated={lastUpdated} // TODO change to `published` once API changes are made
          isLatestRelease={!!latestRelease}
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
            updateCount > 1 ? (
              <Link
                to={`/find-statistics/${publicationSummary.slug}/${latestRelease.slug}/updates`}
              >
                {updateCount} updates{' '}
                <VisuallyHidden>for `${title}`</VisuallyHidden>
              </Link>
            ) : undefined
          }
          onShowReleaseTypeModal={() =>
            logEvent({
              category: `${publicationSummary.title} release page`,
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
