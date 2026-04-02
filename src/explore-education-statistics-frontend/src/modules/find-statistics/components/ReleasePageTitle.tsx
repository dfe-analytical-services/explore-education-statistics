import { useMobileMedia } from '@common/hooks/useMedia';
import { PublicationSummary } from '@common/services/publicationService';
import { Organisation } from '@common/services/types/organisation';
import styles from '@frontend/modules/find-statistics/components/ReleasePageTitle.module.scss';
import React from 'react';
import PublishingOrganisations from './PublishingOrganisations';

interface Props {
  publicationSummary: PublicationSummary;
  releaseTitle: string;
  publishingOrganisations: Organisation[] | undefined;
}

const ReleasePageTitle = ({
  publicationSummary,
  releaseTitle,
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
      </div>
    </>
  );
};
export default ReleasePageTitle;
