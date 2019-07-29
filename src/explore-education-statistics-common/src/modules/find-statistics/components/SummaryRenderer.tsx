import Details, { DetailsToggleHandler } from '@common/components/Details';
import React from 'react';
import ReactMarkdown from 'react-markdown';
import {
  DataBlockData,
  DataBlockMetadata,
  Result,
} from '@common/services/dataBlockService';
import styles from './SummaryRenderer.module.scss';

export interface SummaryRendererProps {
  data: DataBlockData;
  meta: DataBlockMetadata;
  dataKeys: string[];
  dataSummary: string[];
  description: { type: string; body: string };
  onToggle?: DetailsToggleHandler;
}

function getLatestMeasures(result: Result[]) {
  const copiedResult = [...result];

  copiedResult.sort((a, b) => a.timePeriod.localeCompare(b.timePeriod));

  const latest = copiedResult[copiedResult.length - 1];

  const { measures } = latest;
  return measures;
}

export default function SummaryRenderer({
  meta,
  description,
  data,
  dataKeys,
  dataSummary,
  onToggle,
}: SummaryRendererProps) {
  let measures: { [key: string]: string } = {};

  if (meta === undefined) {
    return <div>Invalid data specified</div>;
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
            <div className={styles.keyStatTile} key={indicatorKey}>
              <div className={styles.keyStat}>
                <h3
                  className="govuk-heading-s"
                  data-testid="key-stat-tile-title"
                >
                  {meta.indicators[key].label}
                </h3>
                <p
                  className="govuk-heading-xl"
                  data-testid="key-stat-tile-value"
                >
                  {`${measures[key]}${meta.indicators[key].unit}`}
                </p>
                {dataSummary && (
                  <p className="govuk-body-s">{dataSummary[index]}</p>
                )}
              </div>
              <Details
                onToggle={onToggle}
                summary={`Define '${meta.indicators[key].label}'`}
              >
                <p>
                  This service is not yet available
                  {/*
                  {`${
                    meta.indicators[key].label
                  } is the adipisicing elit. Dolorum hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.`}
                  */}
                </p>
                {/*
                <a
                  className="govuk-details__summary-text"
                  href={`/glossary#${meta.indicators[key].label}`}
                >
                  More &gt;&gt;&gt;
                </a>
                */}
              </Details>
            </div>
          );
        })}
      </div>
      {description.body !== '' ? (
        <ReactMarkdown source={description.body} />
      ) : (
        ''
      )}
    </>
  );
}
