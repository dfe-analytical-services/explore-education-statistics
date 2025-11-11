import Link from '@admin/components/Link';
import styles from '@admin/pages/release/content/components/ReleasePageTitle.module.scss';
import SubscribeIcon from '@common/components/SubscribeIcon';
import { useMobileMedia } from '@common/hooks/useMedia';
import React from 'react';

interface Props {
  publicationSummary: string;
  publicationTitle: string;
  releaseTitle: string;
}

const ReleasePageTitle = ({
  publicationSummary,
  publicationTitle,
  releaseTitle,
}: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <div className={styles.releasePageTitleWrap}>
      <div className={styles.releasePageTitle}>
        <span className="govuk-caption-xl" data-testid="page-title-caption">
          {releaseTitle}
        </span>
        <h2
          className="govuk-heading-xl govuk-!-margin-bottom-2"
          data-testid="page-title"
        >
          {publicationTitle}
        </h2>
        {!isMobileMedia && (
          <p className="govuk-body-l govuk-!-margin-bottom-0">
            {publicationSummary}
          </p>
        )}
      </div>
      {!isMobileMedia && (
        <div className="govuk-!-margin-bottom-4">
          <Link className={styles.link} to="#" unvisited>
            <SubscribeIcon className={styles.icon} />
            Get email alerts
          </Link>
        </div>
      )}
    </div>
  );
};
export default ReleasePageTitle;
