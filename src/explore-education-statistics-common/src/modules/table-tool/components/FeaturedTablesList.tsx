import { FormTextSearchInput } from '@common/components/form';
import { FeaturedTable } from '@common/services/tableBuilderService';
import naturalOrderBy from '@common/utils/array/naturalOrderBy';
import React, { cloneElement, ReactElement, ReactNode, useState } from 'react';

interface Props {
  featuredTables: FeaturedTable[];
  renderLink: (highlight: FeaturedTable) => ReactNode;
}

const FeaturedTablesList = ({ featuredTables = [], renderLink }: Props) => {
  const [highlightSearch, setHighlightSearch] = useState('');

  const filteredTables = naturalOrderBy(
    featuredTables.filter(
      highlight =>
        highlight.name.toLowerCase().includes(highlightSearch) ||
        highlight.description?.toLowerCase().includes(highlightSearch),
    ),
    'name',
  );

  if (!featuredTables.length) {
    return (
      <p className="govuk-!-font-weight-bold">No featured tables available.</p>
    );
  }

  return (
    <>
      <form role="search" className="govuk-!-margin-bottom-6">
        <FormTextSearchInput
          id="tableHighlightsList-search"
          name="search"
          label="Search featured tables"
          className="govuk-!-width-one-third"
          onChange={event => {
            setHighlightSearch(event.target.value.toLowerCase());
          }}
        />
      </form>

      <div aria-live="polite">
        {highlightSearch && (
          <p className="govuk-!-font-weight-bold">
            {`Found ${filteredTables.length} matching ${
              filteredTables.length === 1 ? 'table' : 'tables'
            }`}
          </p>
        )}

        {filteredTables.length > 0 && (
          <ul className="govuk-!-margin-bottom-6" id="featuredTables">
            {filteredTables.map(highlight => {
              const link = renderLink(highlight);
              const descriptionId = `highlight-description-${highlight.id}`;

              return (
                <li key={highlight.id}>
                  <p className="govuk-!-font-weight-bold govuk-!-margin-bottom-1">
                    {link
                      ? cloneElement(link as ReactElement, {
                          'aria-describedby': highlight.description
                            ? descriptionId
                            : undefined,
                        })
                      : null}
                  </p>

                  {highlight.description && (
                    <p id={descriptionId}>{highlight.description}</p>
                  )}
                </li>
              );
            })}
          </ul>
        )}
      </div>
    </>
  );
};

export default FeaturedTablesList;
