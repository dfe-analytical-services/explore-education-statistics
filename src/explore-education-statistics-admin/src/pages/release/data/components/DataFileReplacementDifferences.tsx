import dataReplacementService, {
  DataReplacementPlan,
  FilterMappingFilterGroups,
  FilterMappingFilterItems,
  FilterMappingPlan,
  FilterMappingSource,
  PlanMappings,
  ReplacementMapping,
  UpdateMappingPayload,
} from '@admin/services/dataReplacementService';
import React, { useCallback, useEffect, useMemo } from 'react';
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

type FilterMappings =
  | FilterMappingPlan
  | FilterMappingFilterGroups
  | FilterMappingFilterItems<FilterMappingSource>;

const findFilterMapping = (
  filters: FilterMappings,
  sourceKey: string,
): ReplacementMapping<FilterMappingSource> | undefined => {
  const mapping = filters.mappings[sourceKey];

  if (mapping) {
    return mapping;
  }

  for (let i = 0; i < Object.values(filters.mappings).length; i += 1) {
    const child = Object.values(filters.mappings)[i];
    if ('filterGroups' in child) {
      const result = findFilterMapping(
        child.filterGroups as FilterMappingFilterGroups,
        sourceKey,
      );
      if (result) return result;
    }

    if ('filterItems' in child) {
      const result = findFilterMapping(
        child.filterItems as FilterMappingFilterItems<FilterMappingSource>,
        sourceKey,
      );
      if (result) return result;
    }
  }

  return undefined;
};

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

  useEffect(() => {
    updatePlanMappings(plan.mapping);
  }, [plan.mapping, updatePlanMappings]);

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
      const currentMapping = planMappings.indicators.mappings[sourceKey];

      if (!currentMapping) {
        throw new Error(`Could not find indicator mapping: ${sourceKey}`);
      }

      const previousMapping = {
        candidateKey: currentMapping.candidateKey,
        type: currentMapping.type,
      };

      updatePlanMappings(draft => {
        draft.indicators.mappings[sourceKey].candidateKey = candidateKey;
        draft.indicators.mappings[sourceKey].type = 'ManuallySet';
        return draft;
      });

      try {
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
      } catch (error) {
        updatePlanMappings(draft => {
          draft.indicators.mappings[sourceKey].candidateKey =
            previousMapping.candidateKey;
          draft.indicators.mappings[sourceKey].type = previousMapping.type;
          return draft;
        });
      }
    },
    [
      planMappings.indicators.mappings,
      updatePlanMappings,
      releaseVersionId,
      fileId,
      replacementFileId,
      reloadPlan,
    ],
  );

  const handleLocationsMappingUpdate = useCallback(
    async ({ sourceKey, candidateKey }: UpdateMappingPayload) => {
      const currentMapping = planMappings.locations.mappings[sourceKey];

      if (!currentMapping) {
        throw new Error(`Could not find location mapping: ${sourceKey}`);
      }

      const previousMapping = {
        candidateKey: currentMapping.candidateKey,
        type: currentMapping.type,
      };

      updatePlanMappings(draft => {
        draft.locations.mappings[sourceKey].candidateKey = candidateKey;
        draft.locations.mappings[sourceKey].type = 'ManuallySet';
        return draft;
      });

      try {
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
      } catch (error) {
        updatePlanMappings(draft => {
          draft.locations.mappings[sourceKey].candidateKey =
            previousMapping.candidateKey;
          draft.locations.mappings[sourceKey].type = previousMapping.type;
          return draft;
        });
      }
    },
    [
      planMappings.locations.mappings,
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

      const currentMapping = findFilterMapping(planMappings.filters, sourceKey);

      if (!currentMapping) {
        throw new Error(`Could not find filter mapping: ${sourceKey}`);
      }

      const previousMapping = {
        candidateKey: currentMapping.candidateKey,
        type: currentMapping.type,
      };

      updatePlanMappings(draft => {
        const mapping = findFilterMapping(draft.filters, sourceKey);

        if (mapping) {
          mapping.candidateKey = candidateKey;
          mapping.type = 'ManuallySet';
        }
      });

      const update = {
        originalId: sourceKey,
        newReplacementId: candidateKey,
      };

      try {
        await dataReplacementService.updatePlanFilterMappings(
          releaseVersionId,
          fileId,
          replacementFileId,
          type === 'filter' ? [update] : [],
          type === 'group' ? [update] : [],
          type === 'item' ? [update] : [],
        );

        reloadPlan();
      } catch (error) {
        updatePlanMappings(draft => {
          const mapping = findFilterMapping(draft.filters, sourceKey);

          if (mapping) {
            mapping.candidateKey = previousMapping.candidateKey;
            mapping.type = previousMapping.type;
          }
        });
      }
    },
    [
      fileId,
      planMappings.filters,
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
        filters={planMappings.filters}
        handleMappingUpdate={handleFiltersMappingUpdate}
      />
    </>
  );
}
