import {
  AutoMappedLocation,
  LocationCandidateWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import {
  AutoMappedFilter,
  FilterCandidateWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import ApiDataSetMappingModal from '@admin/pages/release/data/components/ApiDataSetMappingModal';
import ApiDataSetLocationCode from '@admin/pages/release/data/components/ApiDataSetLocationCode';
import { PendingMappingUpdate } from '@admin/services/apiDataSetVersionService';
import Tag from '@common/components/Tag';
import ButtonText from '@common/components/ButtonText';
import Pagination from '@common/components/Pagination';
import LoadingSpinner from '@common/components/LoadingSpinner';
import FormSearchBar from '@common/components/form/FormSearchBar';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';
import chunk from 'lodash/chunk';
import compact from 'lodash/compact';
import React, { useMemo, useState } from 'react';

const itemsPerPage = 10;
const formId = 'auto-mapped-search';

interface Props {
  autoMappedItems: AutoMappedLocation[] | AutoMappedFilter[];
  groupKey: LocationLevelKey | string;
  groupLabel: string;
  label: string;
  newItems?: LocationCandidateWithKey[] | FilterCandidateWithKey[];
  pendingUpdates?: PendingMappingUpdate[];
  onUpdate: (update: PendingMappingUpdate) => Promise<void>;
}

export default function ApiDataSetAutoMappedTable({
  autoMappedItems,
  groupKey,
  groupLabel,
  label,
  newItems = [],
  pendingUpdates = [],
  onUpdate,
}: Props) {
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [searchTerm, setSearchTerm] = useState<string>();

  const filteredItems = useMemo(() => {
    if (searchTerm) {
      const lowerCaseSearchTerm = searchTerm.toLowerCase();
      return autoMappedItems.filter(item => {
        const { candidate, mapping } = item as AutoMappedLocation;

        const searchFields = compact([
          candidate.label,
          candidate.code,
          candidate.laEstab,
          candidate.oldCode,
          candidate.ukprn,
          candidate.urn,
          mapping.source.label,
          mapping.source.code,
          mapping.source.laEstab,
          mapping.source.oldCode,
          mapping.source.ukprn,
          mapping.source.urn,
        ]);

        return searchFields.some(field => {
          return field?.toLowerCase().includes(lowerCaseSearchTerm);
        });
      });
    }
    return autoMappedItems;
  }, [autoMappedItems, searchTerm]);

  const filteredItemsChunks = useMemo(
    () => chunk(filteredItems, itemsPerPage),
    [filteredItems],
  );

  const totalfilteredItems = filteredItems.length;
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
                label={`Search auto mapped ${groupLabel}`}
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
              {`${totalfilteredItems} result${
                totalfilteredItems > 1 ? 's' : ''
              }, showing page ${currentPage} of ${totalPages}`}
            </p>
          </VisuallyHidden>
        </div>
      </div>
      {filteredItemsChunks.length > 0 ? (
        <table
          className="dfe-table-vertical-align-middle"
          data-testid={`auto-mapped-table-${groupKey}`}
        >
          <caption className="govuk-visually-hidden">
            {`Table showing all auto mapped ${groupLabel}s`}
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
                    <td>
                      {mapping.source.label}
                      <ApiDataSetLocationCode location={mapping.source} />
                    </td>
                    <td>
                      {candidate.label}
                      <ApiDataSetLocationCode location={candidate} />
                    </td>
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
                          text={`Updating ${label} mapping`}
                        />
                      ) : (
                        <ApiDataSetMappingModal
                          candidate={candidate}
                          groupKey={groupKey}
                          label={label}
                          mapping={mapping}
                          newItems={newItems}
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
