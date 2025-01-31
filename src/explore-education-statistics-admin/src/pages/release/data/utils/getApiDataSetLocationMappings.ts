import {
  LocationCandidate,
  LocationMapping,
  LocationOptionSource,
  LocationsMapping,
} from '@admin/services/apiDataSetVersionService';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';
import camelCase from 'lodash/camelCase';
import sortBy from 'lodash/sortBy';

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
  newLocations: Partial<Record<LocationLevelKey, LocationCandidateWithKey[]>>;
  mappableLocations: Partial<Record<LocationLevelKey, MappableLocation[]>>;
  newLocationGroups: Partial<Record<LocationLevelKey, LocationOptionSource[]>>;
  deletedLocationGroups: Partial<
    Record<LocationLevelKey, LocationOptionSource[]>
  >;
} {
  const mappableLocations: Partial<
    Record<LocationLevelKey, MappableLocation[]>
  > = {};
  const newLocations: Partial<
    Record<LocationLevelKey, LocationCandidateWithKey[]>
  > = {};
  const autoMappedLocations: Partial<
    Record<LocationLevelKey, AutoMappedLocation[]>
  > = {};

  const newLocationGroups: Partial<
    Record<LocationLevelKey, LocationOptionSource[]>
  > = {};
  const deletedLocationGroups: Partial<
    Record<LocationLevelKey, LocationOptionSource[]>
  > = {};

  sortBy(Object.keys(locationsMapping.levels)).forEach(level => {
    // TODO remove camelCase
    const levelKey = camelCase(level) as LocationLevelKey;

    const levelMappings = locationsMapping.levels[level]?.mappings ?? {};
    const levelCandidates = locationsMapping.levels[level]?.candidates ?? {};

    const levelMappingEntries = Object.entries(levelMappings);
    const levelCandidateEntries = Object.entries(levelCandidates);

    if (!levelMappingEntries.length) {
      newLocationGroups[levelKey] = levelCandidateEntries.map(
        ([, candidate]) => candidate,
      );

      return;
    }

    if (!levelCandidateEntries.length) {
      deletedLocationGroups[levelKey] = levelMappingEntries.map(
        ([, mapping]) => mapping.source,
      );

      return;
    }

    const mappable: MappableLocation[] = [];
    const autoMapped: AutoMappedLocation[] = [];

    const mappedCandidateKeys = new Set<string>();

    levelMappingEntries.forEach(([key, mapping]) => {
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
    const newLocationsForLevel: LocationCandidateWithKey[] =
      levelCandidateEntries
        .filter(([key, _]) => !mappedCandidateKeys.has(key))
        .map(([key, candidate]) => ({
          ...candidate,
          key,
        }));

    if (mappable.length) {
      mappableLocations[levelKey] = mappable;
    }

    if (autoMapped.length) {
      autoMappedLocations[levelKey] = autoMapped;
    }

    if (newLocationsForLevel.length) {
      newLocations[levelKey] = newLocationsForLevel;
    }
  });

  return {
    autoMappedLocations,
    newLocations,
    mappableLocations,
    newLocationGroups,
    deletedLocationGroups,
  };
}
