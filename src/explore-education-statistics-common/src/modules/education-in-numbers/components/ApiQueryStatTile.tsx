import Tag from '@common/components/Tag';
import styles from '@common/modules/education-in-numbers/components/ApiQueryStatTile.module.scss';
import formatPretty from '@common/utils/number/formatPretty';
import classNames from 'classnames';
import React from 'react';
import { EinApiQueryStatTile } from '@common/services/types/einBlocks';

export interface ApiQueryStatTileProps {
  tile: EinApiQueryStatTile;
  /**
   * Must not end with a slash. Both apps normalise this
   * where they load their config.
   */
  publicAppUrl: string;
  testId?: string;
}

const ApiQueryStatTile = ({
  testId = 'api-query-stat-tile',
  tile,
  publicAppUrl,
}: ApiQueryStatTileProps) => {
  const {
    title,
    statistic,
    indicatorUnit,
    decimalPlaces,
    isLatestVersion,
    publicationSlug,
    releaseSlug,
    publicationLabel,
    releaseLabel,
  } = tile;

  const releaseUrl = `${publicAppUrl}/find-statistics/${publicationSlug}/${releaseSlug}`;

  return (
    <div className={styles.tile} data-testid={`${testId}-tile`}>
      <h4
        className={classNames('govuk-body-l', styles.title)}
        data-testid={`${testId}-title`}
      >
        {title ?? 'No title available'}
      </h4>
      <p
        className={classNames('govuk-heading-m', styles.statistic)}
        data-testid={`${testId}-statistic`}
      >
        {statistic
          ? formatPretty(statistic, indicatorUnit, decimalPlaces)
          : 'No statistic available'}
      </p>
      {!isLatestVersion && (
        <Tag
          className={styles.notLatestTag}
          colour="orange"
          testId={`${testId}-not-latest-tag`}
        >
          Not the latest data
        </Tag>
      )}
      {publicationSlug && releaseSlug && publicationLabel && releaseLabel && (
        <div className={styles.meta}>
          <span
            className={styles.release}
            data-testid={`${testId}-link-release`}
          >
            {releaseLabel}
          </span>
          <a
            href={releaseUrl}
            data-testid={`${testId}-link`}
            className={classNames('govuk-link', styles.publication)}
          >
            {publicationLabel}
          </a>
        </div>
      )}
    </div>
  );
};

export default ApiQueryStatTile;
