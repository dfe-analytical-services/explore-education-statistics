import { useMobileMedia } from '@common/hooks/useMedia';
import { PublicationSummaryRedesign } from '@common/services/publicationService';
import SubscribeLink from '@frontend/components/SubscribeLink';
import styles from '@frontend/modules/find-statistics/components/ReleasePageTitle.module.scss';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React from 'react';

interface Props {
  publicationSummary: PublicationSummaryRedesign;
  releaseTitle: string;
}

const ReleasePageTitle = ({ publicationSummary, releaseTitle }: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
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
      {!isMobileMedia && (
        <div className="govuk-!-margin-bottom-4 dfe-flex-shrink--0">
          <SubscribeLink
            url={`/subscriptions/new-subscription/${publicationSummary.slug}`}
            onClick={() => {
              logEvent({
                category: 'Subscribe',
                action: 'Email subscription',
              });
            }}
          />
        </div>
      )}
    </div>
  );
};
export default ReleasePageTitle;
