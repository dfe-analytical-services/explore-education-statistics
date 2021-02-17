import { FormTextSearchInput } from '@common/components/form';
import { TableHighlight } from '@common/services/tableBuilderService';
import React, { cloneElement, ReactElement, ReactNode, useState } from 'react';

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
          <ul className="govuk-!-margin-bottom-6" id="popularTables">
            {filteredHighlights.map(highlight => {
              const link = renderLink(highlight);
              const descriptionId = `highlight-description-${highlight.id}`;

              return (
                <li key={highlight.id}>
                  <p className="govuk-!-font-weight-bold govuk-!-margin-bottom-1">
                    {link
                      ? cloneElement(link as ReactElement, {
                          'aria-describedby': descriptionId,
                        })
                      : null}
                  </p>

                  <p id={descriptionId}>{highlight.description}</p>
                </li>
              );
            })}
          </ul>
        )}
      </div>
    </>
  );
};

export default TableHighlightsList;
