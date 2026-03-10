import { useImmer } from 'use-immer';
import getApiDataSetIndicatorMappings, {
  AutoMappedIndicator,
  IndicatorCandidateWithKey,
  MappableIndicator,
} from '@admin/pages/release/data/utils/getApiDataSetIndicatorMappings';
import React, { useCallback, useEffect, useState } from 'react';
import { PendingMappingUpdate } from '@admin/pages/release/data/types/apiDataSetMappings';
import { useParams } from 'react-router-dom';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import { useQuery } from '@tanstack/react-query';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import apiDataSetVersionQueries from '@admin/queries/apiDataSetVersionQueries';
import PageNav, { NavItem } from '@common/components/PageNav';
import Link from '@admin/components/Link';
import { generatePath } from 'react-router';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import ApiDataSetMappableTable from '@admin/pages/release/data/components/ApiDataSetMappableTable';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import ApiDataSetNewItemsTable from '@admin/pages/release/data/components/ApiDataSetNewItemsTable';
import ApiDataSetAutoMappedTable from '@admin/pages/release/data/components/ApiDataSetAutoMappedTable';
import compact from 'lodash/compact';
import ButtonLink from '@admin/components/ButtonLink';
import NotificationBanner from '@common/components/NotificationBanner';
import getUnmappedIndicatorErrors from '@admin/pages/release/data/utils/getUnmappedIndicatorErrors';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import omit from 'lodash/omit';

export const sectionIds = {
  mappableIndicators: 'mappable-indicators',
  newIndicators: 'new-indicators',
  autoMappedIndicators: 'auto-mapped-locations',
} as const;

