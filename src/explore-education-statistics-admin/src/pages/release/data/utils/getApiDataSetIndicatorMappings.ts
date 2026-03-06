import {
  IndicatorCandidate,
  IndicatorMapping,
  IndicatorsMapping,
} from '@admin/services/apiDataSetVersionService';

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
  const autoMappedIndicators: AutoMappedIndicator[] = Object.entries(
    mappings,
  ).map(([key, mapping]) => {
    if (!mapping.candidateKey) {
      throw new Error(
        `Candidate key missing for AutoMapped indicator: ${JSON.stringify({
          key,
          mapping,
        })}`,
      );
    }

    const candidate = candidates[mapping.candidateKey];

    if (!candidate) {
      throw new Error(
        `Cannot find candidate for AutoMapped indicator: ${JSON.stringify({
          key,
          mapping,
        })}`,
      );
    }

    return {
      mapping: {
        candidateKey: mapping.candidateKey,
        publicId: mapping.publicId,
        source: mapping.source,
        type: mapping.type,
        sourceKey: key,
      },
      candidate: {
        ...candidates[mapping.candidateKey!],
        key: mapping.candidateKey!,
      },
    };
  });

  // TODO EES-6764 - implement
  const newIndicators: IndicatorCandidateWithKey[] = [];

  // TODO EES-6764 - implement
  const mappableIndicators: MappableIndicator[] = [];

  return {
    autoMappedIndicators,
    newIndicators,
    mappableIndicators,
  };
}
