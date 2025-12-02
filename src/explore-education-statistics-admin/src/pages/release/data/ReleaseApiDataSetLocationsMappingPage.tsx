import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import ApiDataSetAutoMappedTable from '@admin/pages/release/data/components/ApiDataSetAutoMappedTable';
import ApiDataSetDeletedLocationGroupsTable from '@admin/pages/release/data/components/ApiDataSetDeletedLocationGroupsTable';
import ApiDataSetLocationCode from '@admin/pages/release/data/components/ApiDataSetLocationCode';
import ApiDataSetMappableTable from '@admin/pages/release/data/components/ApiDataSetMappableTable';
import ApiDataSetNewItemsTable from '@admin/pages/release/data/components/ApiDataSetNewItemsTable';
import ApiDataSetNewLocationGroupsTable from '@admin/pages/release/data/components/ApiDataSetNewLocationGroupsTable';
import { PendingMappingUpdate } from '@admin/pages/release/data/types/apiDataSetMappings';
import getApiDataSetLocationMappings, {
  AutoMappedLocation,
  LocationCandidateWithKey,
  MappableLocation,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import getUnmappedLocationErrors from '@admin/pages/release/data/utils/getUnmappedLocationErrors';
import { mappableTableId } from '@admin/pages/release/data/utils/mappingTableIds';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import apiDataSetVersionQueries from '@admin/queries/apiDataSetVersionQueries';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import apiDataSetVersionService, {
  LocationCandidate,
} from '@admin/services/apiDataSetVersionService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import LoadingSpinner from '@common/components/LoadingSpinner';
import NotificationBanner from '@common/components/NotificationBanner';
import PageNav, { NavItem } from '@common/components/PageNav';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import locationLevelsMap, {
  LocationLevelKey,
} from '@common/utils/locationLevelsMap';
import typedKeys from '@common/utils/object/typedKeys';
import { useQuery } from '@tanstack/react-query';
import compact from 'lodash/compact';
import omit from 'lodash/omit';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { generatePath, useParams } from 'react-router-dom';
import { useImmer } from 'use-immer';

export const sectionIds = {
  deletedLocationGroups: 'deleted-location-groups',
  mappableLocations: 'mappable-locations',
  newLocationGroups: 'new-location-groups',
  newLocations: 'new-locations',
  newLocationsLevel: (level: LocationLevelKey) => `new-locations-${level}`,
  autoMappedLocations: 'auto-mapped-locations',
  autoMappedLocationsLevel: (level: LocationLevelKey) =>
    `auto-mapped-locations-${level}`,
} as const;

// Fields to omit from mapping diff.
const omittedDiffingFields: (keyof LocationCandidateWithKey)[] = [
  'key',
  'label',
];

export default function ReleaseApiDataSetLocationsMappingPage() {
  const [autoMappedLocations, updateAutoMappedLocations] = useImmer<
    Partial<Record<LocationLevelKey, AutoMappedLocation[]>>
  >({});
  const [newLocations, updateNewLocations] = useImmer<
    Partial<Record<LocationLevelKey, LocationCandidateWithKey[]>>
  >({});
  const [mappableLocations, updateMappableLocations] = useImmer<
    Partial<Record<LocationLevelKey, MappableLocation[]>>
  >({});

  const [newLocationGroups, updateNewLocationGroups] = useImmer<
    Partial<Record<LocationLevelKey, LocationCandidate[]>>
  >({});
  const [deletedLocationGroups, updateDeletedLocationGroups] = useImmer<
    Partial<Record<LocationLevelKey, LocationCandidate[]>>
  >({});

  const [pendingUpdates, setPendingUpdates] = useState<PendingMappingUpdate[]>(
    [],
  );

  const { dataSetId, releaseVersionId, publicationId } =
    useParams<ReleaseDataSetRouteParams>();

  const { data: dataSet, isLoading: isLoadingDataSet } = useQuery(
    apiDataSetQueries.get(dataSetId),
  );

  const { data: locationsMapping, isLoading: isLoadingMapping } = useQuery({
    ...apiDataSetVersionQueries.getLocationsMapping(
      dataSet?.draftVersion?.id ?? '',
    ),
    enabled: !!dataSet?.draftVersion?.id,
  });

  useEffect(() => {
    if (locationsMapping) {
      const mappings = getApiDataSetLocationMappings(locationsMapping);

      updateAutoMappedLocations(mappings.autoMappedLocations);
      updateNewLocations(mappings.newLocations);
      updateMappableLocations(mappings.mappableLocations);

      updateNewLocationGroups(mappings.newLocationGroups);
      updateDeletedLocationGroups(mappings.deletedLocationGroups);
    }
  }, [
    locationsMapping,
    updateAutoMappedLocations,
    updateDeletedLocationGroups,
    updateMappableLocations,
    updateNewLocationGroups,
    updateNewLocations,
  ]);

  const totalNewLocations = Object.values(newLocations).flatMap(
    key => key,
  ).length;

  const totalAutoMappedLocations = Object.values(autoMappedLocations).flatMap(
    key => key,
  ).length;

  const unmappedLocationErrors = getUnmappedLocationErrors(mappableLocations);

  const navItems: NavItem[] = useMemo(() => {
    return [
      ...(Object.keys(deletedLocationGroups).length > 0
        ? [
            {
              id: sectionIds.deletedLocationGroups,
              text: 'Location groups not found in new data set',
            },
          ]
        : []),
      {
        id: sectionIds.mappableLocations,
        text: 'Locations not found in new data set',
        subNavItems: getSubNavItems(mappableLocations, mappableTableId),
      },
      ...(Object.keys(newLocationGroups).length > 0
        ? [
            {
              id: sectionIds.newLocationGroups,
              text: 'New location groups',
            },
          ]
        : []),
      {
        id: sectionIds.newLocations,
        text: 'New locations',
        subNavItems: getSubNavItems(newLocations, sectionIds.newLocationsLevel),
      },
      {
        id: sectionIds.autoMappedLocations,
        text: 'Auto mapped locations',
        subNavItems: getSubNavItems(
          autoMappedLocations,
          sectionIds.autoMappedLocationsLevel,
        ),
      },
    ];
  }, [
    autoMappedLocations,
    deletedLocationGroups,
    mappableLocations,
    newLocationGroups,
    newLocations,
  ]);

  const updateMappingState = useCallback(
    (updates: PendingMappingUpdate[]) => {
      // MappableLocations - Add or update mapping
      updateMappableLocations(draft => {
        updates.forEach(
          ({ candidateKey, groupKey, previousMapping, sourceKey, type }) => {
            const level = groupKey as LocationLevelKey;
            const candidate = candidateKey
              ? newLocations[level]?.find(
                  location => location.key === candidateKey,
                )
              : undefined;

            if (previousMapping.type === 'AutoMapped') {
              const updated = draft[level] ?? [];

              updated.push({
                candidate,
                mapping: { ...previousMapping, candidateKey, type },
              });

              draft[level] = updated;
            } else {
              const locationToUpdate = draft[level]?.find(
                location => location.mapping.sourceKey === sourceKey,
              );

              if (locationToUpdate) {
                locationToUpdate.candidate = candidate;
                locationToUpdate.mapping = {
                  ...previousMapping,
                  candidateKey,
                  type,
                };
              }
            }
          },
        );
      });

      // NewLocations - remove mapped candidates, add unmapped candidates
      updateNewLocations(draft => {
        updates.forEach(({ candidateKey, groupKey, previousCandidate }) => {
          const level = groupKey as LocationLevelKey;
          if (candidateKey) {
            // Remove candidates that have been mapped
            draft[level]?.forEach((location, index) => {
              if (location.key === candidateKey) {
                draft[level]?.splice(index, 1);
              }
            });
          } else if (previousCandidate) {
            const updated = draft[level] ?? [];
            updated.push(previousCandidate);
            draft[level] = updated;
          }
        });
      });

      // AutoMapped - remove previously AutoMapped ones
      updateAutoMappedLocations(draft => {
        updates.forEach(({ groupKey, sourceKey, previousMapping }) => {
          const level = groupKey as LocationLevelKey;
          if (previousMapping.type !== 'AutoMapped') {
            return;
          }

          draft[level]?.forEach((location, index) => {
            if (location.mapping.sourceKey === sourceKey) {
              draft[level]?.splice(index, 1);
            }
          });

          if (!draft[level]?.length) {
            delete draft[level];
          }
        });
      });
    },
    [
      newLocations,
      updateAutoMappedLocations,
      updateMappableLocations,
      updateNewLocations,
    ],
  );

  // Batch up and debounce so can set locations to 'no mapping'
  // in quick succession.
  const [handleUpdate] = useDebouncedCallback(async () => {
    if (!dataSet?.draftVersion?.id || !pendingUpdates.length) {
      return;
    }

    await apiDataSetVersionService.updateLocationsMapping(
      dataSet.draftVersion?.id,
      {
        updates: pendingUpdates.map(update =>
          omit(
            { ...update, level: update.groupKey as LocationLevelKey },
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
      setPendingUpdates(current => [
        ...current,
        { ...update, level: update.groupKey },
      ]);
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
            <span className="govuk-caption-l">Map locations</span>
            <h2>{dataSet?.title}</h2>
            {!!unmappedLocationErrors.length && (
              <NotificationBanner title="Action required">
                <ul className="govuk-list">
                  {unmappedLocationErrors.map(error => (
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
              Existing locations in the API data set should be mapped to an
              equivalent location in the new data set. We try to automatically
              match up locations that have the same name, but you can
              double-check these choices.
            </p>
            <p>
              Any locations in the new data set that have not been mapped will
              show as new locations in the API data set.
            </p>

            {Object.keys(deletedLocationGroups).length > 0 && (
              <>
                <h3
                  className="govuk-heading-l"
                  id={sectionIds.deletedLocationGroups}
                >
                  {`Location groups not found in new data set (${
                    Object.keys(deletedLocationGroups).length
                  }) `}
                  <Tag colour="grey">No action required</Tag>
                </h3>

                <ApiDataSetDeletedLocationGroupsTable
                  locationGroups={deletedLocationGroups}
                />
              </>
            )}

            <h3 className="govuk-heading-l" id={sectionIds.mappableLocations}>
              Locations not found in new data set
            </h3>

            {typedKeys(mappableLocations).length > 0 ? (
              <>
                {typedKeys(mappableLocations).map(level => {
                  const levelMappableLocations = mappableLocations[level];
                  const groupLabel = locationLevelsMap[level].plural;

                  if (levelMappableLocations?.length) {
                    return (
                      <ApiDataSetMappableTable
                        key={level}
                        candidateHint={candidate => (
                          <ApiDataSetLocationCode location={candidate} />
                        )}
                        candidateIsMajorMapping={(candidate, mapping) => {
                          return Object.entries(
                            omit(mapping.source, omittedDiffingFields),
                          ).some(
                            ([key, value]) =>
                              candidate[key as keyof LocationCandidate] !==
                              value,
                          );
                        }}
                        groupKey={level}
                        groupLabel={groupLabel}
                        itemLabel="location"
                        itemPluralLabel="locations"
                        mappableItems={levelMappableLocations}
                        newItems={newLocations[level]}
                        pendingUpdates={pendingUpdates}
                        renderCandidate={candidate => (
                          <>
                            {candidate.label}
                            <ApiDataSetLocationCode location={candidate} />
                          </>
                        )}
                        renderSource={source => (
                          <>
                            {source.label}
                            <ApiDataSetLocationCode location={source} />
                          </>
                        )}
                        renderSourceDetails={source => (
                          <LocationDetails location={source} />
                        )}
                        onUpdate={handleUpdateMapping}
                      />
                    );
                  }

                  return null;
                })}
              </>
            ) : (
              <p>No locations.</p>
            )}

            {Object.keys(newLocationGroups).length > 0 && (
              <>
                <h3
                  className="govuk-heading-l govuk-!-margin-top-8"
                  id={sectionIds.newLocationGroups}
                >
                  {`New location groups (${
                    Object.keys(deletedLocationGroups).length
                  }) `}
                  <Tag colour="grey">No action required</Tag>
                </h3>

                <ApiDataSetNewLocationGroupsTable
                  locationGroups={newLocationGroups}
                />
              </>
            )}

            <h3
              className="govuk-heading-l govuk-!-margin-top-8"
              id={sectionIds.newLocations}
            >
              New locations ({totalNewLocations}){' '}
              <Tag colour="grey">No action required</Tag>
            </h3>

            {totalNewLocations > 0 ? (
              <Accordion
                id={`${sectionIds.newLocations}-accordion`}
                testId={`${sectionIds.newLocations}-accordion`}
              >
                {typedKeys(newLocations).map(level => {
                  const levelNewLocations = newLocations[level];
                  const groupLabel = locationLevelsMap[level].plural;

                  if (levelNewLocations?.length) {
                    return (
                      <AccordionSection
                        backToTop={false}
                        heading={`${groupLabel} (${levelNewLocations.length})`}
                        headingTag="h4"
                        id={sectionIds.newLocationsLevel(level)}
                        key={level}
                      >
                        <ApiDataSetNewItemsTable
                          groupKey={level}
                          groupLabel={groupLabel}
                          itemPluralLabel="locations"
                          newItems={levelNewLocations}
                          renderItem={item => (
                            <>
                              {item.label}
                              <ApiDataSetLocationCode location={item} />
                            </>
                          )}
                        />
                      </AccordionSection>
                    );
                  }

                  return null;
                })}
              </Accordion>
            ) : (
              <p>No new locations.</p>
            )}

            <h3
              className="govuk-heading-l govuk-!-margin-top-8"
              id={sectionIds.autoMappedLocations}
            >
              Auto mapped locations ({totalAutoMappedLocations}){' '}
              <Tag colour="grey">No action required</Tag>
            </h3>
            {totalAutoMappedLocations > 0 ? (
              <Accordion
                id={`${sectionIds.autoMappedLocations}-accordion`}
                showOpenAll={false}
                testId={`${sectionIds.autoMappedLocations}-accordion`}
              >
                {typedKeys(autoMappedLocations).map(level => {
                  const levelAutoMappedLocations = autoMappedLocations[level];
                  const groupLabel = locationLevelsMap[level].plural;

                  if (levelAutoMappedLocations?.length) {
                    return (
                      <AccordionSection
                        backToTop={false}
                        heading={`${groupLabel} (${levelAutoMappedLocations.length})`}
                        headingTag="h4"
                        id={sectionIds.autoMappedLocationsLevel(level)}
                        key={level}
                      >
                        <ApiDataSetAutoMappedTable
                          candidateHint={candidate => (
                            <ApiDataSetLocationCode location={candidate} />
                          )}
                          groupKey={level}
                          groupLabel={groupLabel}
                          itemLabel="location"
                          autoMappedItems={levelAutoMappedLocations}
                          newItems={newLocations[level]}
                          pendingUpdates={pendingUpdates}
                          renderCandidate={candidate => (
                            <>
                              {candidate.label}
                              <ApiDataSetLocationCode location={candidate} />
                            </>
                          )}
                          renderSource={source => (
                            <>
                              {source.label}
                              <ApiDataSetLocationCode location={source} />
                            </>
                          )}
                          renderSourceDetails={source => (
                            <LocationDetails location={source} />
                          )}
                          searchFilter={searchTerm =>
                            levelAutoMappedLocations.filter(item => {
                              const { candidate, mapping } = item;

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
                                return field
                                  ?.toLowerCase()
                                  .includes(searchTerm);
                              });
                            })
                          }
                          onUpdate={handleUpdateMapping}
                        />
                      </AccordionSection>
                    );
                  }

                  return null;
                })}
              </Accordion>
            ) : (
              <p>No auto mapped locations.</p>
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

function getSubNavItems(
  locations: Partial<
    Record<
      LocationLevelKey,
      (AutoMappedLocation | LocationCandidateWithKey | MappableLocation)[]
    >
  >,
  sectionId: (level: LocationLevelKey) => string,
): NavItem[] {
  return typedKeys(locations).reduce<NavItem[]>((acc, level) => {
    if (locations[level]?.length) {
      acc.push({
        id: sectionId(level),
        text: locationLevelsMap[level]?.plural ?? level,
      });
    }

    return acc;
  }, []);
}

function LocationDetails({ location }: { location: LocationCandidate }) {
  const { code, oldCode, urn, laEstab, ukprn } = location;
  return (
    <>
      {code && <SummaryListItem term="Code">{code}</SummaryListItem>}
      {oldCode && <SummaryListItem term="Old code">{oldCode}</SummaryListItem>}
      {urn && <SummaryListItem term="URN">{urn}</SummaryListItem>}
      {laEstab && <SummaryListItem term="LAESTAB">{laEstab}</SummaryListItem>}
      {ukprn && <SummaryListItem term="UKPRN">{ukprn}</SummaryListItem>}
    </>
  );
}
