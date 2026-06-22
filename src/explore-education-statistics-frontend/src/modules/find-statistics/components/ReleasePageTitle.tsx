import { useMobileMedia } from '@common/hooks/useMedia';
import { PublicationSummary } from '@common/services/publicationService';
import styles from '@frontend/modules/find-statistics/components/ReleasePageTitle.module.scss';
import React from 'react';

interface Props {
  publicationSummary: PublicationSummary;
  releaseTitle: string;
}

const ReleasePageTitle = ({ publicationSummary, releaseTitle }: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
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
  );
};

export default ReleasePageTitle;
