import Details from '@common/components/Details';
import React from 'react';
import ReactMarkdown from 'react-markdown';
import {
  DataBlockData,
  DataBlockMetadata,
  Result,
} from '@common/services/dataBlockService';

export interface SummaryRendererProps {
  data: DataBlockData;
  meta: DataBlockMetadata;
  dataKeys: string[];
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
    <div>
      <div className="dfe-dash-tiles dfe-dash-tiles--3-in-row">
        {dataKeys.map((key, index) => {
          const indicatorKey = `${key}_${index}`;

          return (
            <div className="dfe-dash-tiles__tile" key={indicatorKey}>
              <h3 className="govuk-heading-m dfe-dash-tiles__heading">
                {meta.indicators[key].label}
              </h3>
              <p
                className="govuk-heading-xl govuk-!-margin-bottom-2"
                data-testid={`tile ${meta.indicators[key].label}`}
              >
                {measures[key]}
                {meta.indicators[key].unit}
              </p>
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
    </div>
  );
}
