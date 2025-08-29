import { useMobileMedia } from '@common/hooks/useMedia';
import { ReleaseVersion } from '@common/services/publicationService';
import SubscribeLink from '@frontend/components/SubscribeLink';
import styles from '@frontend/modules/find-statistics/components/ReleasePageTitle.module.scss';
import classNames from 'classnames';
import React from 'react';

interface Props {
  releaseVersion: ReleaseVersion;
}

const ReleasePageTitle = ({ releaseVersion }: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();
  const { publication, title } = releaseVersion;
  return (
    <div
      className={classNames(
        'govuk-!-margin-bottom-8',
        styles.releasePageTitleWrap,
      )}
    >
      <div className={styles.releasePageTitle}>
        <span className="govuk-caption-xl" data-testid="page-title-caption">
          {title}
        </span>
        <h1
          className="govuk-heading-xl govuk-!-margin-bottom-2"
          data-testid="page-title"
        >
          {publication.title}
        </h1>
        <p className="govuk-body-l govuk-!-margin-bottom-0">
          {publication.summary}
        </p>
      </div>
      {!isMobileMedia && (
        <div className="govuk-!-margin-bottom-4">
          <SubscribeLink
            url={`/subscriptions/new-subscription/${releaseVersion.publication.slug}`}
          />
        </div>
      )}
    </div>
  );
};
export default ReleasePageTitle;
