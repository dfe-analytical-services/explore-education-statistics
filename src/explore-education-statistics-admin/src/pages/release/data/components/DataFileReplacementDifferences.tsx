import dataReplacementService, {
  DataReplacementPlan,
  IndicatorGroupReplacement,
  LocationReplacement,
  MappingsPlan,
  PlanMappings,
  ReplacementMapping,
  TargetReplacement,
  UpdateMappingPayload,
} from '@admin/services/dataReplacementService';
import React, { useCallback, useMemo } from 'react';
import { useImmer } from 'use-immer';
import pickBy from 'lodash/pickBy';
import { Dictionary } from '@common/types';
import DataFileReplacementDifferencesTable, {
  TableMappingGroup,
} from './DataFileReplacementDifferencesTable';

interface Props {
  releaseVersionId: string;
  fileId: string;
  replacementFileId: string;
  plan: DataReplacementPlan;
  reloadPlan: () => void;
}

const getNonAutoSetMappings = (mappings: MappingsPlan<unknown>['mappings']) => {
  return pickBy(mappings, value => value.type !== 'AutoSet');
};

export function uniqueByLabel<T extends { label: string }>(items: T[]): T[] {
  return Array.from(new Map(items.map(item => [item.label, item])).values());
}

/**
 * Builds the data for the tables.
 *
 * This is restricted to items in the groups that are in the replacement mappings.
 *
 * Returns an object containing both:
 * Groups of mapping ids with the same structure as the original groups (filters out empty groups)
 * A set of all mapping ids that both exist in the replacement mappings and the original groups
 *
 */
function createTableMappingsAndGroups<
  G extends IndicatorGroupReplacement | LocationReplacement,
  R extends G[keyof G] extends TargetReplacement ? G[keyof G] : never,
>(
  replacementMappings: Dictionary<ReplacementMapping<unknown>>,
  groups: G[],
  childKey: keyof G,
) {
  return groups.reduce<{
    allUniqueMappingIds: Set<string>;
    groupedMappings: TableMappingGroup[];
  }>(
    ({ allUniqueMappingIds, groupedMappings }, group) => {
      const children = (group[childKey] ?? []) as R[];

      const filteredChildren = children.filter(
        child => replacementMappings[child.id],
      );
      if (filteredChildren.length === 0)
        return { allUniqueMappingIds, groupedMappings };

      const mappings = filteredChildren.map(child => child.id);

      mappings.forEach(childId => allUniqueMappingIds.add(childId));

      return {
        allUniqueMappingIds,
        groupedMappings: [
          ...groupedMappings,
          {
            label: group.label,
            mappings,
          },
        ],
      };
    },
    {
      allUniqueMappingIds: new Set<string>(),
      groupedMappings: [],
    },
  );
}

export default function DataFileReplacementDifferences({
  releaseVersionId,
  fileId,
  replacementFileId,
  plan,
  reloadPlan,
}: Props) {
  const groupsAndMappingsForTables = useMemo(() => {
    // first determine the indicators
    const indicatorsToShow = getNonAutoSetMappings(
      plan.mapping.indicators.mappings,
    );

    // combine all indicator groups from data blocks and footnotes and then dedupe by label
    const allIndicatorGroups = [
      ...plan.dataBlocks.flatMap(block =>
        Object.values(block.indicatorGroups || {}),
      ),
      ...plan.footnotes.flatMap(footnote =>
        Object.values(footnote.indicatorGroups || {}),
      ),
    ];

    const uniqueIndicatorGroups = uniqueByLabel(allIndicatorGroups);

    const groupedIndicatorMappings = createTableMappingsAndGroups(
      indicatorsToShow,
      uniqueIndicatorGroups,
      'indicators',
    );

    const allLocations = plan.dataBlocks.flatMap(block =>
      Object.values(block.locations || {}),
    );

    // second determine the locations
    const locationsToShow = getNonAutoSetMappings(
      plan.mapping.locations.mappings,
    );

    const groupedLocationMappings = createTableMappingsAndGroups(
      locationsToShow,
      allLocations,
      'locationAttributes',
    );

    // TODO: filters will follow a similar pattern

    return {
      indicators: {
        allUniqueMappingIds: groupedIndicatorMappings.allUniqueMappingIds,
        groupedMappings: groupedIndicatorMappings.groupedMappings,
      },
      locations: {
        allUniqueMappingIds: groupedLocationMappings.allUniqueMappingIds,
        groupedMappings: groupedLocationMappings.groupedMappings,
      },
    };
  }, [
    plan.dataBlocks,
    plan.footnotes,
    plan.mapping.indicators.mappings,
    plan.mapping.locations.mappings,
  ]);

  const [planMappings, updatePlanMappings] = useImmer<PlanMappings>(
    plan.mapping,
  );

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
            originalLocationId: sourceKey,
            newReplacementLocationId: candidateKey,
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

      {groupsAndMappingsForTables.indicators.allUniqueMappingIds.size > 0 && (
        <DataFileReplacementDifferencesTable
          tableId="replacements-differences-indicators-table"
          itemType="indicator"
          mappingsPlan={planMappings.indicators}
          mappingGroups={groupsAndMappingsForTables.indicators.groupedMappings}
          mappingsToShow={
            groupsAndMappingsForTables.indicators.allUniqueMappingIds
          }
          handleMappingUpdate={handleIndicatorsMappingUpdate}
          rowLabel="label"
          mappedDataLabels={{
            label: 'Label',
            name: 'Name',
          }}
        />
      )}
      {groupsAndMappingsForTables.locations.allUniqueMappingIds.size > 0 && (
        <DataFileReplacementDifferencesTable
          tableId="replacements-differences-locations-table"
          itemType="location"
          mappingsPlan={planMappings.locations}
          mappingGroups={groupsAndMappingsForTables.locations.groupedMappings}
          mappingsToShow={
            groupsAndMappingsForTables.locations.allUniqueMappingIds
          }
          handleMappingUpdate={handleLocationsMappingUpdate}
          rowLabel="name"
          mappedDataLabels={{
            name: 'Name',
            code: 'Code',
          }}
        />
      )}
    </>
  );
}
