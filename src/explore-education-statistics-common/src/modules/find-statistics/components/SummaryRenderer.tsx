/* eslint-disable react/no-danger */
import { DetailsToggleHandler } from '@common/components/Details';
import React from 'react';
import { DataBlockData, Result } from '@common/services/dataBlockService';
import { ChartMetaData } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import styles from './SummaryRenderer.module.scss';
import KeyStatTile from './KeyStatTile';

export interface SummaryRendererProps {
  data: DataBlockData;
  meta: ChartMetaData;
  dataKeys: string[];
  dataSummary: string[];
  dataDefinition: string[];
  // description: { type: string; body: string };
  onToggle?: DetailsToggleHandler;
}

export function getLatestMeasures(result: Result[]) {
  const copiedResult = [...result];

  copiedResult.sort((a, b) => a.timePeriod.localeCompare(b.timePeriod));

  const latest = copiedResult[copiedResult.length - 1];

  const { measures } = latest;
  return measures;
}

export default function SummaryRenderer({
  meta,
  // description,
  data,
  dataKeys,
  dataSummary,
  dataDefinition,
  onToggle,
}: SummaryRendererProps) {
  let measures: { [key: string]: string } = {};

  if (meta === undefined) {
    return <div>Unable to render summary, invalid data configured</div>;
  }

  if (data) {
    measures = getLatestMeasures(data.result);
  } else {
    dataKeys.forEach(key => {
      measures[key] = '';
    });
  }

  return (
    <>
      <div className={styles.keyStatsContainer}>
        {dataKeys.map((key, index) => {
          const indicatorKey = `${key}_${index}`;

          return (
            <KeyStatTile
              key={indicatorKey}
              meta={meta}
              measures={measures}
              indicatorKey={key}
              onToggle={onToggle}
              dataDefinition={dataDefinition[index]}
              dataSummary={dataSummary[index]}
            />
          );
        })}
      </div>
      {/* description && description.body !== '' && (
        <ReactMarkdown source={description.body} />
      ) */}
    </>
  );
}
