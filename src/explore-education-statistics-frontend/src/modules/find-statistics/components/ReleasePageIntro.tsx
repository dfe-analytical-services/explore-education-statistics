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
import classNames from 'classnames';
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
    lastUpdated,
    published,
    publishingOrganisations,
    updateCount,
    type,
  } = releaseVersionSummary;

  const { nextReleaseDate, latestRelease, supersededByPublication } =
    publicationSummary;

  // Update count includes 'first published' by default, but we only
  // want to show 'actual' update number.
  const updateCountExcludingFirstPublished = updateCount - 1;
  // We only want to show 'updated at' and updates link if there are updates.
  const showUpdatesInfo = updateCountExcludingFirstPublished > 0;

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

      <div
        className={classNames([styles.container, 'govuk-!-margin-bottom-5'])}
      >
        <div className={classNames([styles.container, 'dfe-flex-grow--1'])}>
          {!supersededByPublication && (
            <>
              {releaseVersionSummary.isLatestRelease ? (
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
            </>
          )}

          {releaseVersionSummary.isLatestRelease &&
            isValidPartialDate(nextReleaseDate) && (
              <p className="govuk-!-margin-bottom-0">
                Next release{' '}
                <time
                  className="govuk-!-font-weight-bold"
                  data-testid="Next release"
                >
                  {formatPartialDate(nextReleaseDate)}
                </time>
              </p>
            )}

          <Link
            to={`/find-statistics/${publicationSummary.slug}/releases`}
            className="govuk-!-display-none-print"
            prefetch={false}
          >
            All releases in this series
          </Link>
        </div>

        <div className={styles.container}>
          <Link
            to={`${process.env.CONTENT_API_BASE_URL}/releases/${releaseVersionSummary.id}/files?fromPage=ReleaseDownloads`}
            onClick={() => {
              logEvent({
                category: 'Downloads',
                action: `Release page all files, Release: ${releaseVersionSummary.title}, File: All files`,
              });
            }}
            id="download-all-data-link"
          >
            Download all data (ZIP)
          </Link>
          <Link
            to={
              releaseVersionSummary.isLatestRelease
                ? `/data-tables/${publicationSummary.slug}`
                : `/data-tables/${publicationSummary.slug}/${releaseVersionSummary.slug}`
            }
          >
            Create your own tables
          </Link>
        </div>
      </div>

      {!isMobileMedia && (
        <ReleaseSummaryBlock
          lastUpdated={showUpdatesInfo ? lastUpdated : undefined}
          publishingOrganisations={publishingOrganisations}
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
            showUpdatesInfo ? (
              <Link
                to={`/find-statistics/${publicationSummary.slug}/${releaseVersionSummary.slug}/updates`}
                data-testid="updates-link"
                prefetch={false}
              >
                {updateCountExcludingFirstPublished} update
                {updateCountExcludingFirstPublished === 1 ? '' : 's'}
                <VisuallyHidden>for {title}</VisuallyHidden>
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
