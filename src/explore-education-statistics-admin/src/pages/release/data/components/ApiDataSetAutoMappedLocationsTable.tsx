import {
  AutoMappedLocation,
  LocationCandidateWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import styles from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage.module.scss';
import { PendingLocationMappingUpdate } from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage';
import ApiDataSetLocationMappingModal from '@admin/pages/release/data/components/ApiDataSetLocationMappingModal';
import getApiDataSetLocationCodes from '@admin/pages/release/data/utils/getApiDataSetLocationCodes';
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

const locationsPerPage = 10;
const formId = 'auto-mapped-search';

interface Props {
  level: LocationLevelKey;
  locations: AutoMappedLocation[];
  newLocations?: LocationCandidateWithKey[];
  pendingUpdates?: PendingLocationMappingUpdate[];
  onUpdate: (update: PendingLocationMappingUpdate) => Promise<void>;
}

export default function ApiDataSetAutoMappedLocationsTable({
  level,
  locations,
  newLocations = [],
  pendingUpdates = [],
  onUpdate,
}: Props) {
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [searchTerm, setSearchTerm] = useState<string>();

  const filteredLocations = useMemo(() => {
    if (searchTerm) {
      const lowerCaseSearchTerm = searchTerm.toLowerCase();

      return locations.filter(location => {
        const { candidate, mapping } = location;

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

    return locations;
  }, [locations, searchTerm]);

  const filteredLocationChunks = useMemo(
    () => chunk(filteredLocations, locationsPerPage),
    [filteredLocations],
  );

  const totalFilteredLocations = filteredLocations.length;
  const totalPages = filteredLocationChunks.length;

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
            {locations.length > locationsPerPage && (
              <FormSearchBar
                id={`${formId}-search`}
                label="Search auto mapped locations"
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
              {`${totalFilteredLocations} result${
                totalFilteredLocations > 1 ? 's' : ''
              }, showing page ${currentPage} of ${totalPages}`}
            </p>
          </VisuallyHidden>
        </div>
      </div>
      {filteredLocationChunks.length > 0 ? (
        <table
          className={styles.table}
          data-testid={`auto-mapped-table-${level}`}
        >
          <caption className="govuk-visually-hidden">
            Table showing all auto mapped locations
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
            {filteredLocationChunks[currentPage - 1].map(
              ({ candidate, mapping }) => {
                const isPendingUpdate = !!pendingUpdates.find(
                  update => update.sourceKey === mapping.sourceKey,
                );
                return (
                  <tr key={`mapping-${mapping.sourceKey}`}>
                    <td>
                      {mapping.source.label}
                      <br />
                      <span className={styles.code}>
                        {getApiDataSetLocationCodes(mapping.source)}
                      </span>
                    </td>
                    <td>
                      {candidate.label}
                      <br />
                      <span className={styles.code}>
                        {getApiDataSetLocationCodes(candidate)}
                      </span>
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
                          text="Updating location mapping"
                        />
                      ) : (
                        <ApiDataSetLocationMappingModal
                          candidate={candidate}
                          level={level}
                          mapping={mapping}
                          newLocations={newLocations}
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
