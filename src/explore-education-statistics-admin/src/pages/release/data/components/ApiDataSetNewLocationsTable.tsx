import styles from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage.module.scss';
import { LocationCandidateWithKey } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import getApiDataSetLocationCodes from '@admin/pages/release/data/utils/getApiDataSetLocationCodes';
import Tag from '@common/components/Tag';
import locationLevelsMap, {
  LocationLevelKey,
} from '@common/utils/locationLevelsMap';
import React from 'react';

interface Props {
  level: LocationLevelKey;
  locations: LocationCandidateWithKey[];
}

export default function ApiDataSetNewLocationsTable({
  level,
  locations,
}: Props) {
  return (
    <table data-testid={`new-locations-table-${level}`}>
      <caption className="govuk-visually-hidden">
        {`Table showing new locations for ${
          locationLevelsMap[level]?.plural ?? level
        }`}
      </caption>
      <thead>
        <tr>
          <th className="govuk-!-width-one-third">Current data set</th>
          <th className="govuk-!-width-one-third">New data set</th>
          <th>Type</th>
        </tr>
      </thead>
      <tbody>
        {locations.map(candidate => {
          return (
            <tr key={`candidate-${candidate.key}`}>
              <td>Not applicable</td>
              <td>
                {candidate.label}
                <br />
                <span className={styles.code}>
                  {getApiDataSetLocationCodes(candidate)}
                </span>
              </td>
              <td>
                <Tag colour="grey">Minor</Tag>
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}
