import {
  MappingsPlan,
  UpdateMappingPayload,
} from '@admin/services/dataReplacementService';
import ButtonText from '@common/components/ButtonText';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { Dictionary } from '@common/types';
import mapValues from 'lodash/mapValues';
import pickBy from 'lodash/pickBy';
import React, { useMemo } from 'react';
import {
  TableMappingGroup,
  TypeMapping,
} from '@admin/pages/release/data/components/DataFileReplacementDifferencesTable';
import DifferencesItemMappingModal, {
  DifferencesItemMappingModalProps,
  LabelProps,
} from './DataFileReplacementDifferencesMappingModal';

type DifferencesMappingTableRowsProps<
  ItemType extends keyof TypeMapping,
  SourceItemType extends TypeMapping[ItemType]['source'],
> = {
  onUpdate: (payload: UpdateMappingPayload) => Promise<void>;
  itemType: ItemType;
  mappingsPlan: MappingsPlan<SourceItemType>;
  rowLabel: keyof TypeMapping[ItemType]['source'];
  mappedDataLabels: DifferencesItemMappingModalProps<
    ItemType,
    SourceItemType
  >['mappedDataLabels'];
  replacementGroups: Array<TableMappingGroup>;
} & LabelProps<ItemType>;

export default function DifferencesMappingTableRows<
  ItemType extends keyof TypeMapping,
  SourceItemType extends TypeMapping[ItemType]['source'],
>({
  itemType,
  mappingsPlan: { candidates, mappings },
  onUpdate,
  replacementGroups,
  rowLabel,
  mappedDataLabels,
}: DifferencesMappingTableRowsProps<ItemType, SourceItemType>) {
  const {
    allCandidates,
    unmappedCandidates,
  }: {
    allCandidates: Dictionary<SourceItemType>;
    unmappedCandidates: Dictionary<SourceItemType>;
  } = useMemo(() => {
    // add candidate key inside each mapping candidate
    const allCandidatesWithKeys = mapValues(
      candidates,
      (source, candidateKey) => {
        return { candidateKey, ...source };
      },
    );

    // filter out mapping candidates that exist in mappings already
    const unmappedCandidatesWithKeys = pickBy(
      allCandidatesWithKeys,
      ({ candidateKey }) =>
        !Object.values(mappings).some(mapping => {
          return mapping.candidateKey === candidateKey;
        }),
    );

    return {
      allCandidates: allCandidatesWithKeys,
      unmappedCandidates: unmappedCandidatesWithKeys,
    };
  }, [candidates, mappings]);

  return (
    <>
      {replacementGroups.map(group =>
        group.mappings.map((sourceKey, index) => {
          const mapping = mappings[sourceKey];

          const { source, type, candidateKey } = mapping;
          const isUnset = type === 'Unset';
          const sourceLabelText = String(source[rowLabel]);

          const candidateText =
            candidateKey && allCandidates[candidateKey]?.[rowLabel];

          const itemCurrentMapping = candidateText ?? 'No Mapping';

          return (
            <tr key={`mapping-${sourceKey}`}>
              {index === 0 && (
                <th
                  className="govuk-!-width-one-quarter"
                  rowSpan={group.mappings.length}
                >
                  {group.label}
                </th>
              )}
              <td className="govuk-!-width-one-quarter">{sourceLabelText}</td>
              <td>
                {isUnset ? (
                  <Tag colour="red">not present</Tag>
                ) : (
                  `${itemCurrentMapping}`
                )}
              </td>
              <td className="govuk-!-text-align-right">
                {isUnset && (
                  <ButtonText
                    onClick={() =>
                      onUpdate({
                        sourceKey,
                        candidateKey: undefined, // no mapping
                      })
                    }
                  >
                    No mapping{' '}
                    <VisuallyHidden>for {`${sourceLabelText}`}</VisuallyHidden>
                  </ButtonText>
                )}

                <DifferencesItemMappingModal
                  itemType={itemType}
                  allCandidateOptions={allCandidates}
                  unmappedCandidateOptions={unmappedCandidates}
                  mapping={mapping}
                  onSubmit={onUpdate}
                  rowLabel={rowLabel}
                  mappedDataLabels={mappedDataLabels}
                />
              </td>
            </tr>
          );
        }),
      )}
    </>
  );
}
