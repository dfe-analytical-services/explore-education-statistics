import startCase from 'lodash/startCase';
import TagGroup from '@common/components/TagGroup';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React, { useMemo } from 'react';
import {
  FilterMappingSource,
  IndicatorGroupReplacement,
  IndicatorReplacement,
  IndicatorSource,
  LocationAttributeReplacement,
  LocationReplacement,
  LocationSource,
  MappingsPlan,
  ReplacementMapping,
  UpdateMappingPayload,
} from '@admin/services/dataReplacementService';
import DifferencesMappingTableRows from '@admin/pages/release/data/components/DataFileReplacementDifferencesTableRows';
import DataFileReplacementMappingCountsTag from '@admin/pages/release/data/components/DataFileReplacementMappingCountsTag';
import { LabelProps } from './DataFileReplacementDifferencesMappingModal';

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
  filter: {
    source: FilterMappingSource;
  };
}

export type TableMappingGroup = {
  label: string;
  mappings: string[];
};

export function uniqueByLabel<T extends { label: string }>(items: T[]): T[] {
  return Array.from(new Map(items.map(item => [item.label, item])).values());
}

type TableItemType = 'indicator' | 'location';

type DataFileDifferencesReplacementTableProps<ItemType extends TableItemType> =
  {
    tableId: string;
    itemType: ItemType;
    handleMappingUpdate: (payload: UpdateMappingPayload) => Promise<void>;
    mappingsPlan: MappingsPlan<TypeMapping[ItemType]['source']>;
    replacementGroups: Array<TypeMapping[ItemType]['group']>;
    getGroupMappings: (
      group: TypeMapping[ItemType]['group'],
    ) => Array<TypeMapping[ItemType]['target']>;
  } & LabelProps<ItemType>;

export default function DataFileDifferencesReplacementTable<
  ItemType extends TableItemType,
>({
  tableId,
  itemType,
  handleMappingUpdate,
  mappingsPlan,
  replacementGroups,
  getGroupMappings,
  mappedDataLabels,
  rowLabel,
}: DataFileDifferencesReplacementTableProps<ItemType>) {
  const { mappingGroups, mappingsToShow } = useMemo(() => {
    const mappings = mappingsPlan.mappings as Record<
      string,
      ReplacementMapping<TypeMapping[ItemType]['source']>
    >;
    const mappingIds = new Set<string>();

    const groups = uniqueByLabel(replacementGroups).flatMap(group => {
      const groupMappingIds = getGroupMappings(group)
        .map(mapping => mapping.id)
        .filter(id => mappings[id]?.type !== 'AutoSet' && mappings[id]);

      groupMappingIds.forEach(id => mappingIds.add(id));

      return groupMappingIds.length > 0
        ? [{ label: group.label, mappings: groupMappingIds }]
        : [];
    });

    return { mappingGroups: groups, mappingsToShow: mappingIds };
  }, [getGroupMappings, mappingsPlan.mappings, replacementGroups]);

  const mappingCounts: {
    mapped: number;
    unmapped: number;
  } = useMemo(() => {
    const totalMappingCount = mappingsToShow.size;

    const manualMappedCount = Array.from(mappingsToShow).filter(
      target => mappingsPlan.mappings[target]?.type === 'ManuallySet',
    ).length;

    const unmappedCount = totalMappingCount - manualMappedCount;

    return {
      mapped: manualMappedCount,
      unmapped: unmappedCount,
    };
  }, [mappingsPlan.mappings, mappingsToShow]);

  if (mappingsToShow.size === 0) {
    return null;
  }

  return (
    <div className="table-container">
      <table
        className="dfe-table--vertical-align-middle "
        id={tableId}
        data-testid={tableId}
      >
        <caption className="govuk-!-margin-bottom-3 govuk-!-font-size-24">
          {`${startCase(itemType)}s`}
          <TagGroup className="govuk-!-margin-left-2">
            <DataFileReplacementMappingCountsTag
              mappingType={itemType}
              countType="unmapped"
              count={mappingCounts.unmapped}
            />
            <DataFileReplacementMappingCountsTag
              mappingType={itemType}
              countType="mapped"
              count={mappingCounts.mapped}
            />
          </TagGroup>
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
            onUpdate={handleMappingUpdate}
            mappedDataLabels={mappedDataLabels}
            rowLabel={rowLabel}
            replacementGroups={mappingGroups}
          />
        </tbody>
      </table>
    </div>
  );
}
