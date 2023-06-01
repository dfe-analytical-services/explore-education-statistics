import ChevronCard from '@common/components/ChevronCard';
import ChevronGrid from '@common/components/ChevronGrid';
import ButtonText from '@common/components/ButtonText';
import { FeaturedTable } from '@common/services/tableBuilderService';
import useToggle from '@common/hooks/useToggle';
import FormSearchBar from '@common/components/form/FormSearchBar';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useMobileMedia } from '@common/hooks/useMedia';
import React, { ReactNode, useState } from 'react';

interface Props {
  featuredTables?: FeaturedTable[];
  renderFeaturedTableLink?: (featuredTable: FeaturedTable) => ReactNode;
}

export default function AllFeaturedTables({
  featuredTables = [],
  renderFeaturedTableLink,
}: Props) {
  const { isMedia: isMobileMedia } = useMobileMedia();
  const [listView, toggleListView] = useToggle(false);
  const [filteredFeaturedTables, setFilteredFeaturedTables] = useState<
    FeaturedTable[]
  >(featuredTables);

  const handleSearch = (searchTerm: string) => {
    const lowerCaseSearchTerm = searchTerm.toLowerCase();
    const filtered = featuredTables.filter(
      table =>
        table.name.toLowerCase().includes(lowerCaseSearchTerm) ||
        table.description?.toLowerCase().includes(lowerCaseSearchTerm),
    );
    setFilteredFeaturedTables(filtered);
  };

  return (
    <>
      <h3>All featured tables for this publication</h3>

      <p>
        View featured tables from across all data sets for this publication. If
        you can't find what you are looking for please select a specific data
        set, and then you can create your own table.
      </p>

      <FormSearchBar
        className="govuk-!-margin-bottom-5"
        id="featuredTables-search"
        label="Search featured tables"
        labelSize="s"
        name="search"
        onReset={() => setFilteredFeaturedTables(featuredTables)}
        onSubmit={handleSearch}
      />

      {filteredFeaturedTables.length > 0 ? (
        <>
          <VisuallyHidden>
            <p aria-live="polite" aria-atomic>
              {`Showing ${filteredFeaturedTables.length} featured table${
                filteredFeaturedTables.length > 1 ? 's' : ''
              }`}
            </p>
          </VisuallyHidden>

          {!isMobileMedia && (
            <>
              {listView ? (
                <p>
                  Showing list view.{' '}
                  <ButtonText onClick={toggleListView.off}>
                    View as grid
                  </ButtonText>
                </p>
              ) : (
                <p>
                  Showing grid view.{' '}
                  <ButtonText onClick={toggleListView.on}>
                    View as list
                  </ButtonText>
                </p>
              )}
            </>
          )}
          <ChevronGrid testId="featuredTables">
            {filteredFeaturedTables.map(table => {
              return (
                <ChevronCard
                  as="li"
                  cardSize={listView ? 'l' : 's'}
                  description={table.description ?? ''}
                  key={table.id}
                  link={renderFeaturedTableLink?.(table)}
                  noBorder
                />
              );
            })}
          </ChevronGrid>
        </>
      ) : (
        <p>No featured tables found.</p>
      )}
    </>
  );
}
