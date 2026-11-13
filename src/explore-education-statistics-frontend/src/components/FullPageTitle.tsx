// Page title used on release, table tool search, data set pages
// with a summary that shows above mobile
import { useMobileMedia } from '@common/hooks/useMedia';
import styles from '@frontend/components/FullPageTitle.module.scss';
import React from 'react';

interface Props {
  caption: string;
  summary: string;
  title: string;
}

const FullPageTitle = ({ caption, summary, title }: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <div className={styles.dataSetPageTitle}>
      <span className="govuk-caption-xl" data-testid="page-title-caption">
        {caption}
      </span>
      <h1
        className="govuk-heading-xl govuk-!-margin-bottom-2"
        data-testid="page-title"
      >
        {title}
      </h1>
      {!isMobileMedia && (
        <p className="govuk-body-l govuk-!-margin-bottom-0">{summary}</p>
      )}
    </div>
  );
};

export default FullPageTitle;
