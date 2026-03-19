import { CandidateWithKey } from '@admin/pages/release/data/utils/mappingTypes';
import Tag from '@common/components/Tag';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';
import React, { ReactNode } from 'react';

interface Props {
  groupKey?: LocationLevelKey | string;
  groupLabel?: string;
  itemPluralLabel: string;
  newItems: CandidateWithKey[];
  renderItem: (item: CandidateWithKey) => ReactNode;
}

export default function ApiDataSetNewItemsTable({
  groupKey,
  groupLabel,
  itemPluralLabel,
  newItems,
  renderItem,
}: Props) {
  return (
    <div className="container-with-horizontal-scroll">
      <table data-testid={`new-items-table-${groupKey}`}>
      <caption className="govuk-visually-hidden">
        {`Table showing new ${itemPluralLabel}${
          groupLabel ? ` for ${groupLabel}` : ''
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
          {newItems.map(item => {
            return (
              <tr key={item.key}>
                <td>Not applicable</td>
                <td>{renderItem(item)}</td>
                <td>
                  <Tag colour="grey">Minor</Tag>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
