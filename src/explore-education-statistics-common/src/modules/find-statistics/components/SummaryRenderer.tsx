import Details from '@common/components/Details';
import {
  CharacteristicsData,
  PublicationMeta,
} from '@common/services/tableBuilderService';
import React from 'react';
import ReactMarkdown from 'react-markdown';

export interface SummaryRendererProps {
  data: CharacteristicsData;
  meta: PublicationMeta;
  dataKeys: string[];
  description: { type: string; body: string };
}

export default function SummaryRenderer({
  meta,
  description,
  data,
  dataKeys,
}: SummaryRendererProps) {
  let indicators: { [key: string]: string } = {};
  let indicatorMeta: { [key: string]: { label: string; unit: string } } = {};

  if (data) {
    const result = [...data.result];

    result.sort((a, b) => {
      if (a.timePeriod < b.timePeriod) {
        return -1;
      }

      if (a.timePeriod > b.timePeriod) {
        return 1;
      }

      return 0;
    });

    const latest = result[result.length - 1];

    // eslint-disable-next-line prefer-destructuring
    indicators = latest.indicators;
    indicatorMeta = Array.prototype
      .concat(...Object.values(meta.indicators))
      .reduce((allMeta, next) => ({ ...allMeta, [next.name]: next }), {});
  } else {
    dataKeys.forEach(key => {
      indicators[key] = '';
      indicatorMeta[key] = { label: key, unit: '' };
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
                {indicatorMeta[key].label}
              </h3>
              <p
                className="govuk-heading-xl govuk-!-margin-bottom-2"
                data-testid={`tile ${indicatorMeta[key].label}`}
              >
                {indicators[key]}
                {indicatorMeta[key].unit}
              </p>
              <Details summary={`What is ${indicatorMeta[key].label}?`}>
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
