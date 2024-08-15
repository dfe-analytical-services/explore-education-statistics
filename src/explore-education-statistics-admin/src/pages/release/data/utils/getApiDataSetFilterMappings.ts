import {
  FilterCandidate,
  FiltersMapping,
  FilterMapping,
  FilterOptionSource,
  FilterOptionMapping,
} from '@admin/services/apiDataSetVersionService';
import { Dictionary } from '@common/types';

export interface FilterCandidateWithKey extends FilterOptionSource {
  key: string;
}
export interface FilterMappingWithKey extends FilterOptionMapping {
  sourceKey: string;
}

export interface AutoMappedFilter {
  candidate: FilterCandidateWithKey;
  mapping: FilterMappingWithKey;
}

export interface MappableFilter {
  candidate?: FilterCandidateWithKey;
  mapping: FilterMappingWithKey;
}

export default function getApiDataSetFilterMappings(
  filtersMapping: FiltersMapping,
): {
  autoMappedFilterOptions: Dictionary<AutoMappedFilter[]>;
  newFilterOptions: Dictionary<FilterCandidateWithKey[]>;
  mappableFilterOptions: Dictionary<MappableFilter[]>;
  mappableFilterColumns: Dictionary<FilterMapping>;
  newFilterColumns: Dictionary<FilterCandidate>;
} {
  const mappableFilterOptions: Dictionary<MappableFilter[]> = {};
  const newFilterOptions: Dictionary<FilterCandidateWithKey[]> = {};
  const autoMappedFilterOptions: Dictionary<AutoMappedFilter[]> = {};

  // For MVP these aren't actually mappable, but will be later on.
  const mappableFilterColumns: Dictionary<FilterMapping> = {};
  const newFilterColumns: Dictionary<FilterCandidate> = {};

  const { candidates, mappings } = filtersMapping;

  const mappedOptionCandidateKeys = new Set<string>();

  Object.entries(mappings).forEach(([filterKey, filterMapping]) => {
    const mappable: MappableFilter[] = [];
    const autoMapped: AutoMappedFilter[] = [];

    // Column is unmapped
    if (filterMapping.type === 'AutoNone') {
      mappableFilterColumns[filterKey] = { ...filterMapping };

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

    const newFilterOptionsForLevel: FilterCandidateWithKey[] = filtersMapping
      .candidates[filterKey]
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
      newFilterColumns[filterKey] = filterCandidate;
    }
  });

  return {
    autoMappedFilterOptions,
    mappableFilterOptions,
    newFilterOptions,
    mappableFilterColumns,
    newFilterColumns,
  };
}
