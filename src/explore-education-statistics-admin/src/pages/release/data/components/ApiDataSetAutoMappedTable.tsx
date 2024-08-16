import {
  AutoMappedLocation,
  LocationCandidateWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import {
  AutoMappedFilterOption,
  FilterOptionCandidateWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import ApiDataSetMappingModal from '@admin/pages/release/data/components/ApiDataSetMappingModal';
import { PendingMappingUpdate } from '@admin/pages/release/data/types/apiDataSetMappings';
import {
  FilterOptionSource,
  LocationCandidate,
} from '@admin/services/apiDataSetVersionService';
import Tag from '@common/components/Tag';
import ButtonText from '@common/components/ButtonText';
import Pagination from '@common/components/Pagination';
import LoadingSpinner from '@common/components/LoadingSpinner';
import FormSearchBar from '@common/components/form/FormSearchBar';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';
import chunk from 'lodash/chunk';
import React, { ReactNode, useMemo, useState } from 'react';

const itemsPerPage = 10;
const formId = 'auto-mapped-search';

interface Props {
  autoMappedItems: AutoMappedLocation[] | AutoMappedFilterOption[];
  candidateHint?: (
    candidate: FilterOptionCandidateWithKey | LocationCandidateWithKey,
  ) => string;
  groupKey: LocationLevelKey | string;
  groupLabel: string;
  itemLabel: string;
  newItems?: LocationCandidateWithKey[] | FilterOptionCandidateWithKey[];
  pendingUpdates?: PendingMappingUpdate[];
  renderCandidate: (
    candidate: LocationCandidateWithKey | FilterOptionCandidateWithKey,
  ) => ReactNode;
  renderSource: (source: LocationCandidate | FilterOptionSource) => ReactNode;
  renderSourceDetails?: (
    source: FilterOptionSource | LocationCandidate,
  ) => ReactNode;
  searchFilter: (
    searchTerm: string,
  ) => AutoMappedLocation[] | AutoMappedFilterOption[];
  onUpdate: (update: PendingMappingUpdate) => Promise<void>;
}

export default function ApiDataSetAutoMappedTable({
  autoMappedItems,
  candidateHint,
  groupKey,
  groupLabel,
  itemLabel,
  newItems = [],
  pendingUpdates = [],
  renderCandidate,
  renderSource,
  renderSourceDetails,
  searchFilter,
  onUpdate,
}: Props) {
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [searchTerm, setSearchTerm] = useState<string>();

  const filteredItems = useMemo(() => {
    if (searchTerm) {
      return searchFilter(searchTerm.toLowerCase());
    }
    return autoMappedItems;
  }, [autoMappedItems, searchFilter, searchTerm]);

  const filteredItemsChunks = useMemo(
    () => chunk(filteredItems, itemsPerPage),
    [filteredItems],
  );

  const totalFilteredItems = filteredItems.length;
  const totalPages = filteredItemsChunks.length;

  const [handleSearch] = useDebouncedCallback((term: string) => {
    setSearchTerm(term);
    setCurrentPage(1);
  }, 800);

  return (
    <>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          <form
            id={formId}
            className="govuk-!-margin-bottom-2"
            onSubmit={e => {
              e.preventDefault();
            }}
          >
            {autoMappedItems.length > itemsPerPage && (
              <FormSearchBar
                id={`${formId}-search`}
                label={
                  <>
                    Search auto mapped options
                    <VisuallyHidden>{`for ${groupLabel}`}</VisuallyHidden>
                  </>
                }
                labelSize="s"
                min={0}
                name="search"
                value={searchTerm}
                onChange={handleSearch}
              />
            )}
          </form>
          <VisuallyHidden>
            <p aria-atomic aria-live="polite">
              {`${totalFilteredItems} result${
                totalFilteredItems > 1 ? 's' : ''
              }, showing page ${currentPage} of ${totalPages}`}
            </p>
          </VisuallyHidden>
        </div>
      </div>
      {filteredItemsChunks.length > 0 ? (
        <table
          className="dfe-table--vertical-align-middl"
          data-testid={`auto-mapped-table-${groupKey}`}
        >
          <caption className="govuk-visually-hidden">
            {`Table showing auto mapped options for ${groupLabel}`}
          </caption>
          <thead>
            <tr>
              <th className="govuk-!-width-one-third">Current data set</th>
              <th className="govuk-!-width-one-third">New data set</th>
              <th>Type</th>
              <th className="govuk-!-text-align-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            {filteredItemsChunks[currentPage - 1].map(
              ({ candidate, mapping }) => {
                const isPendingUpdate = !!pendingUpdates.find(
                  update => update.sourceKey === mapping.sourceKey,
                );
                return (
                  <tr key={`mapping-${mapping.sourceKey}`}>
                    <td>{renderSource(mapping.source)}</td>
                    <td>{renderCandidate(candidate)}</td>
                    <td>
                      <Tag colour="grey">Minor</Tag>
                    </td>
                    <td className="govuk-!-text-align-right">
                      {isPendingUpdate ? (
                        <LoadingSpinner
                          alert
                          hideText
                          inline
                          size="sm"
                          text={`Updating auto-mapping for ${mapping.source.label}`}
                        />
                      ) : (
                        <ApiDataSetMappingModal
                          candidate={candidate}
                          candidateHint={candidateHint}
                          groupKey={groupKey}
                          itemLabel={itemLabel}
                          mapping={mapping}
                          newItems={newItems}
                          renderSourceDetails={renderSourceDetails}
                          onSubmit={onUpdate}
                        />
                      )}
                    </td>
                  </tr>
                );
              },
            )}
          </tbody>
        </table>
      ) : (
        'No results found.'
      )}
      <Pagination
        currentPage={currentPage}
        totalPages={totalPages}
        renderLink={({
          'aria-current': ariaCurrent,
          'aria-label': ariaLabel,
          'data-testid': testId,
          children,
          className,
          onClick,
        }) => (
          <ButtonText
            ariaCurrent={ariaCurrent}
            ariaLabel={ariaLabel}
            className={`govuk-link ${className}`}
            testId={testId}
            onClick={onClick}
          >
            {children}
          </ButtonText>
        )}
        onClick={setCurrentPage}
      />
    </>
  );
}
