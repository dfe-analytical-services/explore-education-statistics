import {
  IndicatorMappingWithKey,
  IndicatorsMappingsPlan,
  SourceItem,
  UpdateMappingPayload,
} from '@admin/services/dataReplacementService';
import ButtonText from '@common/components/ButtonText';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import mapValues from 'lodash/mapValues';
import pickBy from 'lodash/pickBy';
import React, { useMemo } from 'react';
import DifferencesItemMappingModal from './DataFileReplacementDifferencesMappingModal';

interface RowsProps {
  mappingKeysToShow: Set<string>;
  itemType: 'indicator' | 'filter' | 'location';
  mappings: IndicatorsMappingsPlan;
  onUpdate: (payload: UpdateMappingPayload) => Promise<void>;
}

export default function DifferencesMappingTableRows({
  mappingKeysToShow,
  itemType,
  mappings: { candidates, mappings },
  onUpdate,
}: RowsProps) {
  const {
    allCandidates,
    unmappedCandidates,
  }: {
    allCandidates: Dictionary<SourceItem>;
    unmappedCandidates: Dictionary<SourceItem>;
  } = useMemo(() => {
    // add candidate key inside each mapping candidate
    const allCandidatesWithKeys = mapValues(
      candidates,
      ({ label }, candidateKey) => {
        return { candidateKey, label };
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

  const mappingsToList: IndicatorMappingWithKey[] = Object.entries(mappings)
    .map(([sourceKey, mapping]) => ({ ...mapping, sourceKey }))
    .filter(({ type }) => type !== 'AutoSet');

  return (
    <>
      {mappingsToList
        .filter(({ sourceKey }) => mappingKeysToShow.has(sourceKey))
        .map(mapping => {
          const { source, sourceKey, type } = mapping;
          const isUnset = type === 'Unset';
          const itemCurrentMapping =
            (mapping.candidateKey &&
              allCandidates[mapping.candidateKey]?.label) ??
            'No mapping';

          return (
            <tr
              key={`mapping-${sourceKey}`}
              className={classNames({
                'rowHighlight--alert': isUnset,
              })}
            >
              <td>{source.label}</td>
              <td>
                {isUnset ? (
                  <Tag colour="red">not present</Tag>
                ) : (
                  itemCurrentMapping
                )}
              </td>
              <td className="govuk-!-text-align-right">
                {mapping.type === 'Unset' && (
                  <ButtonText
                    onClick={() =>
                      onUpdate({
                        sourceKey: mapping.sourceKey,
                        candidateKey: undefined, // no mapping
                      })
                    }
                  >
                    No mapping{' '}
                    <VisuallyHidden>for {mapping.source.label}</VisuallyHidden>
                  </ButtonText>
                )}

                <DifferencesItemMappingModal
                  itemType={itemType}
                  allCandidateOptions={allCandidates}
                  unmappedCandidateOptions={unmappedCandidates}
                  mapping={mapping}
                  onSubmit={onUpdate}
                />
              </td>
            </tr>
          );
        })}
    </>
  );
}
