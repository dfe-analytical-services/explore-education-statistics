import Tag from '@common/components/Tag';
import styles from '@common/modules/education-in-numbers/components/ApiQueryStatTile.module.scss';
import formatPretty from '@common/utils/number/formatPretty';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { EinApiQueryStatTile } from '@common/services/types/einBlocks';

interface LinkRenderProps {
  children: ReactNode;
  className: string;
  testId: string;
  to: string;
}

export interface ApiQueryStatTileProps {
  tile: EinApiQueryStatTile;
  renderLink: (props: LinkRenderProps) => ReactNode;
  testId?: string;
}

const ApiQueryStatTile = ({
  testId = 'api-query-stat-tile',
  tile,
  renderLink,
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

  return (
    <div className={styles.tile} data-testid={`${testId}-tile`}>
      <h4
        className={classNames('govuk-body-l', styles.title)}
        data-testid={`${testId}-title`}
      >
        {title || 'No title available'}
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
          {renderLink({
            children: publicationLabel,
            className: styles.publication,
            testId: `${testId}-link`,
            to: `/find-statistics/${publicationSlug}/${releaseSlug}`,
          })}
        </div>
      )}
    </div>
  );
};

export default ApiQueryStatTile;
