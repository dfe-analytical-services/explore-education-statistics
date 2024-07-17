import {
  LocationCandidate,
  LocationMapping,
  LocationsMapping,
} from '@admin/services/apiDataSetVersionService';
import { Dictionary } from '@common/types';
import { LocationLevelsType } from '@common/utils/locationLevelsMap';
import difference from 'lodash/difference';

export interface AutoMappedLocation {
  candidate: LocationCandidate;
  mapping: LocationMapping;
}
export interface NewLocationCandidate {
  candidate: LocationCandidate;
}
export interface UnmappedAndManuallyMappedLocation {
  candidate?: LocationCandidate;
  mapping: LocationMapping;
}

export default function getApiDataSetLocationMappings(
  locationsMapping: LocationsMapping,
): {
  autoMappedLocations: AutoMappedLocation[];
  newLocationCandidates: Dictionary<NewLocationCandidate[]>;
  unmappedAndManuallyMappedLocations: Dictionary<
    UnmappedAndManuallyMappedLocation[]
  >;
} {
  const levels = Object.keys(locationsMapping?.levels);
  const unmappedAndManuallyMappedLocations: Dictionary<
    UnmappedAndManuallyMappedLocation[]
  > = {};
  const newLocationCandidates: Dictionary<NewLocationCandidate[]> = {};
  // auto mapped locations aren't grouped by level
  const autoMappedLocations: AutoMappedLocation[] = [];

  levels.forEach(level => {
    const unmapped: {
      candidate?: LocationCandidate;
      mapping: LocationMapping;
    }[] = [];
    const autoMapped: {
      candidate: LocationCandidate;
      mapping: LocationMapping;
    }[] = [];
    const levelCandidates =
      locationsMapping.levels[level as LocationLevelsType].candidates;

    Object.values(
      locationsMapping.levels[level as LocationLevelsType].mappings,
    ).forEach(mapping => {
      if (mapping.type === 'AutoMapped' && mapping.candidateKey) {
        const candidate = levelCandidates[mapping.candidateKey];
        autoMapped.push({ candidate, mapping });
      }
      if (mapping.type === 'ManualMapped' && mapping.candidateKey) {
        const candidate = levelCandidates[mapping.candidateKey];
        unmapped.push({ candidate, mapping });
      }
      if (mapping.type === 'AutoNone' || mapping.type === 'ManualNone') {
        unmapped.push({ mapping });
      }
      return mapping;
    });

    const candidateKeys = Object.keys(
      locationsMapping.levels[level as LocationLevelsType].candidates,
    );
    const mappingKeys = Object.keys(
      locationsMapping.levels[level as LocationLevelsType].mappings,
    );

    const newLocations: { candidate: LocationCandidate }[] = difference(
      candidateKeys,
      mappingKeys,
    ).map(key => {
      return {
        candidate:
          locationsMapping.levels[level as LocationLevelsType]?.candidates[key],
      };
    });

    if (unmapped.length) {
      unmappedAndManuallyMappedLocations[level] = unmapped;
    }
    if (autoMapped.length) {
      autoMappedLocations.push(...autoMapped);
    }
    if (newLocations.length) {
      newLocationCandidates[level] = newLocations;
    }
  });

  return {
    autoMappedLocations,
    newLocationCandidates,
    unmappedAndManuallyMappedLocations,
  };
}
