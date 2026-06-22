import {
  PublicationSummary,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import React from 'react';
import Link from '@frontend/components/Link';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { useMobileMedia } from '@common/hooks/useMedia';
import styles from '@frontend/modules/find-statistics/components/ReleasePageTitleQuickLinks.module.scss';

interface Props {
  publicationSummary: PublicationSummary;
  releaseVersionSummary: ReleaseVersionSummary;
  showSubscriptionLink?: boolean;
}

const ReleasePageTitleQuickLinks = ({
  publicationSummary,
  releaseVersionSummary,
  showSubscriptionLink = true,
}: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();
  return (
    <div className={isMobileMedia ? '' : 'govuk-grid-column-one-third'}>
      <h2 className={styles.quickLinksHeading} id="quick-links">
        Quick links
      </h2>
      <nav
        role="navigation"
        aria-labelledby="quick-links"
        data-testid="quick-links"
      >
        <ul className="govuk-list dfe-flex dfe-flex-direction--column dfe-gap-2 govuk-!-margin-0">
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
          {showSubscriptionLink && (
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
          )}
        </ul>
      </nav>
    </div>
  );
};

export default ReleasePageTitleQuickLinks;
