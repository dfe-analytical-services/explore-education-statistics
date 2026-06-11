import startCase from 'lodash/startCase';
import TagGroup from '@common/components/TagGroup';
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
}

export type TableMappingGroup = {
  label: string;
  mappings: string[];
};

type DataFileDifferencesReplacementTableProps<
  ItemType extends keyof TypeMapping,
> = {
  tableId: string;
  itemType: ItemType;
  handleMappingUpdate: (payload: UpdateMappingPayload) => Promise<void>;
  mappingsPlan: MappingsPlan<TypeMapping[ItemType]['source']>;
  mappingsToShow: Set<string>;
  mappingGroups: Array<TableMappingGroup>;
} & LabelProps<ItemType>;

export default function DataFileDifferencesReplacementTable<
  ItemType extends keyof TypeMapping,
>({
  tableId,
  itemType,
  handleMappingUpdate,
  mappingsPlan,
  mappingGroups,
  mappedDataLabels,
  rowLabel,
  mappingsToShow,
}: DataFileDifferencesReplacementTableProps<ItemType>) {
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
