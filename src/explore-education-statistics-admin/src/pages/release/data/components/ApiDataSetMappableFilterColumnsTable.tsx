import { FilterMapping } from '@admin/services/apiDataSetVersionService';
import ApiDataSetMappableFilterColumnOptionsModal from '@admin/pages/release/data/components/ApiDataSetMappableFilterColumnOptionsModal';
import Tag from '@common/components/Tag';
import { Dictionary } from '@common/types';
import React from 'react';

interface Props {
  mappableFilterColumns: Dictionary<FilterMapping>;
}

export default function ApiDataSetMappableFilterColumnsTable({
  mappableFilterColumns,
}: Props) {
  return (
    <table
      className="dfe-table--row-highlights"
      id="mappable-filter-columns-table"
      data-testid="mappable-filter-columns-table"
    >
      <caption className="govuk-visually-hidden">
        Table showing filter columns not found in the new data set
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
                {column.source.label}
                <br />
                <span className="dfe-colour--dark-grey">
                  ID: <code>{key}</code>
                </span>
                <br />
                <ApiDataSetMappableFilterColumnOptionsModal
                  id={key}
                  label={column.source.label}
                  modalLabel="Current data set filter column"
                  options={Object.values(column.optionMappings).map(
                    option => option.source.label,
                  )}
                />
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
