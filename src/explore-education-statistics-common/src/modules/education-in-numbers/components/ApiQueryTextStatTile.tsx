import styles from '@common/modules/education-in-numbers/components/FreeTextStatTile.module.scss';
import React from 'react';
import { EinApiQueryStatTile } from '@common/services/types/einBlocks';

export interface ApiQueryStatTileProps {
  tile: EinApiQueryStatTile;
  testId?: string;
}

const ApiQueryStatTile = ({
  testId = 'api-query-stat-tile',
  tile,
}: ApiQueryStatTileProps) => {
  const {
    title,
    statistic,
    indicatorUnit,
    decimalPlaces,
    publicationSlug,
    releaseSlug,
  } = tile;

  return (
    <div className={styles.tile} data-testid={`${testId}-tile`}>
      <h4 className="govuk-body-l" data-testid={`${testId}-title`}>
        {title ?? '@MarkFix ApiQueryStatTile title is missing'}
      </h4>
      <p className="govuk-heading-m" data-testid={`${testId}-statistic`}>
        {statistic ?? '@MarkFix missing stat - needs formatting too'}
      </p>
      <a
        href={`ees.blah/${publicationSlug}/${releaseSlug}`}
        data-testid={`${testId}-link`}
        className="govuk-link govuk-!-display-inline-block govuk-!-margin-top-4"
      >
        {`ees.blah/${publicationSlug}/${releaseSlug}`}
      </a>
    </div>
  );
};

export default ApiQueryStatTile;
