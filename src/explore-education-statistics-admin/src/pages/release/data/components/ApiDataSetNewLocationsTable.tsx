import { NewLocationCandidate } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import locationLevelsMap from '@common/utils/locationLevelsMap';
import React from 'react';
import camelCase from 'lodash/camelCase';

interface Props {
  level: string;
  locations: NewLocationCandidate[];
}

export default function ApiDataSetNewLocationsTable({
  level,
  locations,
}: Props) {
  return (
    <table data-testid={`new-${level}`}>
      <caption className="govuk-visually-hidden">
        {`Table showing new locations for ${
          locationLevelsMap[camelCase(level)]?.plural ?? level
        }`}
      </caption>
      <thead>
        <tr>
          <th className="govuk-!-width-one-third">Current data set</th>
          <th className="govuk-!-width-one-third">New data set</th>
        </tr>
      </thead>
      <tbody>
        {locations.map(({ candidate }) => {
          return (
            <tr key={`candidate-${candidate.code}`}>
              <td>Not applicable to current data set</td>
              <td>
                {candidate.label}
                <br />
                {candidate.code}
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}
