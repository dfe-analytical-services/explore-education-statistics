import {
  FilterMappingFilterGroups,
  FilterMappingFilterItems,
  FilterMappingPlan,
  FilterMappingSource,
  MappingType,
  ReplacementMapping,
  UpdateMappingPayload,
} from '@admin/services/dataReplacementService';
import React, { useMemo } from 'react';
import { Dictionary } from '@common/types';
import Tag from '@common/components/Tag';
import ButtonText from '@common/components/ButtonText';
import VisuallyHidden from '@common/components/VisuallyHidden';
import DifferencesItemMappingModal from '@admin/pages/release/data/components/DataFileReplacementDifferencesMappingModal';
import TagGroup from '@common/components/TagGroup';
import DataFileReplacementMappingCountsTag from '@admin/pages/release/data/components/DataFileReplacementMappingCountsTag';

export type TableHeaderCell = {
  id: string;
  label: string;
  rowSpan: number;
};

export type TableRowData = {
  headers: TableHeaderCell[];
  mappingToEdit: ReplacementMapping<FilterMappingSource>;
  allCandidates?: Dictionary<FilterMappingSource>;
  unmappedCandidates: Dictionary<FilterMappingSource>;
  level: number;
};

export type FilterTableData = {
  rows: TableRowData[];
  mappingCounts: {
    mapped: number;
    unmapped: number;
    unmappedParents: number;
  };
};

const CAN_MAP_VALUES: MappingType[] = ['Unset', 'ManuallySet'] as const;
const CAN_SHOW_CHILDREN: MappingType[] = ['AutoSet', 'ManuallySet'] as const;

type MappingsType =
  | FilterMappingPlan
  | FilterMappingFilterGroups
  | FilterMappingFilterItems<FilterMappingSource>;

/**
 * This will generate a flat array of rows for the table, with each item specifying how many rows it covers, so it
 * can encompass all the ancestor rows (i.e. row-span).
 *
 * If the mapping doesn't have a valid mapping, it won't traverse down.
 */
const buildFilterTableData = (filters: FilterMappingPlan) => {
  const buildRows = (
    currentFilters: MappingsType,
    level = 0,
  ): FilterTableData => {
    const { candidates, mappings } = currentFilters;

    const mappedCandidateKeys = new Set(
      Object.values(mappings)
        .map(mapping => mapping.candidateKey)
        .filter(Boolean),
    );

    const unmappedCandidates = Object.fromEntries(
      Object.entries(candidates).filter(
        ([candidateId]) => !mappedCandidateKeys.has(candidateId),
      ),
    );

    const allCandidates =
      Object.keys(candidates).length > 0 ? candidates : undefined;

    return Object.entries(mappings).reduce<FilterTableData>(
      (result, [mappingId, mapping]) => {
        const replacementMapping =
          mapping as ReplacementMapping<FilterMappingSource>;

        const selfCanBeMapped = CAN_MAP_VALUES.includes(
          replacementMapping.type,
        );
        const canShowChildren = CAN_SHOW_CHILDREN.includes(
          replacementMapping.type,
        );

        let childMappings: MappingsType | undefined;

        if ('filterGroups' in mapping) {
          childMappings = mapping.filterGroups as FilterMappingFilterGroups;
        } else if ('filterItems' in mapping) {
          childMappings =
            mapping.filterItems as FilterMappingFilterItems<FilterMappingSource>;
        }

        const childData = childMappings
          ? buildRows(childMappings, level + 1)
          : {
              rows: [],
              mappingCounts: {
                mapped: 0,
                unmapped: 0,
                unmappedParents: 0,
              },
            };

        const selfCounts = {
          mapped: replacementMapping.type === 'ManuallySet' ? 1 : 0,
          unmapped: replacementMapping.type === 'Unset' ? 1 : 0,
          unmappedParents:
            replacementMapping.type === 'ParentNotMapped' ? 1 : 0,
        };

        const childRows = canShowChildren ? childData.rows : [];

        // add this mapping first if it can be mapped
        const rowsForMapping: TableRowData[] = [
          ...(selfCanBeMapped
            ? [
                {
                  headers: [],
                  mappingToEdit: replacementMapping,
                  level,
                  allCandidates,
                  unmappedCandidates,
                },
              ]
            : []),
          ...childRows,
        ];

        if (rowsForMapping.length > 0) {
          rowsForMapping[0] = {
            ...rowsForMapping[0],
            headers: [
              {
                id: mappingId,
                label: replacementMapping.source.label,
                rowSpan: rowsForMapping.length,
              },
              ...rowsForMapping[0].headers,
            ],
          };
        }

        return {
          rows: [...result.rows, ...rowsForMapping],
          mappingCounts: {
            mapped:
              result.mappingCounts.mapped +
              selfCounts.mapped +
              childData.mappingCounts.mapped,
            unmapped:
              result.mappingCounts.unmapped +
              selfCounts.unmapped +
              childData.mappingCounts.unmapped,
            unmappedParents:
              result.mappingCounts.unmappedParents +
              selfCounts.unmappedParents +
              childData.mappingCounts.unmappedParents,
          },
        };
      },
      {
        rows: [],
        mappingCounts: {
          mapped: 0,
          unmapped: 0,
          unmappedParents: 0,
        },
      },
    );
  };
  return buildRows(filters);
};

