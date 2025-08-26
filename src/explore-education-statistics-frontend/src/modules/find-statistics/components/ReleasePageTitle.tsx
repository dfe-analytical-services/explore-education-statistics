import { ReleaseVersion } from '@common/services/publicationService';
import styles from '@frontend/modules/find-statistics/components/ReleasePageTitle.module.scss';
import React from 'react';
import classNames from 'classnames';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { useMobileMedia } from '@common/hooks/useMedia';

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
          <a
            className={classNames(
              'govuk-!-display-none-print',
              styles.subscribeLink,
            )}
            href={`/subscriptions/new-subscription/${releaseVersion.publication.slug}`}
            data-testid={`subscription-${releaseVersion.publication.slug}`}
            onClick={() => {
              logEvent({
                category: 'Subscribe',
                action: 'Email subscription',
              });
            }}
          >
            <svg
              aria-hidden="true"
              xmlns="http://www.w3.org/2000/svg"
              height="18"
              width="18"
              viewBox="0 0 459.334 459.334"
            >
              <path d="M177.216 404.514c-.001.12-.009.239-.009.359 0 30.078 24.383 54.461 54.461 54.461s54.461-24.383 54.461-54.461c0-.12-.008-.239-.009-.359H175.216zM403.549 336.438l-49.015-72.002v-89.83c0-60.581-43.144-111.079-100.381-122.459V24.485C254.152 10.963 243.19 0 229.667 0s-24.485 10.963-24.485 24.485v27.663c-57.237 11.381-100.381 61.879-100.381 122.459v89.83l-49.015 72.002a24.76 24.76 0 0 0 20.468 38.693H383.08a24.761 24.761 0 0 0 20.469-38.694z" />
            </svg>
            Get email alerts
          </a>
        </div>
      )}
    </div>
  );
};
export default ReleasePageTitle;
