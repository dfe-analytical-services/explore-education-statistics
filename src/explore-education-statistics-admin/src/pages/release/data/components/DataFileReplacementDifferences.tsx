import dataReplacementService, {
  DataReplacementPlan,
  IndicatorGroupReplacement,
  LocationReplacement,
  Mapping,
  MappingsPlan,
  PlanMappings,
  TargetReplacement,
  UpdateMappingPayload,
} from '@admin/services/dataReplacementService';
import React, { useCallback, useMemo } from 'react';
import { useImmer } from 'use-immer';
import pickBy from 'lodash/pickBy';
import { Dictionary } from '@common/types';
import DataFileReplacementTable, {
  TableMappingGroup,
} from './DataFileReplacementTable';

interface Props {
  releaseVersionId: string;
  fileId: string;
  replacementFileId: string;
  plan: DataReplacementPlan;
  reloadPlan: () => void;
}

export type SourceMappingType = 'indicator' | 'location';
export default function DataFileReplacementDifferences({
  releaseVersionId,
  fileId,
  replacementFileId,
  plan,
  reloadPlan,
}: Props) {
  const referencedData = useMemo(() => {
    const getMappingsToShow = <T,>(mappings: MappingsPlan<T>['mappings']) => {
      return pickBy(mappings, value => value.type !== 'AutoSet');
    };

    type WithLabel = { label: string };
    type GroupType = IndicatorGroupReplacement | LocationReplacement;

    function uniqueByLabel<T extends WithLabel>(items: T[]): T[] {
      return Array.from(
        new Map(items.map(item => [item.label, item])).values(),
      );
    }

    function createGroupsOfMappingIds<
      G extends GroupType,
      R extends G[keyof G] extends TargetReplacement ? G[keyof G] : never,
    >(
      mappingsToShow: Dictionary<Mapping<unknown>>,
      allGroups: G[],
      childKey: keyof G,
    ) {
      const uniqueGroups = uniqueByLabel(allGroups);

      return uniqueGroups.reduce<TableMappingGroup[]>(
        (groupOfMapping, group) => {
          const children = (group[childKey] ?? []) as R[];

          const filteredChildren = children.filter(
            child => mappingsToShow[child.id],
          );
          if (filteredChildren.length === 0) return groupOfMapping;

          return [
            ...groupOfMapping,
            {
              label: group.label,
              mappings: filteredChildren.map(child => child.id),
            },
          ];
        },
        [] as TableMappingGroup[],
      );
    }

    const indicatorsToShow = getMappingsToShow(
      plan.mapping.indicators.mappings,
    );

    const indicatorsToShowIds = Object.keys(indicatorsToShow);

    const allIndicatorGroups = [
      ...plan.dataBlocks.flatMap(block =>
        Object.values(block.indicatorGroups || {}),
      ),
      ...plan.footnotes.flatMap(footnote =>
        Object.values(footnote.indicatorGroups || {}),
      ),
    ];

    const groupedIndicatorMappings = createGroupsOfMappingIds(
      indicatorsToShow,
      allIndicatorGroups,
      'indicators',
    );

    const allLocations = plan.dataBlocks.flatMap(block =>
      Object.values(block.locations || {}),
    );

    const locationsToShow = getMappingsToShow(plan.mapping.locations.mappings);

    const locationsToShowIds = Object.keys(locationsToShow);

    const groupedLocationMappings = createGroupsOfMappingIds(
      locationsToShow,
      allLocations,
      'locationAttributes',
    );

    return {
      indicators: {
        all: indicatorsToShowIds,
        grouped: groupedIndicatorMappings,
      },
      locations: { all: locationsToShowIds, grouped: groupedLocationMappings },
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

      <DataFileReplacementTable
        tableId="replacements-differences-indicators-table"
        itemType="indicator"
        mappingsPlan={planMappings.indicators}
        mappingGroups={referencedData.indicators.grouped}
        mappingsToShow={referencedData.indicators.all}
        handleIndicatorsMappingUpdate={handleIndicatorsMappingUpdate}
        mappedDataLabels={['label', 'name']}
      />
      <DataFileReplacementTable
        tableId="replacements-differences-locations-table"
        itemType="location"
        mappingsPlan={planMappings.locations}
        mappingGroups={referencedData.locations.grouped}
        mappingsToShow={referencedData.locations.all}
        handleIndicatorsMappingUpdate={handleLocationsMappingUpdate}
        mappedDataLabels={['name', 'code']}
      />
    </>
  );
}
