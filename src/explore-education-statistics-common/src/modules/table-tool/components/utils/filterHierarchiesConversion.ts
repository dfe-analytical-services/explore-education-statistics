import { FullTableQuery } from '@common/services/tableBuilderService';
import mapValues from 'lodash/mapValues';
import { FiltersFormValues } from '../FiltersForm';

const fhAncestrySeparator = ',';

/**
 *  convert an option's related options (ancestor ids, this option id and child total ids) into a unique string
 * */
export function hierarchyOptionsToString(relatedOptions: string[]): string {
  return relatedOptions.join(fhAncestrySeparator);
}

/**
 * convert an option's unique string into a related options list (ancestor ids, this option id and child total ids)
 * */
export function hierarchyOptionsFromString(relatedOptions: string): string[] {
  return relatedOptions.split(fhAncestrySeparator);
}

export function convertHierarchiesFormToQuery(
  filterHierarchies: FiltersFormValues['filterHierarchies'],
): FullTableQuery['filterHierarchiesOptions'] {
  return mapValues(filterHierarchies, selectedHierarchyOptions =>
    selectedHierarchyOptions.map(hierarchyOptionsFromString),
  );
}

export function convertHierarchiesQueryToForm(
  filterHierarchies: FullTableQuery['filterHierarchiesOptions'],
): FiltersFormValues['filterHierarchies'] {
  return mapValues(filterHierarchies, selectedHierarchyOptions =>
    selectedHierarchyOptions.map(hierarchyOptionsToString),
  );
}
