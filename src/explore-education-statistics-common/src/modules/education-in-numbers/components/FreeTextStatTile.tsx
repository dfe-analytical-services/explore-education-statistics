import { EinFreeTextStatTile } from '@common/services/types/einBlocks';
import styles from '@common/modules/education-in-numbers/components/FreeTextStatTile.module.scss';
import React from 'react';

export interface FreeTextStatTileProps {
  tile: EinFreeTextStatTile;
  testId?: string;
}

const FreeTextStatTile = ({
  testId = 'free-text-stat-tile',
  tile,
}: FreeTextStatTileProps) => {
  const { title, trend, statistic, linkUrl, linkText } = tile;

  return (
    <div className={styles.tile} data-testid={`${testId}-tile`}>
      <h4 className="govuk-body-l" data-testid={`${testId}-title`}>
        {title || 'No title available'}
      </h4>
      <p className="govuk-heading-m" data-testid={`${testId}-statistic`}>
        {statistic || 'No statistic available'}
      </p>
      <p className="govuk-body" data-testid={`${testId}-trend`}>
        {trend}
      </p>

      {linkText && linkUrl && (
        <div className={styles.linkContainer}>
          <a
            href={linkUrl}
            data-testid={`${testId}-link`}
            className="govuk-link govuk-!-display-inline-block govuk-!-margin-top-4"
          >
            {linkText}
          </a>
        </div>
      )}
    </div>
  );
};

export default FreeTextStatTile;
