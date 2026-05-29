import {
  MappingsPlan,
  UpdateMappingPayload,
} from '@admin/services/dataReplacementService';
import ButtonText from '@common/components/ButtonText';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { Dictionary, KeysWithType } from '@common/types';
import classNames from 'classnames';
import mapValues from 'lodash/mapValues';
import pickBy from 'lodash/pickBy';
import React, { useMemo } from 'react';
import {
  TableMappingGroup,
  TypeMapping,
} from '@admin/pages/release/data/components/DataFileReplacementTable';
import DifferencesItemMappingModal from './DataFileReplacementDifferencesMappingModal';
import { SourceMappingType } from './DataFileReplacementDifferences';

export default function DifferencesMappingTableRows<
  ItemType extends SourceMappingType,
  SourceItemType extends TypeMapping[ItemType]['source'],
>({
  itemType,
  mappingsPlan: { candidates, mappings },
  onUpdate,
  replacementGroups,
  mappedDataLabels,
}: {
  onUpdate: (payload: UpdateMappingPayload) => Promise<void>;
  itemType: ItemType;
  mappingsPlan: MappingsPlan<SourceItemType>;
  mappedDataLabels: KeysWithType<SourceItemType, string>[];
  replacementGroups: Array<TableMappingGroup>;
}) {
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
          const itemCurrentMapping =
            (candidateKey &&
              String(
                allCandidates[candidateKey]?.[mappedDataLabels[0]] ?? '',
              )) ??
            'No mapping';

          return (
            <tr
              key={`mapping-${sourceKey}`}
              className={classNames({
                'rowHighlight--alert': isUnset,
              })}
            >
              <td className="govuk-!-width-one-quarter">
                {index === 0 ? group.label : ''}
              </td>
              <td className="govuk-!-width-one-quarter">
                {String(source[mappedDataLabels[0]])}
              </td>
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
                    <VisuallyHidden>
                      for {`${String(source[mappedDataLabels[0]])}`}
                    </VisuallyHidden>
                  </ButtonText>
                )}

                <DifferencesItemMappingModal
                  itemType={itemType}
                  allCandidateOptions={allCandidates}
                  unmappedCandidateOptions={unmappedCandidates}
                  mapping={mapping}
                  onSubmit={onUpdate}
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
