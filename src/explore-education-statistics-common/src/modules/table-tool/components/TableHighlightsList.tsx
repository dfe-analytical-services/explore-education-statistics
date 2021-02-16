import { FormTextSearchInput } from '@common/components/form';
import { TableHighlight } from '@common/services/tableBuilderService';
import React, { ReactNode, useState } from 'react';

interface Props {
  highlights: TableHighlight[];
  renderLink: (highlight: TableHighlight) => ReactNode;
}

const TableHighlightsList = ({ highlights = [], renderLink }: Props) => {
  const [highlightSearch, setHighlightSearch] = useState('');

  const filteredHighlights = highlights
    .filter(
      highlight =>
        highlight.name.toLowerCase().includes(highlightSearch) ||
        highlight.description.toLowerCase().includes(highlightSearch),
    )
    .sort();

  if (!highlights.length) {
    return (
      <p className="govuk-!-font-weight-bold">No popular tables available.</p>
    );
  }

  return (
    <>
      <form role="search" className="govuk-!-margin-bottom-6">
        <FormTextSearchInput
          id="tableHighlightsList-search"
          name="search"
          label="Search popular tables"
          className="govuk-!-width-one-third"
          onChange={event => {
            setHighlightSearch(event.target.value.toLowerCase());
          }}
        />
      </form>

      <div aria-live="polite">
        {highlightSearch && (
          <p className="govuk-!-font-weight-bold">
            {`Found ${filteredHighlights.length} matching ${
              filteredHighlights.length === 1 ? 'table' : 'tables'
            }`}
          </p>
        )}

        {filteredHighlights.length > 0 && (
          <ul className="govuk-!-margin-bottom-6">
            {filteredHighlights.map(highlight => (
              <li key={highlight.id}>
                <p className="govuk-!-font-weight-bold govuk-!-margin-bottom-1">
                  {renderLink(highlight)}
                </p>
                <p>{highlight.description}</p>
              </li>
            ))}
          </ul>
        )}
      </div>
    </>
  );
};

export default TableHighlightsList;
