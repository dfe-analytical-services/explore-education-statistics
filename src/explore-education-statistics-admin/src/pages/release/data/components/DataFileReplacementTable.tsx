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
import { KeysWithType } from '@common/types';
import DifferencesMappingTableRows from '@admin/pages/release/data/components/DataFileReplacementDifferencesTableRows';
import DataFileReplacementMappingCountsTag from '@admin/pages/release/data/components/DataFileReplacementMappingCountsTag';

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

export default function DataFileReplacementTable<
  ItemType extends keyof TypeMapping,
>({
  tableId,
  itemType,
  handleIndicatorsMappingUpdate,
  mappingsPlan,
  mappingGroups,
  mappedDataLabels,
  mappingsToShow,
}: {
  tableId: string;
  itemType: ItemType;
  handleIndicatorsMappingUpdate: (
    payload: UpdateMappingPayload,
  ) => Promise<void>;
  mappingsPlan: MappingsPlan<TypeMapping[ItemType]['source']>;
  mappingsToShow: string[];
  mappingGroups: Array<TableMappingGroup>;
  mappedDataLabels: KeysWithType<TypeMapping[ItemType]['source'], string>[];
}) {
  const mappingCounts: {
    mapped: number;
    unmapped: number;
  } = useMemo(() => {
    const totalMappingCount = mappingsToShow.length;

    const manualMappedCount = mappingsToShow.filter(
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
        className="dfe-table--vertical-align-middle dfe-table--row-highlights"
        id={tableId}
        data-testid={tableId}
      >
        <caption className="govuk-!-margin-bottom-3 govuk-!-font-size-24">
          <span key={itemType}>
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
            mappedDataLabels={mappedDataLabels}
            replacementGroups={mappingGroups}
          />
        </tbody>
      </table>
    </div>
  );
}
