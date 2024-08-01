import Link from '@admin/components/Link';
import ButtonLink from '@admin/components/ButtonLink';
import getApiDataSetFilterMappings, {
  AutoMappedFilter,
  FilterCandidateWithKey,
  MappableFilter,
} from '@admin/pages/release/data//utils/getApiDataSetFilterMappings';
import ApiDataSetMappableTable from '@admin/pages/release/data/components/ApiDataSetMappableTable';
import ApiDataSetNewItemsTable from '@admin/pages/release/data/components/ApiDataSetNewItemsTable';
import ApiDataSetAutoMappedTable from '@admin/pages/release/data/components/ApiDataSetAutoMappedTable';
import getUnmappedFilterErrors from '@admin/pages/release/data/utils/getUnmappedFilterErrors';
import ApiDataSetMappableFilterColumnsTable from '@admin/pages/release/data/components/ApiDataSetMappableFilterColumnsTable';
import ApiDataSetNewFilterColumnsTable from '@admin/pages/release/data/components/ApiDataSetNewFilterColumnsTable';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import apiDataSetVersionQueries from '@admin/queries/apiDataSetVersionQueries';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import apiDataSetVersionService, {
  FilterCandidate,
  FilterColumnMapping,
  PendingMappingUpdate,
} from '@admin/services/apiDataSetVersionService';
import NotificationBanner from '@common/components/NotificationBanner';
import Tag from '@common/components/Tag';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import PageNav, { NavItem } from '@common/components/PageNav';
import { Dictionary } from '@common/types';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import { useQuery } from '@tanstack/react-query';
import React, { useCallback, useEffect, useState } from 'react';
import { generatePath, useParams } from 'react-router-dom';
import omit from 'lodash/omit';
import { useImmer } from 'use-immer';

