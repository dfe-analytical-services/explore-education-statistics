import {
  MappingsPlan,
  MappingWithKey,
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
import { TypeMapping } from '@admin/pages/release/data/components/DataFileReplacementTable';
import DifferencesItemMappingModal from './DataFileReplacementDifferencesMappingModal';
import { SourceMappingType } from './DataFileReplacementDifferences';

export default function DifferencesMappingTableRows<
  ItemType extends SourceMappingType,
  SourceItemType = TypeMapping[ItemType]['source'],
  ReplacementGroupType extends
    TypeMapping[ItemType]['group'] = TypeMapping[ItemType]['group'],
  TargetReplacementType extends
    TypeMapping[ItemType]['target'] = TypeMapping[ItemType]['target'],
>({
  itemType,
  mappingsPlan: { candidates, mappings },
  onUpdate,
  label,
  replacementGroups,
  replacementGroupKey,
  targetReplacementKey,
}: {
  onUpdate: (payload: UpdateMappingPayload) => Promise<void>;
  itemType: ItemType;
  mappingsPlan: MappingsPlan<SourceItemType>;
  label: KeysWithType<SourceItemType, string>;
  replacementGroups: Array<ReplacementGroupType>;
  targetReplacementKey: KeysWithType<TargetReplacementType, string>;
  replacementGroupKey: KeysWithType<
    ReplacementGroupType,
    TargetReplacementType[]
  >;
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
        (group[replacementGroupKey] as TargetReplacementType[]).map(
          (targetReplacement, index) => {
            const sourceKey = targetReplacement[targetReplacementKey] as string;

            const mapping = mappings[sourceKey];

            const { source, type } = mapping;
            const isUnset = type === 'Unset';
            const itemCurrentMapping =
              (mapping.candidateKey &&
                String(allCandidates[mapping.candidateKey]?.[label])) ??
              'No mapping';

            return (
              <tr
                key={`mapping-${sourceKey}`}
                className={classNames({
                  'rowHighlight--alert': isUnset,
                })}
              >
                <td>{index === 0 ? group.label : ''}</td>
                <td>{targetReplacement.label}</td>
                <td>
                  {isUnset ? (
                    <Tag colour="red">not present</Tag>
                  ) : (
                    `${itemCurrentMapping}`
                  )}
                </td>
                <td className="govuk-!-text-align-right">
                  {mapping.type === 'Unset' && (
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
                        for {`${mapping.source[label]}`}
                      </VisuallyHidden>
                    </ButtonText>
                  )}

                  <DifferencesItemMappingModal
                    itemType={itemType}
                    allCandidateOptions={allCandidates}
                    unmappedCandidateOptions={unmappedCandidates}
                    mapping={{ ...mapping, sourceKey }}
                    onSubmit={onUpdate}
                    label={label}
                  />
                </td>
              </tr>
            );
          },
        ),
      )}
    </>
  );
}
