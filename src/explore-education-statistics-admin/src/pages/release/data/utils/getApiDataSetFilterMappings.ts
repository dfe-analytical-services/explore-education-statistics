import {
  FilterCandidate,
  FiltersMapping,
  FilterMapping,
  FilterOptionSource,
  FilterOptionMapping,
} from '@admin/services/apiDataSetVersionService';
import { Dictionary } from '@common/types';

export interface FilterOptionCandidateWithKey extends FilterOptionSource {
  key: string;
}
export interface FilterOptionMappingWithKey extends FilterOptionMapping {
  sourceKey: string;
}

export interface AutoMappedFilterOption {
  candidate: FilterOptionCandidateWithKey;
  mapping: FilterOptionMappingWithKey;
}

export interface MappableFilterOption {
  candidate?: FilterOptionCandidateWithKey;
  mapping: FilterOptionMappingWithKey;
}

export default function getApiDataSetFilterMappings(
  filtersMapping: FiltersMapping,
): {
  autoMappedFilterOptions: Dictionary<AutoMappedFilterOption[]>;
  newFilterOptions: Dictionary<FilterOptionCandidateWithKey[]>;
  mappableFilterOptions: Dictionary<MappableFilterOption[]>;
  mappableFilters: Dictionary<FilterMapping>;
  newFilters: Dictionary<FilterCandidate>;
} {
  const mappableFilterOptions: Dictionary<MappableFilterOption[]> = {};
  const newFilterOptions: Dictionary<FilterOptionCandidateWithKey[]> = {};
  const autoMappedFilterOptions: Dictionary<AutoMappedFilterOption[]> = {};

  // For MVP these aren't actually mappable, but will be later on.
  const mappableFilters: Dictionary<FilterMapping> = {};
  const newFilters: Dictionary<FilterCandidate> = {};

  const { candidates, mappings } = filtersMapping;

  const mappedOptionCandidateKeys = new Set<string>();

  Object.entries(mappings).forEach(([filterKey, filterMapping]) => {
    const mappable: MappableFilterOption[] = [];
    const autoMapped: AutoMappedFilterOption[] = [];

    // Column is unmapped
    if (filterMapping.type === 'AutoNone') {
      mappableFilters[filterKey] = { ...filterMapping };

      return;
    }

    Object.entries(filterMapping.optionMappings).forEach(
      ([optionKey, optionMapping]) => {
        if (optionMapping.type === 'AutoMapped') {
          if (!optionMapping.candidateKey) {
            throw new Error(
              `Candidate key missing for AutoMapped filter option: ${JSON.stringify(
                {
                  filterKey,
                  optionKey,
                  optionMapping,
                },
              )}`,
            );
          }

          mappedOptionCandidateKeys.add(optionMapping.candidateKey);

          const candidate = candidates[filterKey].options?.[optionKey];

          if (candidate) {
            autoMapped.push({
              candidate: { ...candidate, key: optionMapping.candidateKey },
              mapping: { ...optionMapping, sourceKey: optionKey },
            });
          } else {
            throw new Error(
              `Cannot find candidate for AutoMapped filter option: ${JSON.stringify(
                {
                  filterKey,
                  optionKey,
                  optionMapping,
                },
              )}`,
            );
          }
        }

        if (optionMapping.type === 'ManualMapped') {
          if (!optionMapping.candidateKey) {
            throw new Error(
              `Candidate key missing for ManualMapped filter option: ${JSON.stringify(
                {
                  filterKey,
                  optionKey,
                  optionMapping,
                },
              )}`,
            );
          }
          mappedOptionCandidateKeys.add(optionMapping.candidateKey);

          const candidate =
            candidates[filterKey].options?.[optionMapping.candidateKey];

          if (candidate) {
            mappable.push({
              candidate: { ...candidate, key: optionMapping.candidateKey },
              mapping: { ...optionMapping, sourceKey: optionKey },
            });
          } else {
            throw new Error(
              `Cannot find candidate for ManualMapped filter option: ${JSON.stringify(
                {
                  filterKey,
                  optionKey,
                  optionMapping,
                },
              )}`,
            );
          }
        }

        if (
          optionMapping.type === 'AutoNone' ||
          optionMapping.type === 'ManualNone'
        ) {
          mappable.push({
            mapping: { ...optionMapping, sourceKey: optionKey },
          });
        }
      },
    );

    const optionCandidates = candidates[filterKey]?.options ?? {};

    const newFilterOptionsForLevel: FilterOptionCandidateWithKey[] =
      filtersMapping.candidates[filterKey]
        ? Object.entries(optionCandidates)
            .filter(([key, _]) => !mappedOptionCandidateKeys.has(key))
            .map(([key, candidate]) => ({
              ...candidate,
              key,
            }))
        : [];

    if (autoMapped.length) {
      autoMappedFilterOptions[filterKey] = autoMapped;
    }
    if (mappable.length) {
      mappableFilterOptions[filterKey] = mappable;
    }
    if (newFilterOptionsForLevel.length) {
      newFilterOptions[filterKey] = newFilterOptionsForLevel;
    }
  });

  Object.entries(candidates).forEach(([filterKey, filterCandidate]) => {
    if (!filtersMapping.mappings[filterKey]) {
      newFilters[filterKey] = filterCandidate;
    }
  });

  return {
    autoMappedFilterOptions,
    mappableFilterOptions,
    newFilterOptions,
    mappableFilters,
    newFilters,
  };
}
