import { FilterCandidate } from '@admin/services/apiDataSetVersionService';
import ApiDataSetMappableFilterColumnOptionsModal from '@admin/pages/release/data/components/ApiDataSetMappableFilterColumnOptionsModal';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { Dictionary } from '@common/types';
import React from 'react';

interface Props {
  newFilters: Dictionary<FilterCandidate>;
}

export default function ApiDataSetNewFilterColumnsTable({ newFilters }: Props) {
  return (
    <table id="new-filter-columns-table" data-testid="new-filter-columns-table">
      <caption className="govuk-visually-hidden">
        Table showing filter columns not found in new data set
      </caption>
      <thead>
        <tr>
          <th className="govuk-!-width-one-third">Current data set</th>
          <th className="govuk-!-width-one-third">New data set</th>
          <th>Type</th>
        </tr>
      </thead>
      <tbody>
        {Object.entries(newFilters).map(([key, filter]) => {
          return (
            <tr key={`column-${key}`}>
              <td>No mapping available</td>
              <td>
                {filter.label}
                <br />
                <VisuallyHidden>Column: </VisuallyHidden>
                <code>{key}</code>
                <br />
                {filter.options && Object.keys(filter.options).length > 0 && (
                  <ApiDataSetMappableFilterColumnOptionsModal
                    column={key}
                    label={filter.label}
                    options={Object.values(filter.options).map(
                      option => option.label,
                    )}
                  />
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
