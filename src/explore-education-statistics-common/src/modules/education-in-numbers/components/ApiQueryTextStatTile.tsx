import styles from '@common/modules/education-in-numbers/components/FreeTextStatTile.module.scss';
import React from 'react';
import { EinApiQueryStatTile } from '@common/services/types/einBlocks';

export interface ApiQueryStatTileProps {
  tile: EinApiQueryStatTile;
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
    publicationSlug,
    releaseSlug,
  } = tile;

  // @MarkFix ask Marv/Rich how to display this
  // publicAppUrl may or may not end with a slash, depending on how it's configured
  const releaseUrl = `${publicAppUrl.replace(
    /\/$/,
    '',
  )}/find-statistics/${publicationSlug}/${releaseSlug}`;

  // @MarkFix statistic needs to be formatted using indicatorUnit/decimalPlaces
  return (
    <div className={styles.tile} data-testid={`${testId}-tile`}>
      <h4 className="govuk-body-l" data-testid={`${testId}-title`}>
        {title ?? 'No title available'}
      </h4>
      <p className="govuk-heading-m" data-testid={`${testId}-statistic`}>
        {statistic ?? 'No statistic available'}
      </p>
      <a
        href={releaseUrl}
        data-testid={`${testId}-link`}
        className="govuk-link govuk-!-display-inline-block govuk-!-margin-top-4"
      >
        {releaseUrl}
      </a>
    </div>
  );
};

export default ApiQueryStatTile;
