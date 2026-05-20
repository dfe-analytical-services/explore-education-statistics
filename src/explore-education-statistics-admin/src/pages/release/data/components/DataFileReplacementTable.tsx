import startCase from 'lodash/startCase';
import TagGroup from '@common/components/TagGroup';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React, { useMemo } from 'react';
import {
  IndicatorGroupReplacement,
  IndicatorReplacement,
  IndicatorSource,
  LocationAttributeReplacement,
  LocationReplacement,
  LocationSource,
  MappingsPlan,
  PlanMappings,
  SourceItem,
  UpdateMappingPayload,
} from '@admin/services/dataReplacementService';
import { Dictionary, KeysWithType } from '@common/types';
import mapValues from 'lodash/mapValues';
import pickBy from 'lodash/pickBy';
import { SourceMappingType } from '@admin/pages/release/data/components/DataFileReplacementDifferences';
import DifferencesMappingTableRows from '@admin/pages/release/data/components/DataFileReplacementDifferencesTableRows';

const TypeName = React.memo(function GetTypeName({
  mappingType,
  capitalise = false,
  plural = true,
}: {
  mappingType: SourceMappingType;
  capitalise?: boolean;
  plural?: boolean;
}) {
  let name = `${mappingType}${plural ? 's' : ''}`;
  name = capitalise ? startCase(name) : name;

  return <>{name}</>;
});

function MappingCountsTag({
  mappingType,
  countType,
  count,
}: {
  mappingType: SourceMappingType;
  countType: 'mapped' | 'unmapped';
  count: number;
}) {
  return count === 0 ? null : (
    <Tag colour={countType === 'mapped' ? 'blue' : 'red'}>
      {`${count} ${countType} `}
      <TypeName
        mappingType={mappingType}
        capitalise={false}
        plural={count > 1}
      />
    </Tag>
  );
}

export interface TypeMapping {
  indicator: {
    source: IndicatorSource;
    group: IndicatorGroupReplacement;
    target: IndicatorReplacement;
  };
  location: {
    source: LocationSource;
    group: LocationReplacement;
    target: LocationAttributeReplacement;
  };
}

export default function DataFileReplacementTable<
  ItemType extends keyof TypeMapping,
  ReplacementGroupType extends
    TypeMapping[ItemType]['group'] = TypeMapping[ItemType]['group'],
  TargetReplacementType extends
    TypeMapping[ItemType]['target'] = TypeMapping[ItemType]['target'],
>({
  tableId,
  itemType,
  handleIndicatorsMappingUpdate,
  mappingsPlan,
  mappingGroups,
  label,
  replacementGroupKey,
  targetReplacementKey,
}: {
  tableId: string;
  itemType: ItemType;
  handleIndicatorsMappingUpdate: (
    payload: UpdateMappingPayload,
  ) => Promise<void>;
  mappingsPlan: MappingsPlan<TypeMapping[ItemType]['source']>;
  mappingGroups: Array<ReplacementGroupType>;
  label: KeysWithType<TypeMapping[ItemType]['source'], string>;
  targetReplacementKey: KeysWithType<TargetReplacementType, string>;
  replacementGroupKey: KeysWithType<
    ReplacementGroupType,
    TargetReplacementType[]
  >;
}) {
  const filteredGroups = useMemo(() => {
    // mapping groups are the ones we built and are the the ones we want to show
    // filteredMappingsWithSource filters out ones that have AutoSet to true
    return mappingGroups
      .map(group => {
        const targets = group[replacementGroupKey] as TargetReplacementType[];

        const newTargets = targets.filter(target => {
          const k = target[targetReplacementKey] as string;
          return mappingsPlan.mappings[k]?.type !== 'AutoSet';
        });

        if (newTargets.length === 0) return undefined;

        if (newTargets.length === targets.length) return group;

        return {
          ...group,
          [replacementGroupKey]: newTargets,
        };
      })
      .filter(group => group !== undefined);
  }, [mappingsPlan, mappingGroups, replacementGroupKey]);

  const mappingCounts: {
    mapped: number;
    unmapped: number;
  } = useMemo(() => {
    const allTargets = Object.values(filteredGroups).flatMap(
      group => group[replacementGroupKey] as TargetReplacementType[],
    );

    const totalMappingCount = allTargets.length;

    const autoMappedCount = allTargets.filter(
      target =>
        mappingsPlan.mappings[target[targetReplacementKey] as string]?.type ===
        'AutoSet',
    ).length;

    const manualMappedCount = allTargets.filter(
      target =>
        mappingsPlan.mappings[target[targetReplacementKey] as string]?.type ===
        'ManuallySet',
    ).length;

    const unmappedCount =
      totalMappingCount - autoMappedCount - manualMappedCount;

    return {
      mapped: manualMappedCount,
      unmapped: unmappedCount,
    };
  }, [filteredGroups, mappingsPlan, replacementGroupKey, targetReplacementKey]);

  return (
    <div className="table-container">
      <table
        className="dfe-table--vertical-align-middle dfe-table--row-highlights"
        id={tableId}
        data-testid={tableId}
      >
        <caption className="govuk-!-margin-bottom-3 govuk-!-font-size-24">
          <span key={itemType}>
            <TypeName mappingType={itemType} capitalise plural />{' '}
            <TagGroup className="govuk-!-margin-left-2">
              <MappingCountsTag
                mappingType={itemType}
                countType="unmapped"
                count={mappingCounts.unmapped}
              />
              <MappingCountsTag
                mappingType={itemType}
                countType="mapped"
                count={mappingCounts.mapped}
              />
            </TagGroup>
          </span>
        </caption>
        <thead>
          <VisuallyHidden as="tr">
            <th className="govuk-!-width-one-quarter">Original Group</th>
            <th className="govuk-!-width-one-quarter">Original Item</th>
            <th className="govuk-!-width-one-quarter">Mapping</th>
            <th className="govuk-!-text-align-right">Actions</th>
          </VisuallyHidden>
        </thead>
        <tbody data-testid={`${tableId}-body`}>
          <DifferencesMappingTableRows
            itemType={itemType}
            mappingsPlan={mappingsPlan}
            onUpdate={handleIndicatorsMappingUpdate}
            label={label}
            replacementGroups={filteredGroups}
            replacementGroupKey={replacementGroupKey}
            targetReplacementKey={targetReplacementKey}
          />
        </tbody>
      </table>
    </div>
  );
}
