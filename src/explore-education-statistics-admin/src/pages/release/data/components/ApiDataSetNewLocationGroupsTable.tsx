import ApiDataSetLocationGroupOptionsModal from '@admin/pages/release/data/components/ApiDataSetLocationGroupOptionsModal';
import { LocationCandidate } from '@admin/services/apiDataSetVersionService';
import Tag from '@common/components/Tag';
import locationLevelsMap, {
  LocationLevelKey,
} from '@common/utils/locationLevelsMap';
import React from 'react';

interface Props {
  locationGroups: Partial<Record<LocationLevelKey, LocationCandidate[]>>;
}

export default function ApiDataSetNewLocationGroupsTable({
  locationGroups,
}: Props) {
  return (
    <table
      id="new-location-groups-table"
      data-testid="new-location-groups-table"
    >
      <caption className="govuk-visually-hidden">
        Table showing new location groups added by new data set
      </caption>
      <thead>
        <tr>
          <th className="govuk-!-width-one-third">Current data set</th>
          <th className="govuk-!-width-one-third">New data set</th>
          <th>Type</th>
        </tr>
      </thead>
      <tbody>
        {Object.entries(locationGroups).map(([key, options]) => {
          const levelKey = key as LocationLevelKey;

          return (
            <tr key={levelKey}>
              <td>No mapping available</td>
              <td>
                {locationLevelsMap[levelKey].plural}
                {options.length > 0 && (
                  <>
                    <br />
                    <ApiDataSetLocationGroupOptionsModal
                      level={levelKey}
                      options={options}
                    />
                  </>
                )}
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
