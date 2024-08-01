import { FilterCandidate } from '@admin/services/apiDataSetVersionService';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { Dictionary } from '@common/types';
import CollapsibleList from '@common/components/CollapsibleList';
import React from 'react';

interface Props {
  newFilterColumns: Dictionary<FilterCandidate>;
}

export default function ApiDataSetNewFilterColumnsTable({
  newFilterColumns,
}: Props) {
  return (
    <table id="new-filter-columns-table" data-testid="new-filter-columns-table">
      <caption className="govuk-!-margin-bottom-3 govuk-!-font-size-24">
        <VisuallyHidden>
          Table showing filter columns not found in the new data set
        </VisuallyHidden>
      </caption>
      <thead>
        <tr>
          <th className="govuk-!-width-one-third">Current data set</th>
          <th className="govuk-!-width-one-third">New data set</th>
          <th>Type</th>
        </tr>
      </thead>
      <tbody>
        {Object.entries(newFilterColumns).map(([key, column]) => {
          return (
            <tr key={`column-${key}`}>
              <td>No mapping available</td>
              <td>
                {column.label} <br />
                id: {key} <br />
                {column.options && Object.keys(column.options).length > 0 && (
                  <CollapsibleList
                    buttonClassName="govuk-!-margin-bottom-0"
                    collapseAfter={0}
                    id={`filter-column-options-${key}`}
                    itemName="filter option"
                    itemNamePlural="filter options"
                    listClassName="govuk-!-margin-bottom-0 govuk-!-margin-top-2"
                  >
                    {Object.entries(column.options).map(
                      ([optionKey, option]) => {
                        return <li key={optionKey}>{option.label}</li>;
                      },
                    )}
                  </CollapsibleList>
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
