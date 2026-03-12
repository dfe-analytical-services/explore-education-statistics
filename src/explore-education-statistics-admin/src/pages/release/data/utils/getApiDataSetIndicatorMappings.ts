import {
  IndicatorCandidate,
  IndicatorMapping,
  IndicatorsMapping,
} from '@admin/services/apiDataSetVersionService';
import { Dictionary } from '@common/types';

export interface IndicatorCandidateWithKey extends IndicatorCandidate {
  key: string;
}

export interface IndicatorMappingWithKey extends IndicatorMapping {
  sourceKey: string;
}

export interface AutoMappedIndicator {
  candidate: IndicatorCandidateWithKey;
  mapping: IndicatorMappingWithKey;
}

export interface MappableIndicator {
  candidate?: IndicatorCandidateWithKey;
  mapping: IndicatorMappingWithKey;
}

export default function getApiDataSetIndicatorMappings({
  mappings,
  candidates,
}: IndicatorsMapping): {
  autoMappedIndicators: AutoMappedIndicator[];
  newIndicators: IndicatorCandidateWithKey[];
  mappableIndicators: MappableIndicator[];
} {
  const autoMappedIndicators: AutoMappedIndicator[] = Object.entries(mappings)
    .filter(([_, mapping]) => mapping.type === 'AutoMapped')
    .map(([key, mapping]) => {
      const candidate = getCandidateWithKey(mapping, key, candidates);
      return {
        mapping: {
          candidateKey: mapping.candidateKey,
          publicId: mapping.publicId,
          source: mapping.source,
          type: mapping.type,
          sourceKey: key,
        },
        candidate,
      };
    });

  const newIndicators: IndicatorCandidateWithKey[] = Object.entries(candidates)
    .filter(([candidateKey, _]) => {
      return !Object.values(mappings).some(
        mapping => mapping.candidateKey === candidateKey,
      );
    })
    .map(([candidateKey, candidate]) => ({
      label: candidate.label,
      key: candidateKey,
    }));

  const mappableIndicators: MappableIndicator[] = Object.entries(mappings)
    .filter(([_, mapping]) => mapping.type !== 'AutoMapped')
    .map(([key, mapping]) => {
      const candidate =
        mapping.type === 'ManualMapped'
          ? getCandidateWithKey(mapping, key, candidates)
          : undefined;

      return {
        mapping: {
          ...mapping,
          sourceKey: key,
        },
        candidate,
      };
    });

  return {
    autoMappedIndicators,
    newIndicators,
    mappableIndicators,
  };
}

function getCandidateWithKey(
  mapping: IndicatorMapping,
  sourceKey: string,
  candidates: Dictionary<IndicatorCandidate>,
): IndicatorCandidateWithKey {
  if (!mapping.candidateKey) {
    throw new Error(
      `Candidate key missing for ${mapping.type} indicator: ${JSON.stringify({
        sourceKey,
        mapping,
      })}`,
    );
  }

  const candidate = candidates[mapping.candidateKey!];

  if (!candidate) {
    throw new Error(
      `Cannot find candidate for ${mapping.type} indicator: ${JSON.stringify({
        sourceKey,
        mapping,
      })}`,
    );
  }

  return {
    ...candidate,
    key: mapping.candidateKey!,
  };
}
