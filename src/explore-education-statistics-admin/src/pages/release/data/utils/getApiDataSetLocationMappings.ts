import {
  LocationCandidate,
  LocationMapping,
  LocationsMapping,
} from '@admin/services/apiDataSetVersionService';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';
import { camelCase } from 'lodash';

export interface LocationCandidateWithKey extends LocationCandidate {
  key: string;
}
export interface LocationMappingWithKey extends LocationMapping {
  sourceKey: string;
}

export interface AutoMappedLocation {
  candidate: LocationCandidateWithKey;
  mapping: LocationMappingWithKey;
}

export interface MappableLocation {
  candidate?: LocationCandidateWithKey;
  mapping: LocationMappingWithKey;
}

export default function getApiDataSetLocationMappings(
  locationsMapping: LocationsMapping,
): {
  autoMappedLocations: Partial<Record<LocationLevelKey, AutoMappedLocation[]>>;
  newLocationCandidates: Partial<
    Record<LocationLevelKey, LocationCandidateWithKey[]>
  >;
  mappableLocations: Partial<Record<LocationLevelKey, MappableLocation[]>>;
} {
  const mappableLocations: Partial<
    Record<LocationLevelKey, MappableLocation[]>
  > = {};
  const newLocationCandidates: Partial<
    Record<LocationLevelKey, LocationCandidateWithKey[]>
  > = {};
  const autoMappedLocations: Partial<
    Record<LocationLevelKey, AutoMappedLocation[]>
  > = {};

  Object.keys(locationsMapping.levels).forEach(level => {
    const mappable: MappableLocation[] = [];
    const autoMapped: AutoMappedLocation[] = [];

    const levelMappings = locationsMapping.levels[level]?.mappings ?? {};
    const levelCandidates = locationsMapping.levels[level]?.candidates ?? {};

    const mappedCandidateKeys = new Set<string>();

    Object.entries(levelMappings).forEach(([key, mapping]) => {
      if (mapping.type === 'AutoMapped') {
        if (!mapping.candidateKey) {
          throw new Error(
            `Candidate key missing for AutoMapped location: ${JSON.stringify({
              level,
              key,
              mapping,
            })}`,
          );
        }

        mappedCandidateKeys.add(mapping.candidateKey);

        const candidate = levelCandidates[mapping.candidateKey];

        if (candidate) {
          autoMapped.push({
            candidate: { ...candidate, key: mapping.candidateKey },
            mapping: { ...mapping, sourceKey: key },
          });
        } else {
          throw new Error(
            `Cannot find candidate for AutoMapped location: ${JSON.stringify({
              level,
              key,
              mapping,
            })}`,
          );
        }
      }

      if (mapping.type === 'ManualMapped') {
        if (!mapping.candidateKey) {
          throw new Error(
            `Candidate key missing for ManualMapped location: ${JSON.stringify({
              level,
              key,
              mapping,
            })}`,
          );
        }

        mappedCandidateKeys.add(mapping.candidateKey);

        const candidate = levelCandidates[mapping.candidateKey];

        if (candidate) {
          mappable.push({
            candidate: { ...candidate, key: mapping.candidateKey },
            mapping: { ...mapping, sourceKey: key },
          });
        } else {
          throw new Error(
            `Cannot find candidate for ManualMapped location: ${JSON.stringify({
              level,
              key,
              mapping,
            })}`,
          );
        }
      }

      if (mapping.type === 'AutoNone' || mapping.type === 'ManualNone') {
        mappable.push({ mapping: { ...mapping, sourceKey: key } });
      }
    });

    // New locations:
    // - completely new locations (have a unique key)
    // - locations that were auto mapped but the mapping has been removed
    // (the original mapping and the candidate have the same key)
    const newLocations: LocationCandidateWithKey[] = Object.entries(
      levelCandidates,
    )
      .filter(([key, _]) => !mappedCandidateKeys.has(key))
      .map(([key, candidate]) => ({
        ...candidate,
        key,
      }));

    // TODO remove camelCase
    const levelKey = camelCase(level) as LocationLevelKey;

    if (mappable.length) {
      mappableLocations[levelKey] = mappable;
    }

    if (autoMapped.length) {
      autoMappedLocations[levelKey] = autoMapped;
    }

    if (newLocations.length) {
      newLocationCandidates[levelKey] = newLocations;
    }
  });

  return {
    autoMappedLocations,
    newLocationCandidates,
    mappableLocations,
  };
}
