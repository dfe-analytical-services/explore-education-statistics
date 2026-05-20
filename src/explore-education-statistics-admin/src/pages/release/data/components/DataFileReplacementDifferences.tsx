import dataReplacementService, {
  DataReplacementPlan,
  Mapping,
  MappingsPlan,
  PlanMappings,
  UpdateMappingPayload,
} from '@admin/services/dataReplacementService';
import React, { useCallback, useMemo } from 'react';
import { useImmer } from 'use-immer';
import pickBy from 'lodash/pickBy';
import { Dictionary } from '@common/types';
import DataFileReplacementTable from './DataFileReplacementTable';

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

    function uniqueByLabel<T extends WithLabel>(items: T[]): T[] {
      return Array.from(
        new Map(items.map(item => [item.label, item])).values(),
      );
    }

    function createGroups<
      GroupType extends WithLabel,
      ChildKey extends keyof GroupType,
      ChildType extends GroupType[ChildKey] extends readonly (infer C)[]
        ? C
        : never,
      TargetIdentifier extends keyof ChildType,
    >(
      mappingsToShow: Dictionary<Mapping<unknown>>,
      allGroups: GroupType[],
      childKey: ChildKey,
      targetIdentifier: TargetIdentifier &
        (ChildType[TargetIdentifier] extends string ? TargetIdentifier : never),
    ): GroupType[] {
      const uniqueGroups = uniqueByLabel(allGroups);

      return uniqueGroups.filter(group => {
        const children = (group[childKey] ?? []) as ChildType[];

        return children.some(child => {
          const key = child[targetIdentifier] as unknown as string;
          return Boolean(mappingsToShow[key]);
        });
      });
    }

    const indicatorsToShow = getMappingsToShow(
      plan.mapping.indicators.mappings,
    );

    const allIndicatorGroups = [
      ...plan.dataBlocks.flatMap(block =>
        Object.values(block.indicatorGroups || {}),
      ),
      ...plan.footnotes.flatMap(footnote =>
        Object.values(footnote.indicatorGroups || {}),
      ),
    ];

    const res = createGroups(
      indicatorsToShow,
      allIndicatorGroups,
      'indicators',
      'name',
    );

    const allLocations = plan.dataBlocks.flatMap(block =>
      Object.values(block.locations || {}),
    );

    const locationsToShow = getMappingsToShow(plan.mapping.locations.mappings);
    const loc = createGroups(
      locationsToShow,
      allLocations,
      'locationAttributes',
      'id',
    );

    return { indicators: res, locations: loc };
  }, [plan.dataBlocks, plan.footnotes]);

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
            originalColumnName: sourceKey,
            newReplacementColumnName: candidateKey,
          },
        ],
      );

      reloadPlan();
    },
    [
      updatePlanMappings,
      fileId,
      releaseVersionId,
      reloadPlan,
      replacementFileId,
    ],
  );

  console.log(plan);

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
        mappingGroups={referencedData.indicators}
        handleIndicatorsMappingUpdate={handleIndicatorsMappingUpdate}
        replacementGroupKey="indicators"
        label="label"
        targetReplacementKey="name"
      />
      <DataFileReplacementTable
        tableId="replacements-differences-locations-table"
        itemType="location"
        mappingsPlan={planMappings.locations}
        mappingGroups={referencedData.locations}
        handleIndicatorsMappingUpdate={handleIndicatorsMappingUpdate}
        replacementGroupKey="locationAttributes"
        label="code"
        targetReplacementKey="id"
      />
    </>
  );
}