export default function ReleaseApiDataSetFiltersMappingPage() {
  const [autoMappedFilterOptions, updateAutoMappedFilterOptions] = useImmer<
    Dictionary<AutoMappedFilter[]>
  >({});
  const [newFilterOptionCandidates, updateNewFilterOptionCandidates] = useImmer<
    Dictionary<FilterCandidateWithKey[]>
  >({});
  const [mappableFilterOptions, updateMappableFilterOptions] = useImmer<
    Dictionary<MappableFilter[]>
  >({});
  const [mappableFilterColumns, updateMappableFilterColumns] = useImmer<
    Dictionary<FilterColumnMapping>
  >({});
  const [newFilterColumnCandidates, updateNewFilterColumnCandidates] = useImmer<
    Dictionary<FilterCandidate>
  >({});
  const [pendingUpdates, setPendingUpdates] = useState<PendingMappingUpdate[]>(
    [],
  );

  const { dataSetId, releaseId, publicationId } =
    useParams<ReleaseDataSetRouteParams>();

  const { data: dataSet, isLoading: isLoadingDataSet } = useQuery(
    apiDataSetQueries.get(dataSetId),
  );

  const { data: filtersMapping, isLoading: isLoadingMapping } = useQuery({
    ...apiDataSetVersionQueries.getFiltersMapping(
      dataSet?.draftVersion?.id ?? '',
    ),
    enabled: !!dataSet?.draftVersion?.id,
  });

  useEffect(() => {
    if (filtersMapping) {
      const {
        autoMappedFilterOptions: initialAutoMappedFilterOptions,
        newFilterOptionCandidates: initialNewFilterOptionCandidates,
        mappableFilterOptions: initialMappableFilterOptions,
        mappableFilterColumns: initialMappableFilterColumns,
        newFilterColumnCandidates: initialNewFilterColumnCandidates,
      } = getApiDataSetFilterMappings(filtersMapping);
      updateAutoMappedFilterOptions(initialAutoMappedFilterOptions);
      updateNewFilterOptionCandidates(initialNewFilterOptionCandidates);
      updateMappableFilterOptions(initialMappableFilterOptions);
      updateMappableFilterColumns(initialMappableFilterColumns);
      updateNewFilterColumnCandidates(initialNewFilterColumnCandidates);
    }
  }, [
    filtersMapping,
    updateAutoMappedFilterOptions,
    updateMappableFilterColumns,
    updateMappableFilterOptions,
    updateNewFilterColumnCandidates,
    updateNewFilterOptionCandidates,
  ]);

  const totalNewFilterOptionCandidates = Object.values(
    newFilterOptionCandidates,
  ).flatMap(key => key).length;

  const totalAutoMappedFilterOptions = Object.values(
    autoMappedFilterOptions,
  ).flatMap(key => key).length;

  const unmappedFilterErrors = filtersMapping
    ? getUnmappedFilterErrors(mappableFilterOptions, filtersMapping)
    : [];

  const navItems: NavItem[] = [
    {
      id: 'mappable-filter-options',
      text: 'Filter options not found in the new data set',
      subNavItems: Object.keys(mappableFilterOptions).map(filterKey => {
        return {
          id: `mappable-${filterKey}`,
          text: filtersMapping?.mappings[filterKey].source.label ?? filterKey,
        };
      }),
    },
    {
      id: 'new-filter-options',
      text: 'New filter options',
      subNavItems: Object.keys(newFilterOptionCandidates).map(filterKey => {
        return {
          id: `new-filter-options-${filterKey}`,
          text: filtersMapping?.candidates[filterKey].label ?? filterKey,
        };
      }),
    },
    {
      id: 'auto-mapped-filter-options',
      text: 'Auto mapped filter options',
      subNavItems: Object.keys(autoMappedFilterOptions).map(filterKey => {
        return {
          id: `auto-mapped-${filterKey}`,
          text: filtersMapping?.mappings[filterKey].source.label ?? filterKey,
        };
      }),
    },
  ];

  const updateMappingState = useCallback(
    (updates: PendingMappingUpdate[]) => {
      // MappableFilterOption - Add or update mapping
      updateMappableFilterOptions(draft => {
        updates.forEach(
          ({ candidateKey, groupKey, previousMapping, sourceKey, type }) => {
            const candidate = candidateKey
              ? newFilterOptionCandidates[groupKey]?.find(
                  filterOption => filterOption.key === candidateKey,
                )
              : undefined;

            if (previousMapping.type === 'AutoMapped') {
              const updated = draft[groupKey] ?? [];

              updated.push({
                candidate,
                mapping: { ...previousMapping, candidateKey, type },
              });

              draft[groupKey] = updated;
            } else {
              const filterOptionToUpdate = draft[groupKey]?.find(
                filterOption => filterOption.mapping.sourceKey === sourceKey,
              );

              if (filterOptionToUpdate) {
                filterOptionToUpdate.candidate = candidate;
                filterOptionToUpdate.mapping = {
                  ...previousMapping,
                  candidateKey,
                  type,
                };
              }
            }
          },
        );
      });

      // NewFilterOptions - remove mapped candidates, add unmapped candidates
      updateNewFilterOptionCandidates(draft => {
        updates.forEach(({ candidateKey, groupKey, previousCandidate }) => {
          if (candidateKey) {
            // Remove candidates that have been mapped
            draft[groupKey]?.forEach((filterOption, index) => {
              if (filterOption.key === candidateKey) {
                draft[groupKey]?.splice(index, 1);
              }
            });
          } else if (previousCandidate) {
            const updated = draft[groupKey] ?? [];
            updated.push(previousCandidate);
            draft[groupKey] = updated;
          }
        });
      });

      // AutoMapped - remove previously AutoMapped ones
      updateAutoMappedFilterOptions(draft => {
        updates.forEach(({ groupKey, sourceKey, previousMapping }) => {
          if (previousMapping.type !== 'AutoMapped') {
            return;
          }

          draft[groupKey]?.forEach((filterOption, index) => {
            if (filterOption.mapping.sourceKey === sourceKey) {
              draft[groupKey]?.splice(index, 1);
            }
          });

          if (!draft[groupKey]?.length) {
            delete draft[groupKey];
          }
        });
      });
    },
    [
      newFilterOptionCandidates,
      updateAutoMappedFilterOptions,
      updateMappableFilterOptions,
      updateNewFilterOptionCandidates,
    ],
  );

  // Batch up and debounce so can set filter options to 'no mapping'
  // in quick succession.
  const [handleUpdate] = useDebouncedCallback(async () => {
    if (!dataSet?.draftVersion?.id || !pendingUpdates.length) {
      return;
    }

    await apiDataSetVersionService.updateFilterOptionsMapping(
      dataSet.draftVersion?.id,
      {
        updates: pendingUpdates.map(update =>
          omit(
            { ...update, filterKey: update.groupKey },
            'previousCandidate',
            'previousMapping',
            'groupKey',
          ),
        ),
      },
    );
    updateMappingState(pendingUpdates);
    setPendingUpdates([]);
  }, 1000);

  const handleUpdateMapping = useCallback(
    async (update: PendingMappingUpdate) => {
      setPendingUpdates(current => [...current, update]);
      handleUpdate();
    },
    [handleUpdate],
  );

  return (
    <>
      <Link
        back
        className="govuk-!-margin-bottom-6"
        to={generatePath<ReleaseDataSetRouteParams>(
          releaseApiDataSetDetailsRoute.path,
          {
            publicationId,
            releaseId,
            dataSetId,
          },
        )}
      >
        Back
      </Link>
      <LoadingSpinner loading={isLoadingDataSet || isLoadingMapping}>
        <div className="govuk-grid-row">
          <PageNav items={navItems} />
          <div className="govuk-grid-column-three-quarters">
            <span className="govuk-caption-l">Map filters</span>
            <h2>{dataSet?.title}</h2>
            {!!unmappedFilterErrors.length && (
              <NotificationBanner title="Action required">
                <ul className="govuk-list">
                  {unmappedFilterErrors.map(error => (
                    <li key={error.id}>
                      <Link to={`#${error.id}`}>
                        <strong>{error.message}</strong>
                      </Link>
                    </li>
                  ))}
                </ul>
              </NotificationBanner>
            )}
            <p>
              Existing filters in the publication data set need to be mapped to
              an equivalent filter in the new data set. We try to automatically
              match filters that appear to be a good fit, but you should
              double-check these choices.
            </p>
            <p>
              Any filters in the new data set that have not been mapped will
              become new filters in the publication data set.
            </p>

            {Object.keys(mappableFilterColumns).length > 0 && (
              <>
                <h3
                  className="govuk-heading-l dfe-flex dfe-align-items--center"
                  id="mappable-filter-columns"
                >
                  Filter columns not found in the new data set{' '}
                  <Tag className="govuk-!-margin-left-2" colour="grey">
                    No action required
                  </Tag>
                </h3>

                <ApiDataSetMappableFilterColumnsTable
                  mappableFilterColumns={mappableFilterColumns}
                />
              </>
            )}

            <h3 className="govuk-heading-l" id="mappable-filter-options">
              Filter options not found in the new data set
            </h3>

            {Object.keys(mappableFilterOptions).length > 0 ? (
              <>
                {Object.keys(mappableFilterOptions).map(filterKey => (
                  <ApiDataSetMappableTable
                    key={`unmapped-${filterKey}`}
                    groupKey={filterKey}
                    label="filter option"
                    groupLabel={
                      filtersMapping?.mappings[filterKey].source.label ?? ''
                    }
                    mappableItems={mappableFilterOptions[filterKey]}
                    newItems={newFilterOptionCandidates[filterKey]}
                    pendingUpdates={pendingUpdates}
                    showColumnName
                    onUpdate={handleUpdateMapping}
                  />
                ))}
              </>
            ) : (
              <p>No filter options not found in the new data set.</p>
            )}

            {Object.keys(newFilterColumnCandidates).length > 0 && (
              <>
                <h3
                  className="govuk-heading-l govuk-!-margin-top-8"
                  id="new-filter-columns"
                >
                  New filter columns <Tag colour="grey">No action required</Tag>
                </h3>
                <ApiDataSetNewFilterColumnsTable
                  newFilterColumns={newFilterColumnCandidates}
                />
              </>
            )}

            <h3
              className="govuk-heading-l govuk-!-margin-top-8"
              id="new-filter-options"
            >
              New filter options ({totalNewFilterOptionCandidates}){' '}
              <Tag colour="grey">No action required</Tag>
            </h3>

            {totalNewFilterOptionCandidates > 0 ? (
              <Accordion
                id="new-filter-options"
                testId="new-filter-options-accordion"
              >
                {Object.keys(newFilterOptionCandidates).map(filterKey => {
                  if (newFilterOptionCandidates[filterKey].length) {
                    const filterLabel =
                      filtersMapping?.candidates[filterKey].label;
                    return (
                      <AccordionSection
                        goToTop={false}
                        heading={`${filterLabel} (${newFilterOptionCandidates[filterKey].length})`}
                        headingTag="h4"
                        id={`new-filter-options-${filterKey}`}
                        key={`new-filter-options-${filterKey}`}
                      >
                        <p>
                          <strong>Column:</strong> {filterKey}
                        </p>
                        <ApiDataSetNewItemsTable
                          groupKey={filterKey}
                          groupLabel={filterLabel ?? filterKey}
                          label="filter option"
                          newItems={newFilterOptionCandidates[filterKey]}
                        />
                      </AccordionSection>
                    );
                  }
                  return null;
                })}
              </Accordion>
            ) : (
              <p>No new filter options.</p>
            )}

            <h3
              className="govuk-heading-l govuk-!-margin-top-8"
              id="auto-mapped-filter-options"
            >
              Auto mapped filter options ({totalAutoMappedFilterOptions}){' '}
              <Tag colour="grey">No action required</Tag>
            </h3>

            {totalAutoMappedFilterOptions > 0 ? (
              <Accordion
                id="auto-mapped"
                showOpenAll={false}
                testId="auto-mapped-accordion"
              >
                {Object.keys(autoMappedFilterOptions).map(filterKey => {
                  if (autoMappedFilterOptions[filterKey]?.length) {
                    const filterLabel =
                      filtersMapping?.mappings[filterKey].source.label;
                    return (
                      <AccordionSection
                        goToTop={false}
                        heading={`${filterLabel} (${autoMappedFilterOptions[filterKey].length})`}
                        headingTag="h4"
                        id={`auto-mapped-${filterKey}`}
                        key={`auto-mapped-${filterKey}`}
                      >
                        <p>
                          <strong>Column:</strong> {filterKey}
                        </p>
                        <ApiDataSetAutoMappedTable
                          groupKey={filterKey}
                          groupLabel={filterLabel ?? filterKey}
                          label="filter option"
                          autoMappedItems={autoMappedFilterOptions[filterKey]}
                          newItems={newFilterOptionCandidates[filterKey]}
                          pendingUpdates={pendingUpdates}
                          onUpdate={handleUpdateMapping}
                        />
                      </AccordionSection>
                    );
                  }
                  return null;
                })}
              </Accordion>
            ) : (
              <p>No auto mapped filter options.</p>
            )}

            {dataSet && (
              <ButtonLink
                className="govuk-!-margin-top-4"
                to={generatePath<ReleaseDataSetRouteParams>(
                  releaseApiDataSetDetailsRoute.path,
                  {
                    publicationId,
                    releaseId,
                    dataSetId: dataSet.id,
                  },
                )}
              >
                Continue
              </ButtonLink>
            )}
          </div>
        </div>
      </LoadingSpinner>
    </>
  );
}
