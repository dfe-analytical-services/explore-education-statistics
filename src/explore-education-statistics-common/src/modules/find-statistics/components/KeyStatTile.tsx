import React from 'react';
import Details, { DetailsToggleHandler } from '@common/components/Details';
import formatPretty from '@common/lib/utils/number/formatPretty';
import styles from './SummaryRenderer.module.scss';
import { ChartMetaData } from './charts/ChartFunctions';

interface KeyStatProps {
  meta: ChartMetaData;
  measures: {
    [key: string]: string;
  };
  indicatorKey: string;
  onToggle?: DetailsToggleHandler;
  dataSummary: string;
  dataDefinition: string;
}

const KeyStatTile = ({
  meta,
  measures,
  indicatorKey,
  dataSummary,
  onToggle,
  dataDefinition,
}: KeyStatProps) => {
  return (
    <div className={styles.keyStatTile}>
      <div className={styles.keyStat}>
        <h3 className="govuk-heading-s" data-testid="key-stat-tile-title">
          {meta.indicators[indicatorKey].label}
        </h3>
        <p className="govuk-heading-xl" data-testid="key-stat-tile-value">
          {`${formatPretty(measures[indicatorKey])}${
            meta.indicators[indicatorKey].unit
          }`}
        </p>
        {dataSummary && <p className="govuk-body-s">{dataSummary}</p>}
      </div>
      <Details
        onToggle={onToggle}
        summary={`Define '${meta.indicators[indicatorKey].label}'`}
      >
        <div dangerouslySetInnerHTML={{ __html: dataDefinition }} />
      </Details>
    </div>
  );
};

export default KeyStatTile;
