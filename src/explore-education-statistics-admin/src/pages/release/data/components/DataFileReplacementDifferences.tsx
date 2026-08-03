import dataReplacementService, {
  DataReplacementPlan,
  FilterMappingFilterGroups,
  FilterMappingFilterItems,
  FilterMappingPlan,
  FilterMappingSource,
  PlanMappings,
  UpdateMappingPayload,
} from '@admin/services/dataReplacementService';
import React, { useCallback, useMemo } from 'react';
import { useImmer } from 'use-immer';
import DataFileReplacementFilterDifferencesTable from '@admin/pages/release/data/components/DataFileReplacementFilterDifferencesTable';
import DataFileReplacementDifferencesTable from './DataFileReplacementDifferencesTable';

interface Props {
  releaseVersionId: string;
  fileId: string;
  replacementFileId: string;
  plan: DataReplacementPlan;
  reloadPlan: () => void;
}

export default function DataFileReplacementDifferences({
  releaseVersionId,
  fileId,
  replacementFileId,
  plan,
  reloadPlan,
}: Props) {
  const [planMappings, updatePlanMappings] = useImmer<PlanMappings>(
    plan.mapping,
  );

  // instead of passing the datablocks down to the tables, it seems better to handle this here
  const indicatorReplacementGroups = useMemo(() => {
    // combine all indicator groups from data blocks and footnotes and then dedupe by label
    return [
      ...plan.dataBlocks.flatMap(block =>
        Object.values(block.indicatorGroups || {}),
      ),
      ...plan.footnotes.flatMap(footnote =>
        Object.values(footnote.indicatorGroups || {}),
      ),
    ];
  }, [plan.dataBlocks, plan.footnotes]);

  const locationReplacementGroups = useMemo(() => {
    return plan.dataBlocks.flatMap(block =>
      Object.values(block.locations || {}),
    );
  }, [plan.dataBlocks]);

  const handleIndicatorsMappingUpdate = useCallback(
    async ({ sourceKey, candidateKey }: UpdateMappingPayload) => {
      updatePlanMappings(draft => {
        draft.indicators.mappings[sourceKey].candidateKey = candidateKey;
        draft.indicators.mappings[sourceKey].type = 'ManuallySet';
        return draft;
      });
      await dataReplacementService.updatePlanIndicatorMappings(
        releaseVersionId,
        fileId,
        replacementFileId,
        [
          {
            originalId: sourceKey,
            newReplacementId: candidateKey,
          },
        ],
      );

      reloadPlan();
    },
    [
      updatePlanMappings,
      releaseVersionId,
      fileId,
      replacementFileId,
      reloadPlan,
    ],
  );

  const handleLocationsMappingUpdate = useCallback(
    async ({ sourceKey, candidateKey }: UpdateMappingPayload) => {
      updatePlanMappings(draft => {
        draft.locations.mappings[sourceKey].candidateKey = candidateKey;
        draft.locations.mappings[sourceKey].type = 'ManuallySet';
        return draft;
      });
      await dataReplacementService.updatePlanLocationMappings(
        releaseVersionId,
        fileId,
        replacementFileId,
        [
          {
            originalId: sourceKey,
            newReplacementId: candidateKey,
          },
        ],
      );

      reloadPlan();
    },
    [
      updatePlanMappings,
      releaseVersionId,
      fileId,
      replacementFileId,
      reloadPlan,
    ],
  );

  const handleFiltersMappingUpdate = useCallback(
    async (updatePayload: UpdateMappingPayload, type: string) => {
      const { sourceKey, candidateKey } = updatePayload;

      updatePlanMappings(draft => {
        const updateMapping = (
          mappingsPlan:
            | FilterMappingPlan
            | FilterMappingFilterGroups
            | FilterMappingFilterItems<FilterMappingSource>,
        ): boolean => {
          const mapping = mappingsPlan.mappings[sourceKey];

          if (mapping) {
            mapping.candidateKey = candidateKey;
            mapping.type = 'ManuallySet';
            return true;
          }

          return Object.values(mappingsPlan.mappings).some(childMapping => {
            if ('filterGroups' in childMapping) {
              return updateMapping(
                childMapping.filterGroups as FilterMappingFilterGroups,
              );
            }

            if ('filterItems' in childMapping) {
              return updateMapping(
                childMapping.filterItems as FilterMappingFilterItems<FilterMappingSource>,
              );
            }

            return false;
          });
        };

        updateMapping(draft.filters);

        return draft;
      });

      const updateData = {
        originalId: updatePayload.sourceKey,
        newReplacementId: updatePayload.candidateKey,
      };
      const filterUpdates = type === 'filter' ? [updateData] : [];
      const filterGroupUpdates = type === 'group' ? [updateData] : [];
      const filterItemUpdates = type === 'item' ? [updateData] : [];

      await dataReplacementService.updatePlanFilterMappings(
        releaseVersionId,
        fileId,
        replacementFileId,
        filterUpdates,
        filterGroupUpdates,
        filterItemUpdates,
      );

      reloadPlan();
    },
    [
      fileId,
      releaseVersionId,
      reloadPlan,
      replacementFileId,
      updatePlanMappings,
    ],
  );

  return (
    <>
      <h3>Mapping Dependencies</h3>

      <p>
        The following items were not found in the replacement data and were
        previously used in existing datablocks or footnotes.
        <br /> Please map these items to new items that appear in the
        replacement data or select "No mapping" for items that are no longer
        represented.
      </p>

      <DataFileReplacementDifferencesTable
        tableId="replacements-differences-indicators-table"
        itemType="indicator"
        mappingsPlan={planMappings.indicators}
        replacementGroups={indicatorReplacementGroups}
        getGroupMappings={group => group.indicators}
        handleMappingUpdate={handleIndicatorsMappingUpdate}
        rowLabel="label"
        mappedDataLabels={{
          label: 'Label',
          name: 'Name',
        }}
      />
      <DataFileReplacementDifferencesTable
        tableId="replacements-differences-locations-table"
        itemType="location"
        mappingsPlan={planMappings.locations}
        replacementGroups={locationReplacementGroups}
        getGroupMappings={group => group.locationAttributes}
        handleMappingUpdate={handleLocationsMappingUpdate}
        rowLabel="name"
        mappedDataLabels={{
          name: 'Name',
          code: 'Code',
        }}
      />

      <DataFileReplacementFilterDifferencesTable
        filters={plan.mapping.filters}
        handleMappingUpdate={handleFiltersMappingUpdate}
      />
    </>
  );
}
