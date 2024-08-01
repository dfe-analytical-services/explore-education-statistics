import { FilterColumnMapping } from '@admin/services/apiDataSetVersionService';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { Dictionary } from '@common/types';
import CollapsibleList from '@common/components/CollapsibleList';
import React from 'react';

interface Props {
  mappableFilterColumns: Dictionary<FilterColumnMapping>;
}

export default function ApiDataSetMappableFilterColumnsTable({
  mappableFilterColumns,
}: Props) {
  return (
    <table
      className="dfe-has-row-highlights"
      id="mappable-filter-columns-table"
      data-testid="mappable-filter-columns-table"
    >
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
        {Object.entries(mappableFilterColumns).map(([key, column]) => {
          return (
            <tr key={`column-${key}`}>
              <td>
                {column.source.label} <br />
                id: {key}
                <br />{' '}
                <CollapsibleList
                  buttonClassName="govuk-!-margin-bottom-0"
                  collapseAfter={0}
                  id={`filter-column-options-${key}`}
                  itemName="filter option"
                  itemNamePlural="filter options"
                  listClassName="govuk-!-margin-bottom-0 govuk-!-margin-top-2"
                >
                  {Object.entries(column.optionMappings).map(
                    ([optionKey, mapping]) => {
                      return <li key={optionKey}>{mapping.source.label}</li>;
                    },
                  )}
                </CollapsibleList>
              </td>
              <td>No mapping available</td>
              <td>
                <Tag colour="blue">Major</Tag>
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}
