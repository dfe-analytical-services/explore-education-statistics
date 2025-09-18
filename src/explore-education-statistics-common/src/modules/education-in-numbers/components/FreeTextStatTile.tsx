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
    <div className={styles.tile}>
      <h4 className="govuk-body-l" data-testid={`${testId}-title`}>
        {title}
      </h4>
      <p className="govuk-heading-m" data-testid={`${testId}-statistic`}>
        {statistic}
      </p>
      <p className="govuk-body" data-testid={`${testId}-trend`}>
        {trend}
      </p>

      {linkText && linkUrl && (
        <a
          href={linkUrl}
          data-testid={`${testId}-link`}
          className="govuk-link govuk-!-display-inline-block govuk-!-margin-top-4"
        >
          {linkText}
        </a>
      )}
    </div>
  );
};

export default FreeTextStatTile;
