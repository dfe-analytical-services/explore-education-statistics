import { useMobileMedia } from '@common/hooks/useMedia';
import {
  PublicationSummary,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import { Organisation } from '@common/services/types/organisation';
import PublishingOrganisations from '@common/modules/find-statistics/components/PublishingOrganisations';
import styles from '@frontend/modules/find-statistics/components/ReleasePageTitle.module.scss';
import React from 'react';
import Link from '@frontend/components/Link';
import { logEvent } from '@frontend/services/googleAnalyticsService';

interface Props {
  publicationSummary: PublicationSummary;
  releaseTitle: string;
  releaseVersionSummary: ReleaseVersionSummary;
  publishingOrganisations: Organisation[] | undefined;
}

const ReleasePageTitle = ({
  publicationSummary,
  releaseTitle,
  releaseVersionSummary,
  publishingOrganisations,
}: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <>
      <PublishingOrganisations
        publishingOrganisations={publishingOrganisations}
      />

      <div className={styles.releasePageTitleWrap}>
        <div className={styles.releasePageTitle}>
          <span className="govuk-caption-xl" data-testid="page-title-caption">
            {releaseTitle}
          </span>
          <h1
            className="govuk-heading-xl govuk-!-margin-bottom-2"
            data-testid="page-title"
          >
            {publicationSummary.title}
          </h1>
          {!isMobileMedia && (
            <p className="govuk-body-l govuk-!-margin-bottom-0">
              {publicationSummary.summary}
            </p>
          )}
        </div>

        <div className="govuk-grid-column-one-third">
          <h2
            className="govuk-heading-m govuk-!-margin-bottom-2"
            id="quick-links"
            style={{ borderTop: '3px solid #1d70b8', paddingTop: '1rem' }}
          >
            Quick links
          </h2>
          <nav
            role="navigation"
            aria-labelledby="quick-links"
            data-testid="quick-links"
          >
            <ul className="govuk-list dfe-flex dfe-flex-direction--column dfe-gap-2">
              <li>
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
              </li>
              <li>
                <Link
                  to={
                    releaseVersionSummary.isLatestRelease
                      ? `/data-tables/${publicationSummary.slug}`
                      : `/data-tables/${publicationSummary.slug}/${releaseVersionSummary.slug}`
                  }
                >
                  Create your own tables
                </Link>
              </li>
              <li>
                <Link
                  to={`/subscriptions/new-subscription/${publicationSummary.slug}`}
                  onClick={() => {
                    logEvent({
                      category: 'Subscribe',
                      action: 'Email subscription',
                    });
                  }}
                >
                  Get email alerts
                </Link>
              </li>
            </ul>
          </nav>
        </div>
      </div>
    </>
  );
};
export default ReleasePageTitle;
