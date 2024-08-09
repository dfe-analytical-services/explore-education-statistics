import { LocationCandidateWithKey } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import { FilterCandidateWithKey } from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import ApiDataSetLocationCode from '@admin/pages/release/data/components/ApiDataSetLocationCode';
import Tag from '@common/components/Tag';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';
import React from 'react';

interface Props {
  groupKey: LocationLevelKey | string;
  groupLabel: string;
  label: string;
  newItems: FilterCandidateWithKey[] | LocationCandidateWithKey[];
}

export default function ApiDataSetNewItemsTable({
  groupKey,
  groupLabel,
  label,
  newItems,
}: Props) {
  return (
    <table data-testid={`new-items-table-${groupKey}`}>
      <caption className="govuk-visually-hidden">
        {`Table showing new ${label}s for ${groupLabel}`}
      </caption>
      <thead>
        <tr>
          <th className="govuk-!-width-one-third">Current data set</th>
          <th className="govuk-!-width-one-third">New data set</th>
          <th>Type</th>
        </tr>
      </thead>
      <tbody>
        {newItems.map(candidate => {
          return (
            <tr key={`candidate-${candidate.key}`}>
              <td>Not applicable</td>
              <td>
                {candidate.label}
                <ApiDataSetLocationCode location={candidate} />
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