export default function ReleaseApiDataSetIndicatorsMappingPage() {
  const [autoMappedIndicators, updateAutoMappedIndicators] = useImmer<
    AutoMappedIndicator[]
  >([]);

  const [newIndicators, updateNewIndicators] = useImmer<
    IndicatorCandidateWithKey[]
  >([]);

  const [mappableIndicators, updateMappableIndicators] = useImmer<
    MappableIndicator[]
  >([]);

  const [pendingUpdates, setPendingUpdates] = useState<PendingMappingUpdate[]>(
    [],
  );

  const { dataSetId, releaseVersionId, publicationId } =
    useParams<ReleaseDataSetRouteParams>();

  const { data: dataSet, isLoading: isLoadingDataSet } = useQuery(
    apiDataSetQueries.get(dataSetId),
  );

  const { data: indicatorsMapping, isLoading: isLoadingMapping } = useQuery({
    ...apiDataSetVersionQueries.getIndicatorsMapping(
      dataSet?.draftVersion?.id ?? '',
    ),
    enabled: !!dataSet?.draftVersion?.id,
  });

  useEffect(() => {
    if (indicatorsMapping) {
      const mappings = getApiDataSetIndicatorMappings(indicatorsMapping);

      updateAutoMappedIndicators(mappings.autoMappedIndicators);
      updateNewIndicators(mappings.newIndicators);
      updateMappableIndicators(mappings.mappableIndicators);
    }
  }, [
    indicatorsMapping,
    updateAutoMappedIndicators,
    updateMappableIndicators,
    updateNewIndicators,
  ]);

  const navItems: NavItem[] = [
    {
      id: sectionIds.mappableIndicators,
      text: 'Indicators not found in new data set',
    },
    {
      id: sectionIds.newIndicators,
      text: 'Indicators not found in old dataset',
    },
    {
      id: sectionIds.autoMappedIndicators,
      text: 'Indicators found in both',
    },
  ];

  const updateMappingState = useCallback(
    (updates: PendingMappingUpdate[]) => {
      updateMappableIndicators(draft => {
        updates.forEach(
          ({ candidateKey, previousMapping, sourceKey, type }) => {
            const candidate = candidateKey
              ? newIndicators.find(indicator => indicator.key === candidateKey)
              : undefined;

            if (previousMapping.type === 'AutoMapped') {
              const updated = draft ?? [];
              updated.push({
                candidate,
                mapping: { ...previousMapping, candidateKey, type },
              });
            } else {
              const indicatorToUpdate = draft.find(
                indicator => indicator.mapping.sourceKey === sourceKey,
              );

              if (indicatorToUpdate) {
                indicatorToUpdate.candidate = candidate;
                indicatorToUpdate.mapping = {
                  ...previousMapping,
                  candidateKey,
                  type,
                };
              }
            }
          },
        );

        // New indicators - remove mapped candidates, add unmapped candidates
        updateNewIndicators(draftNewIndicators => {
          updates.forEach(({ candidateKey, previousCandidate }) => {
            if (candidateKey) {
              // Remove candidates that have been mapped
              draftNewIndicators.forEach((indicator, index) => {
                if (indicator.key === candidateKey) {
                  draftNewIndicators.splice(index, 1);
                }
              });
            } else if (previousCandidate) {
              const updated = draftNewIndicators ?? [];
              updated.push(previousCandidate);
            }
          });
        });
      });

      // AutoMapped - remove previously AutoMapped ones
      updateAutoMappedIndicators(draft => {
        updates.forEach(({ sourceKey, previousMapping }) => {
          if (previousMapping.type !== 'AutoMapped') {
            return;
          }

          draft.forEach((indicator, index) => {
            if (indicator.mapping.sourceKey === sourceKey) {
              draft.splice(index, 1);
            }
          });
        });
      });
    },
    [
      newIndicators,
      updateAutoMappedIndicators,
      updateMappableIndicators,
      updateNewIndicators,
    ],
  );

  // Batch up and debounce so can set indicators to 'no mapping'
  // in quick succession.
  const [handleUpdate] = useDebouncedCallback(async () => {
    if (!dataSet?.draftVersion?.id || !pendingUpdates.length) {
      return;
    }

    await apiDataSetVersionService.updateIndicatorsMapping(
      dataSet.draftVersion?.id,
      {
        updates: pendingUpdates.map(update =>
          omit({ ...update }, 'previousCandidate', 'previousMapping'),
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

  const unmappedIndicatorErrors =
    getUnmappedIndicatorErrors(mappableIndicators);

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
            <span className="govuk-caption-l">Map indicators</span>
            <h2>{dataSet?.title}</h2>
            {!!unmappedIndicatorErrors.length && (
              <NotificationBanner title="Action required">
                <ul className="govuk-list">
                  {unmappedIndicatorErrors.map(error => (
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
              Existing indicators in the API data set need to be mapped to an
              equivalent indicators in the new data set. We try to automatically
              match indicators that appear to be a good fit, but you should
              double-check these choices.
            </p>
            <p>
              Any indicators in the new data set that have not been mapped will
              become new indicators in the API data set.
            </p>

            <h3 className="govuk-heading-l" id={sectionIds.mappableIndicators}>
              Indicators not found in new data set ({mappableIndicators.length})
            </h3>

            {mappableIndicators.length > 0 ? (
              <ApiDataSetMappableTable
                key="indicator-mappings"
                itemLabel="indicator"
                itemPluralLabel="indicators"
                groupLabel="Mappable indicators"
                mappableItems={mappableIndicators}
                newItems={newIndicators}
                pendingUpdates={pendingUpdates}
                renderCandidate={candidate => candidate.label}
                // TODO EES-6764 - no renderCaptionEnd here as there are no groupings for indicators
                renderSource={source => source.label}
                onUpdate={handleUpdateMapping}
              />
            ) : (
              <p>No indicators.</p>
            )}

            <h3
              className="govuk-heading-l govuk-!-margin-top-8"
              id={sectionIds.newIndicators}
            >
              Indicators not found in old dataset ({newIndicators.length}){' '}
              <Tag colour="grey">No action required</Tag>
            </h3>

            {newIndicators.length > 0 ? (
              <Accordion
                id={`${sectionIds.newIndicators}-accordion`}
                testId={`${sectionIds.newIndicators}-accordion`}
              >
                return (
                <AccordionSection
                  backToTop={false}
                  caption="" // TODO EES-6764 - no caption here are indicators aren't grouped
                  heading={`New indicators (${newIndicators.length})`}
                  headingTag="h4"
                  id="new-indicators"
                  key="new-indicators"
                >
                  <ApiDataSetNewItemsTable
                    groupLabel="New indicators"
                    itemPluralLabel="indicators"
                    newItems={newIndicators}
                    renderItem={item => item.label}
                  />
                </AccordionSection>
                );
              </Accordion>
            ) : (
              <p>No new indicators.</p>
            )}

            <h3
              className="govuk-heading-l govuk-!-margin-top-8"
              id={sectionIds.autoMappedIndicators}
            >
              Indicators found in both ({autoMappedIndicators.length}){' '}
              <Tag colour="grey">No action required</Tag>
            </h3>

            {autoMappedIndicators.length > 0 ? (
              <>
                <p>
                  These indicators have been mapped automatically.{' '}
                  <strong>There is no action required.</strong>
                </p>
                <Accordion
                  id={`${sectionIds.autoMappedIndicators}-accordion`}
                  showOpenAll={false}
                  testId={`${sectionIds.autoMappedIndicators}-accordion`}
                >
                  return (
                  <AccordionSection
                    backToTop={false}
                    caption="" // TODO EES-6764 - empty caption, as only have a single accordion section.
                    heading={`Mapped indicators (${autoMappedIndicators.length})`}
                    headingTag="h4"
                    id={sectionIds.autoMappedIndicators}
                    key="mapped-indicators"
                  >
                    <ApiDataSetAutoMappedTable
                      itemLabel="indicator"
                      autoMappedItems={autoMappedIndicators}
                      newItems={newIndicators}
                      pendingUpdates={pendingUpdates}
                      renderCandidate={candidate => candidate.label}
                      renderSource={source => source.label}
                      searchFilter={searchTerm =>
                        autoMappedIndicators.filter(item => {
                          const { candidate, mapping } = item;

                          const searchFields = compact([
                            candidate.label,
                            mapping.source.label,
                          ]);

                          return searchFields.some(field => {
                            return field?.toLowerCase().includes(searchTerm);
                          });
                        })
                      }
                      onUpdate={handleUpdateMapping}
                    />
                  </AccordionSection>
                  );
                </Accordion>
              </>
            ) : (
              <p>No indicators found in both.</p>
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
