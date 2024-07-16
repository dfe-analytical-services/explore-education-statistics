import Link from '@admin/components/Link';
import ButtonLink from '@admin/components/ButtonLink';
import ApiDataSetMappableLocationsTable from '@admin/pages/release/data/components/ApiDataSetMappableLocationsTable';
import ApiDataSetNewLocationsTable from '@admin/pages/release/data/components/ApiDataSetNewLocationsTable';
import ApiDataSetAutoMappedLocationsTable from '@admin/pages/release/data/components/ApiDataSetAutoMappedLocationsTable';
import getApiDataSetLocationMappings, {
  AutoMappedLocation,
  LocationCandidateWithKey,
  LocationMappingWithKey,
  MappableLocation,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import getUnmappedLocationErrors from '@admin/pages/release/data/utils/getUnmappedLocationErrors';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import apiDataSetVersionQueries from '@admin/queries/apiDataSetVersionQueries';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import apiDataSetVersionService, {
  LocationMappingUpdate,
} from '@admin/services/apiDataSetVersionService';
import Tag from '@common/components/Tag';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import PageNav, { NavItem } from '@common/components/PageNav';
import NotificationBanner from '@common/components/NotificationBanner';
import locationLevelsMap, {
  LocationLevelKey,
} from '@common/utils/locationLevelsMap';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import { useQuery } from '@tanstack/react-query';
import React, { useCallback, useEffect, useState } from 'react';
import { generatePath, useParams } from 'react-router-dom';
import omit from 'lodash/omit';
import { useImmer } from 'use-immer';

export const noMappingValue = 'noMapping';

export interface PendingLocationMappingUpdate extends LocationMappingUpdate {
  previousCandidate?: LocationCandidateWithKey;
  previousMapping: LocationMappingWithKey;
}

export default function ReleaseApiDataSetLocationsMappingPage() {
  const [autoMappedLocations, updateAutoMappedLocations] = useImmer<
    Partial<Record<LocationLevelKey, AutoMappedLocation[]>>
  >({});
  const [newLocationCandidates, updateNewLocationCandidates] = useImmer<
    Partial<Record<LocationLevelKey, LocationCandidateWithKey[]>>
  >({});
  const [mappableLocations, updateMappableLocations] = useImmer<
    Partial<Record<LocationLevelKey, MappableLocation[]>>
  >({});
  const [pendingUpdates, setPendingUpdates] = useState<
    PendingLocationMappingUpdate[]
  >([]);

  const { dataSetId, releaseId, publicationId } =
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
      const {
        autoMappedLocations: initialAutoMappedLocations,
        newLocationCandidates: initialNewLocationCandidates,
        mappableLocations: initialMappableLocations,
      } = getApiDataSetLocationMappings(locationsMapping);
      updateAutoMappedLocations(initialAutoMappedLocations);
      updateNewLocationCandidates(initialNewLocationCandidates);
      updateMappableLocations(initialMappableLocations);
    }
  }, [
    locationsMapping,
    updateAutoMappedLocations,
    updateMappableLocations,
    updateNewLocationCandidates,
  ]);

  const totalNewLocationCandidates = Object.values(
    newLocationCandidates,
  ).flatMap(key => key).length;

  const totalAutoMappedLocations = Object.values(autoMappedLocations).flatMap(
    key => key,
  ).length;

  const unmappedLocationErrors = getUnmappedLocationErrors(mappableLocations);

  const navItems: NavItem[] = [
    {
      id: 'mappable-locations',
      text: 'Locations not found in the new data set',
      subNavItems: getSubNavItems(mappableLocations, 'mappable'),
    },
    {
      id: 'new-locations',
      text: 'New locations',
      subNavItems: getSubNavItems(newLocationCandidates, 'new-locations'),
    },
    {
      id: 'auto-mapped-locations',
      text: 'Auto mapped locations',
      subNavItems: getSubNavItems(autoMappedLocations, 'auto-mapped'),
    },
  ];

  const updateMappingState = useCallback(
    (updates: PendingLocationMappingUpdate[]) => {
      // MappableLocations - Add or update mapping
      updateMappableLocations(draft => {
        updates.forEach(
          ({ candidateKey, level, previousMapping, sourceKey, type }) => {
            const candidate = candidateKey
              ? newLocationCandidates[level]?.find(
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
      updateNewLocationCandidates(draft => {
        updates.forEach(({ candidateKey, level, previousCandidate }) => {
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
        updates.forEach(({ level, sourceKey, previousMapping }) => {
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
      newLocationCandidates,
      updateAutoMappedLocations,
      updateMappableLocations,
      updateNewLocationCandidates,
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
          omit(update, 'previousCandidate', 'previousMapping'),
        ),
      },
    );
    updateMappingState(pendingUpdates);
    setPendingUpdates([]);
  }, 1000);

  const handleUpdateMapping = useCallback(
    async (update: PendingLocationMappingUpdate) => {
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
              Existing locations in the publication data set should be mapped to
              an equivalent location in the new data set. We try to
              automatically match up locations that have the same name, but you
              can double-check these choices.
            </p>
            <p>
              Any locations in the new data set that have not been mapped will
              show as new locations in the publication data set.
            </p>

            <h3 className="govuk-heading-l" id="unmapped-locations">
              Locations not found in the new data set
            </h3>

            {Object.keys(mappableLocations).length > 0 ? (
              <>
                {(Object.keys(mappableLocations) as LocationLevelKey[]).map(
                  level => {
                    const levelMappableLocations = mappableLocations[level];
                    if (levelMappableLocations?.length) {
                      return (
                        <ApiDataSetMappableLocationsTable
                          key={`unmapped-${level}`}
                          level={level}
                          locations={levelMappableLocations}
                          newLocations={newLocationCandidates[level]}
                          pendingUpdates={pendingUpdates}
                          onUpdate={handleUpdateMapping}
                        />
                      );
                    }
                    return null;
                  },
                )}
              </>
            ) : (
              <p>No locations not found in the new data set.</p>
            )}

            <h3
              className="govuk-heading-l govuk-!-margin-top-8"
              id="new-locations"
            >
              New locations ({totalNewLocationCandidates}){' '}
              <Tag colour="grey">No action required</Tag>
            </h3>

            {totalNewLocationCandidates > 0 ? (
              <Accordion id="new-locations" testId="new-locations-accordion">
                {(Object.keys(newLocationCandidates) as LocationLevelKey[]).map(
                  level => {
                    const levelNewLocations = newLocationCandidates[level];
                    if (levelNewLocations?.length) {
                      return (
                        <AccordionSection
                          goToTop={false}
                          heading={`${
                            locationLevelsMap[level]?.plural ?? level
                          } (${levelNewLocations.length})`}
                          headingTag="h4"
                          id={`new-locations-${level}`}
                          key={`new-locations-${level}`}
                        >
                          <ApiDataSetNewLocationsTable
                            level={level}
                            locations={levelNewLocations}
                          />
                        </AccordionSection>
                      );
                    }
                    return null;
                  },
                )}
              </Accordion>
            ) : (
              <p>No new locations.</p>
            )}

            <h3
              className="govuk-heading-l govuk-!-margin-top-8"
              id="auto-mapped-locations"
            >
              Auto mapped locations ({totalAutoMappedLocations}){' '}
              <Tag colour="grey">No action required</Tag>
            </h3>
            {totalAutoMappedLocations > 0 ? (
              <Accordion
                id="auto-mapped"
                showOpenAll={false}
                testId="auto-mapped-accordion"
              >
                {(Object.keys(autoMappedLocations) as LocationLevelKey[]).map(
                  level => {
                    const levelAutoMappedLocations = autoMappedLocations[level];
                    if (levelAutoMappedLocations?.length) {
                      return (
                        <AccordionSection
                          goToTop={false}
                          heading={`${
                            locationLevelsMap[level]?.plural ?? level
                          } (${levelAutoMappedLocations?.length})`}
                          headingTag="h4"
                          id={`auto-mapped-${level}`}
                          key={`auto-mapped-${level}`}
                        >
                          <ApiDataSetAutoMappedLocationsTable
                            level={level}
                            locations={levelAutoMappedLocations}
                            newLocations={newLocationCandidates[level]}
                            pendingUpdates={pendingUpdates}
                            onUpdate={handleUpdateMapping}
                          />
                        </AccordionSection>
                      );
                    }
                    return null;
                  },
                )}
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

function getSubNavItems(
  locations: Partial<
    Record<
      LocationLevelKey,
      (AutoMappedLocation | LocationCandidateWithKey | MappableLocation)[]
    >
  >,
  key: string,
) {
  return (Object.keys(locations) as LocationLevelKey[]).reduce<NavItem[]>(
    (acc, level) => {
      if (locations[level]?.length) {
        acc.push({
          id: `${key}-${level}`,
          text: locationLevelsMap[level]?.plural ?? level,
        });
      }
      return acc;
    },
    [],
  );
}