type DataFileDifferencesReplacementTableProps = {
  handleMappingUpdate: (
    payload: UpdateMappingPayload,
    type: string,
  ) => Promise<void>;
  filters?: FilterMappingPlan;
};

export default function DataFileReplacementFilterDifferencesTable({
  filters,
  handleMappingUpdate,
}: DataFileDifferencesReplacementTableProps) {
  const tableData = useMemo(
    () => (filters ? buildFilterTableData(filters) : undefined),
    [filters],
  );

  const outputHeaders = (row: TableRowData) => {
    const rowLabel = row.mappingToEdit.source.id;
    return row.headers.map(header =>
      rowLabel === header.id && header.rowSpan === 1 ? (
        <td key={header.id} rowSpan={header.rowSpan}>
          {header.label}
        </td>
      ) : (
        <th key={header.id} rowSpan={header.rowSpan} style={{ width: '10%' }}>
          {header.label}
        </th>
      ),
    );
  };

  if (tableData === undefined || tableData.rows.length === 0) return null;

  return (
    <div className="table-container">
      <table className="dfe-table--vertical-align-middle ">
        <caption className="govuk-!-margin-bottom-3 govuk-!-font-size-24">
          Filters
          <TagGroup className="govuk-!-margin-left-2">
            <DataFileReplacementMappingCountsTag
              mappingType="filter"
              countType="unmapped"
              count={
                tableData.mappingCounts.unmapped +
                tableData.mappingCounts.unmappedParents
              }
            />
            {tableData.mappingCounts.unmappedParents > 0 && (
              <Tag colour="red">
                {tableData.mappingCounts.unmappedParents} not shown
              </Tag>
            )}
            <DataFileReplacementMappingCountsTag
              mappingType="filter"
              countType="mapped"
              count={tableData.mappingCounts.mapped}
            />
          </TagGroup>
        </caption>

        <tbody>
          {tableData.rows.map(td => {
            const mapping = td.mappingToEdit;
            const { source, type, candidateKey } = td.mappingToEdit;

            const isUnset = type === 'Unset';
            const sourceLabelText = source.label;

            const candidateText =
              candidateKey && td.allCandidates?.[candidateKey]?.label;
            const itemCurrentMapping = candidateText ?? 'No Mapping';

            let payloadType: string = 'filter';
            if (td.level === 2) {
              payloadType = 'item';
            } else if (td.level === 1) {
              payloadType = 'group';
            }

            return (
              <tr key={td.mappingToEdit.source.id}>
                {outputHeaders(td)}

                {td.level < 2 && <td colSpan={2 - td.level} />}

                <td className="govuk-!-width-one-quarter">
                  {isUnset ? (
                    <Tag colour="red">not present</Tag>
                  ) : (
                    `${itemCurrentMapping}`
                  )}
                </td>

                <td className="govuk-!-text-align-right govuk-!-width-one-quarter">
                  {isUnset && (
                    <ButtonText
                      onClick={() =>
                        handleMappingUpdate(
                          { sourceKey: source.id, candidateKey: undefined },
                          payloadType,
                        )
                      }
                    >
                      No mapping{' '}
                      <VisuallyHidden>
                        for {`${sourceLabelText}`}
                      </VisuallyHidden>
                    </ButtonText>
                  )}

                  {td.allCandidates && (
                    <DifferencesItemMappingModal
                      itemType="filter"
                      allCandidateOptions={td.allCandidates}
                      unmappedCandidateOptions={td.unmappedCandidates}
                      mapping={mapping}
                      onSubmit={async payload => {
                        await handleMappingUpdate(payload, payloadType);
                      }}
                      rowLabel="label"
                      mappedDataLabels={{ label: 'Label' }}
                    />
                  )}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
