import { SourceItem } from '@admin/services/apiDataSetVersionService';
import {
  IndicatorMappingWithKey,
  IndicatorsMappingsPlan,
  UpdateMappingPayload,
  UpdateMappingPayloadMultiple,
} from '@admin/services/dataReplacementService';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import mapValues from 'lodash/mapValues';
import pickBy from 'lodash/pickBy';
import React, { useCallback, useMemo, useState } from 'react';
import DifferencesItemMappingModal from './DataFileReplacementDifferencesMappingModal';

interface RowsProps {
  itemType: 'indicator' | 'filter' | 'location';
  mappings: IndicatorsMappingsPlan;
  onUpdate: (payload: UpdateMappingPayloadMultiple) => Promise<void>;
}

export default function DifferencesMappingTableRows({
  itemType,
  mappings: { candidates, mappings },
  onUpdate,
}: RowsProps) {
  const [currentMappingItem, setCurrentMappingItem] = useState<
    IndicatorMappingWithKey | undefined
  >();
  const [loadingMappings, setLoadingMappings] = useState<
    Set<UpdateMappingPayload['sourceKey']>
  >(new Set());

  const handleModalMappingSubmit = useCallback(
    (payload: UpdateMappingPayloadMultiple) => {
      const updateMappingKeys = payload.map(({ sourceKey }) => sourceKey);
      setLoadingMappings(prev => {
        const next = new Set(prev);
        updateMappingKeys.forEach(key => next.add(key));
        return next;
      });

      onUpdate(payload).finally(() =>
        setLoadingMappings(prev => {
          const next = new Set(prev);
          updateMappingKeys.forEach(key => next.delete(key));
          return next;
        }),
      );
    },
    [],
  );

  const {
    allCandidates,
    unmappedCandidates,
  }: {
    allCandidates: Dictionary<SourceItem>;
    unmappedCandidates: Dictionary<SourceItem>;
  } = useMemo(() => {
    // add key inside to each item
    const allCandidatesWithKeys = mapValues(
      candidates,
      ({ label }, candidateKey) => ({ candidateKey, label }),
    );

    const unmappedCandidatesWithKeys = pickBy(
      allCandidatesWithKeys,
      ({ candidateKey }) =>
        // filter out candidates that exist in mappings
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
      {currentMappingItem && (
        <DifferencesItemMappingModal
          itemType={itemType}
          allCandidateOptions={allCandidates}
          unmappedCandidateOptions={unmappedCandidates}
          currentItem={currentMappingItem}
          onSubmit={handleModalMappingSubmit}
          onClose={() => setCurrentMappingItem(undefined)}
        />
      )}

      {mappingsToList.map((mapping, index) => {
        const { source, sourceKey, type } = mapping;
        const isUnset = type === 'Unset';
        const itemCurrentMapping =
          (mapping.candidateKey &&
            allCandidates[mapping.candidateKey]?.label) ??
          'No mapping';
        const isLoading = loadingMappings.has(mapping.sourceKey);

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
              <LoadingSpinner
                loading={isLoading}
                alert
                hideText
                inline
                size="sm"
                text={`Updating mapping for ${mapping.source.label}`}
              >
                {mapping.type === 'Unset' && (
                  <ButtonText
                    onClick={() =>
                      handleModalMappingSubmit([
                        {
                          sourceKey: mapping.sourceKey,
                          candidateKey: undefined, // no mapping
                        },
                      ])
                    }
                  >
                    No mapping{' '}
                    <VisuallyHidden>for {mapping.source.label}</VisuallyHidden>
                  </ButtonText>
                )}

                <ButtonText
                  className="govuk-!-margin-left-2"
                  onClick={() => setCurrentMappingItem(mapping)}
                >
                  Map item{' '}
                  <VisuallyHidden>{mapping.source.label}</VisuallyHidden>
                </ButtonText>
              </LoadingSpinner>
            </td>
          </tr>
        );
      })}
    </>
  );
}
