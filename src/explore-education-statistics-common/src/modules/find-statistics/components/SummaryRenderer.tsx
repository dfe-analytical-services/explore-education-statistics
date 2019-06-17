import Details from '@common/components/Details';
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
  dataSummary?: string[];
  description: { type: string; body: string };
}

function getLatestMeasures(result: Result[]) {
  const copiedResult = [...result];

  copiedResult.sort((a, b) => {
    if (a.year < b.year) {
      return -1;
    }

    if (a.year > b.year) {
      return 1;
    }

    return 0;
  });

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
}: SummaryRendererProps) {
  let measures: { [key: string]: string } = {};

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
                <h3 className="govuk-heading-s">
                  {meta.indicators[key].label}
                </h3>
                <p
                  className="govuk-heading-xl"
                  data-testid={`tile ${meta.indicators[key].label}`}
                >
                  {measures[key]}
                  {meta.indicators[key].unit}
                </p>
                {dataSummary && (
                  <p className="govuk-body-s">{dataSummary[index]}</p>
                )}
              </div>
              <Details summary={`What is ${meta.indicators[key].label}?`}>
                Overall absence is the adipisicing elit. Dolorum hic nobis
                voluptas quidem fugiat enim ipsa reprehenderit nulla.
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
