import Link from '@admin/components/Link';
import ButtonLink from '@admin/components/ButtonLink';
import getApiDataSetFilterMappings, {
  AutoMappedFilterOption,
  FilterOptionCandidateWithKey,
  MappableFilterOption,
} from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import ApiDataSetMappableTable from '@admin/pages/release/data/components/ApiDataSetMappableTable';
import ApiDataSetNewItemsTable from '@admin/pages/release/data/components/ApiDataSetNewItemsTable';
import ApiDataSetAutoMappedTable from '@admin/pages/release/data/components/ApiDataSetAutoMappedTable';
import getUnmappedFilterErrors from '@admin/pages/release/data/utils/getUnmappedFilterErrors';
import ApiDataSetMappableFilterColumnsTable from '@admin/pages/release/data/components/ApiDataSetMappableFilterColumnsTable';
import ApiDataSetNewFilterColumnsTable from '@admin/pages/release/data/components/ApiDataSetNewFilterColumnsTable';
import { PendingMappingUpdate } from '@admin/pages/release/data/types/apiDataSetMappings';
import { mappableTableId } from '@admin/pages/release/data/utils/mappingTableIds';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import apiDataSetVersionQueries from '@admin/queries/apiDataSetVersionQueries';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import apiDataSetVersionService, {
  FilterCandidate,
  FilterMapping,
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
import camelCase from 'lodash/camelCase';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { generatePath, useParams } from 'react-router-dom';
import omit from 'lodash/omit';
import { useImmer } from 'use-immer';
import sumBy from 'lodash/sumBy';
import compact from 'lodash/compact';

export const sectionIds = {
  mappableFilterColumns: 'mappable-filter-columns',
  mappableFilterOptions: 'mappable-filter-options',
  newFilterColumns: 'new-filter-columns',
  newFilterOptions: 'new-filter-options',
  newFilterOptionsGroup: (filterKey: string) =>
    `new-filter-options-${camelCase(filterKey)}`,
  autoMappedFilterOptions: 'auto-mapped-filter-options',
  autoMappedFilterOptionsGroup: (filterKey: string) =>
    `auto-mapped-filter-options-${camelCase(filterKey)}`,
} as const;

export default function ReleaseApiDataSetFiltersMappingPage() {
  const [autoMappedFilterOptions, updateAutoMappedFilterOptions] = useImmer<
    Dictionary<AutoMappedFilterOption[]>
  >({});
  const [newFilterOptions, updateNewFilterOptions] = useImmer<
    Dictionary<FilterOptionCandidateWithKey[]>
  >({});
  const [mappableFilterOptions, updateMappableFilterOptions] = useImmer<
    Dictionary<MappableFilterOption[]>
  >({});
  const [mappableFilters, updateMappableFilters] = useImmer<
    Dictionary<FilterMapping>
  >({});
  const [newFilters, updateNewFilters] = useImmer<Dictionary<FilterCandidate>>(
    {},
  );
  const [pendingUpdates, setPendingUpdates] = useState<PendingMappingUpdate[]>(
    [],
  );

  const { dataSetId, releaseVersionId, publicationId } =
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
      const mappings = getApiDataSetFilterMappings(filtersMapping);

      updateAutoMappedFilterOptions(mappings.autoMappedFilterOptions);
      updateNewFilterOptions(mappings.newFilterOptions);
      updateMappableFilterOptions(mappings.mappableFilterOptions);
      updateMappableFilters(mappings.mappableFilters);
      updateNewFilters(mappings.newFilters);
    }
  }, [
    filtersMapping,
    updateAutoMappedFilterOptions,
    updateMappableFilters,
    updateMappableFilterOptions,
    updateNewFilters,
    updateNewFilterOptions,
  ]);

  const totalNewFilterOptions = sumBy(
    Object.values(newFilterOptions),
    candidates => candidates.length,
  );

  const totalAutoMappedFilterOptions = sumBy(
    Object.values(autoMappedFilterOptions),
    options => options.length,
  );

  const unmappedFilterErrors = filtersMapping
    ? getUnmappedFilterErrors(mappableFilterOptions, filtersMapping)
    : [];

  const navItems: NavItem[] = useMemo(() => {
    return [
      ...(Object.keys(mappableFilters).length > 0
        ? [
            {
              id: sectionIds.mappableFilterColumns,
              text: 'Filter columns not found in new data set',
            },
          ]
        : []),
      {
        id: sectionIds.mappableFilterOptions,
        text: 'Filter options not found in new data set',
        subNavItems: Object.keys(mappableFilterOptions).map(filterKey => {
          return {
            id: mappableTableId(filterKey),
            text: filtersMapping?.mappings[filterKey].source.label ?? filterKey,
          };
        }),
      },
      ...(Object.keys(newFilters).length > 0
        ? [
            {
              id: sectionIds.newFilterColumns,
              text: 'New filter columns',
            },
          ]
        : []),
      {
        id: sectionIds.newFilterOptions,
        text: 'Filter options not found in old dataset',
        subNavItems: Object.keys(newFilterOptions).map(filterKey => {
          return {
            id: sectionIds.newFilterOptionsGroup(filterKey),
            text: filtersMapping?.candidates[filterKey].label ?? filterKey,
          };
        }),
      },
      {
        id: sectionIds.autoMappedFilterOptions,
        text: 'Filter options found in both',
        subNavItems: Object.keys(autoMappedFilterOptions).map(filterKey => {
          return {
            id: sectionIds.autoMappedFilterOptionsGroup(filterKey),
            text: filtersMapping?.mappings[filterKey].source.label ?? filterKey,
          };
        }),
      },
    ];
  }, [
    autoMappedFilterOptions,
    filtersMapping?.candidates,
    filtersMapping?.mappings,
    mappableFilterOptions,
    mappableFilters,
    newFilterOptions,
    newFilters,
  ]);

  const updateMappingState = useCallback(
    (updates: PendingMappingUpdate[]) => {
      // MappableFilterOption - Add or update mapping
      updateMappableFilterOptions(draft => {
        updates.forEach(
          ({ candidateKey, groupKey, previousMapping, sourceKey, type }) => {
            const candidate = candidateKey
              ? newFilterOptions[groupKey]?.find(
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
      updateNewFilterOptions(draft => {
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
      newFilterOptions,
      updateAutoMappedFilterOptions,
      updateMappableFilterOptions,
      updateNewFilterOptions,
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
            releaseVersionId,
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
              Existing filters in the API data set need to be mapped to an
              equivalent filter in the new data set. We try to automatically
              match filters that appear to be a good fit, but you should
              double-check these choices.
            </p>
            <p>
              Any filters in the new data set that have not been mapped will
              become new filters in the API data set.
            </p>

            {Object.keys(mappableFilters).length > 0 && (
              <>
                <h3
                  className="govuk-heading-l dfe-flex dfe-align-items--center"
                  id={sectionIds.mappableFilterColumns}
                >
                  Filter columns not found in new data set{' '}
                  <Tag className="govuk-!-margin-left-2" colour="grey">
                    No action required
                  </Tag>
                </h3>

                <ApiDataSetMappableFilterColumnsTable
                  mappableFilters={mappableFilters}
                />
              </>
            )}

            <h3
              className="govuk-heading-l"
              id={sectionIds.mappableFilterOptions}
            >
              Filter options not found in new data set
            </h3>

            {Object.keys(mappableFilterOptions).length > 0 && filtersMapping ? (
              <>
                {Object.keys(mappableFilterOptions).map(filterKey => (
                  <ApiDataSetMappableTable
                    key={filterKey}
                    groupKey={filterKey}
                    itemLabel="filter option"
                    itemPluralLabel="filter options"
                    groupLabel={filtersMapping.mappings[filterKey].source.label}
                    mappableItems={mappableFilterOptions[filterKey]}
                    newItems={newFilterOptions[filterKey]}
                    pendingUpdates={pendingUpdates}
                    renderCandidate={candidate => candidate.label}
                    renderCaptionEnd={
                      <>
                        <br />
                        <div className="govuk-!-font-size-19 govuk-!-margin-top-4">
                          Column:{' '}
                          <code className="govuk-!-font-weight-regular">
                            {filterKey}
                          </code>
                        </div>
                      </>
                    }
                    renderSource={source => source.label}
                    onUpdate={handleUpdateMapping}
                  />
                ))}
              </>
            ) : (
              <p>No filter options.</p>
            )}

            {Object.keys(newFilters).length > 0 && (
              <>
                <h3
                  className="govuk-heading-l govuk-!-margin-top-8"
                  id={sectionIds.newFilterColumns}
                >
                  New filter columns <Tag colour="grey">No action required</Tag>
                </h3>
                <ApiDataSetNewFilterColumnsTable newFilters={newFilters} />
              </>
            )}

            <h3
              className="govuk-heading-l govuk-!-margin-top-8"
              id={sectionIds.newFilterOptions}
            >
              Filter options not found in old dataset ({totalNewFilterOptions}){' '}
              <Tag colour="grey">No action required</Tag>
            </h3>

            {totalNewFilterOptions > 0 && filtersMapping ? (
              <Accordion
                id={`${sectionIds.newFilterOptions}-accordion`}
                testId={`${sectionIds.newFilterOptions}-accordion`}
              >
                {Object.keys(newFilterOptions).map(filterKey => {
                  if (newFilterOptions[filterKey].length) {
                    const filterLabel =
                      filtersMapping.candidates[filterKey].label;

                    return (
                      <AccordionSection
                        caption={
                          <>
                            <strong>Column:</strong> <code>{filterKey}</code>
                          </>
                        }
                        goToTop={false}
                        heading={`${filterLabel} (${newFilterOptions[filterKey].length})`}
                        headingTag="h4"
                        id={sectionIds.newFilterOptionsGroup(filterKey)}
                        key={filterKey}
                      >
                        <ApiDataSetNewItemsTable
                          groupKey={filterKey}
                          groupLabel={filterLabel}
                          itemPluralLabel="filter options"
                          newItems={newFilterOptions[filterKey]}
                          renderItem={item => item.label}
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
              id={sectionIds.autoMappedFilterOptions}
            >
              Filter options found in both ({totalAutoMappedFilterOptions}){' '}
              <Tag colour="grey">No action required</Tag>
            </h3>

            {totalAutoMappedFilterOptions > 0 ? (
              <>
                <p>
                  These filters have been mapped automatically.{' '}
                  <strong>There is no action required.</strong>
                </p>
                <Accordion
                  id={`${sectionIds.autoMappedFilterOptions}-accordion`}
                  showOpenAll={false}
                  testId={`${sectionIds.autoMappedFilterOptions}-accordion`}
                >
                  {Object.keys(autoMappedFilterOptions).map(filterKey => {
                    if (
                      autoMappedFilterOptions[filterKey]?.length &&
                      filtersMapping
                    ) {
                      const filterLabel =
                        filtersMapping.mappings[filterKey].source.label;

                      return (
                        <AccordionSection
                          caption={
                            <>
                              <strong>Column:</strong> <code>{filterKey}</code>
                            </>
                          }
                          goToTop={false}
                          heading={`${filterLabel} (${autoMappedFilterOptions[filterKey].length})`}
                          headingTag="h4"
                          id={sectionIds.autoMappedFilterOptionsGroup(
                            filterKey,
                          )}
                          key={filterKey}
                        >
                          <ApiDataSetAutoMappedTable
                            groupKey={filterKey}
                            groupLabel={filterLabel}
                            itemLabel="filter option"
                            autoMappedItems={autoMappedFilterOptions[filterKey]}
                            newItems={newFilterOptions[filterKey]}
                            pendingUpdates={pendingUpdates}
                            renderCandidate={candidate => candidate.label}
                            renderSource={source => source.label}
                            searchFilter={searchTerm =>
                              autoMappedFilterOptions[filterKey].filter(
                                item => {
                                  const { candidate, mapping } = item;

                                  const searchFields = compact([
                                    candidate.label,
                                    mapping.source.label,
                                  ]);

                                  return searchFields.some(field => {
                                    return field
                                      ?.toLowerCase()
                                      .includes(searchTerm);
                                  });
                                },
                              )
                            }
                            onUpdate={handleUpdateMapping}
                          />
                        </AccordionSection>
                      );
                    }
                    return null;
                  })}
                </Accordion>
              </>
            ) : (
              <p>No filter options found in both.</p>
            )}

            {dataSet && (
              <ButtonLink
                className="govuk-!-margin-top-4"
                to={generatePath<ReleaseDataSetRouteParams>(
                  releaseApiDataSetDetailsRoute.path,
                  {
                    publicationId,
                    releaseVersionId,
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
