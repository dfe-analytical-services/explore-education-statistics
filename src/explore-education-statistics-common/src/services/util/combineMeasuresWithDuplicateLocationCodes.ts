import {
  LocationOption,
  TableDataResult,
} from '@common/services/tableBuilderService';
import groupBy from 'lodash/groupBy';
import isEqual from 'lodash/isEqual';
import mapValues from 'lodash/mapValues';
import partition from 'lodash/partition';
import sum from 'lodash/sum';
import uniq from 'lodash/uniq';
import uniqWith from 'lodash/uniqWith';

type LocationGroupingKey = {
  level: string;
  code: string;
};

export type MeasurementsMergeStrategy = (
  measurementValues: (string | undefined)[],
) => string;

/**
 * Combine rows of {@param results} that reference Locations that use duplicate
 * Codes but have unique Names.
 *
 * This is possible when, say, a Provider changes its name during the course of
 * the period of time that a Data Table covers, thus leading to 2 distinct names
 * for the same Provider (under the same Code).
 *
 * The API already handles this scenario by returning a single combined Location
 * option which groups these 2 or more duplicate Locations into a single
 * selectable option in the Table Tool (with the label of the form
 * "Location A / Location B").
 *
 * {@param deduplicatedLocations} is an array of Locations available for the
 * given Subject being queried, and from this list, the single combined Location
 * label can be determined for use as the Location label for the row header that
 * these combined result rows will fall under.
 *
 * {@param measurementsMergeStrategy} is the strategy whereby multiple values
 * from the various duplicate Locations are merged together so that the final
 * value will reside within a single table cell. By default this is by summing
 * any numerical values together or, if no numeric values exist, to display the
 * first non-numeric string.
 */
export default function combineMeasuresWithDuplicateLocationCodes(
  results: TableDataResult[],
  deduplicatedLocations: LocationOption[],
  measurementsMergeStrategy: MeasurementsMergeStrategy = sumNumericValuesMergeStrategy,
): TableDataResult[] {
  // If there is no potential data to merge because no Locations needed deduplicating, simply return the original
  // results.
  if (deduplicatedLocations.length === 0) {
    return results;
  }

  // Otherwise, locate the potentially affected rows of data in order to merge them.  Find any rows belonging to a
  // deduplicated Location.
  const [deduplicatedLocationsResults, unaffectedResults] = partition(
    results,
    result => {
      // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
      const { code } = result.location![result.geographicLevel];
      return deduplicatedLocations.find(
        dedupedLocations =>
          dedupedLocations.level === result.geographicLevel &&
          dedupedLocations.value === code,
      );
    },
  );

  // If there is no data that belongs to any of the Locations that were deduplicated, return the original results.
  if (deduplicatedLocationsResults.length === 0) {
    return results;
  }

  // Otherwise, iterate through the results that potentially need merging, looking at each row's location[geographicLevel], and see if rows with the same
  // geographicLevel also have the same location[geographicLevel].code but different location[geographicLevel].name.
  const resultsGroupedByLocationCodeAndLevel = groupBy(
    deduplicatedLocationsResults,
    result =>
      JSON.stringify({
        level: result.geographicLevel,
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        code: result.location![result.geographicLevel].code,
      }),
  );

  return Object.entries(resultsGroupedByLocationCodeAndLevel).flatMap(
    ([key, resultsForLocation]) => {
      const { level, code }: LocationGroupingKey = JSON.parse(key);
      const resultsGroupedByLocationName = groupBy(
        resultsForLocation,
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        r => r.location![level].name,
      );

      // If there is only a single unique Location name for this combination of Level and Code, no combining of result
      // sets needs to be done.
      if (Object.keys(resultsGroupedByLocationName).length === 1) {
        return resultsForLocation;
      }

      // Get the label for the combined Location row.
      const combinedLocation = deduplicatedLocations.find(
        l => l.level === level && l.value === code,
      );
      if (!combinedLocation) {
        throw new Error(
          `No available Location exists with level ${level} and code ${code}`,
        );
      }

      const allAvailableTimePeriods = uniq(
        resultsForLocation.flatMap(result => result.timePeriod),
      );

      // Generate a set of result rows that will cover every Time Period / Filter combination that is present in
      // this set of data.  These combinations will be used to gather values from each Location name's data sets
      // against every measurement that is captured in this data.  The values will be kept in order of their Location
      // name so that the order of combined measurement values will match the order of the combined Location names in the
      // table row label e.g. ("Provider 1 / Provider 2" - Achievements: "20 / 35"), where "20" is the "Achievements" value
      // for Provider 1, and "35" is the "Achievements" value for Provider 2.
      const timePeriodFilterCombinations: TableDataResult[] = allAvailableTimePeriods.flatMap(
        timePeriod => {
          const rowsForTimePeriod = resultsForLocation.filter(
            r => r.timePeriod === timePeriod,
          );
          const allAvailableFilterCombinations = uniqWith(
            rowsForTimePeriod.map(result => result.filters.sort()),
            (a1, a2) => isEqual(a1, a2),
          );
          const allAvailableMeasures = uniq(
            rowsForTimePeriod.flatMap(result => Object.keys(result.measures)),
          );
          return allAvailableFilterCombinations.flatMap(filters => {
            return {
              timePeriod,
              filters,
              geographicLevel: level,
              location: {
                [level]: {
                  name: combinedLocation.label,
                  code,
                },
              },
              measures: allAvailableMeasures.reduce(
                (acc, measure) => ({
                  ...acc,
                  [measure]: '',
                }),
                {},
              ),
            } as TableDataResult;
          });
        },
      );

      const allAvailableLocationNames = Object.keys(
        resultsGroupedByLocationName,
      ).sort();

      // Now for each combination of Time Period and Filters, produce a single combined row of data that merges
      // the duplicate rows from each Location into one, using a strategy to merge a set of data for each measurement
      // into a single value.
      const deduplicatedResults = timePeriodFilterCombinations.flatMap(
        combination => {
          // For each measure, collect the value for that measure from each Location's results for this Time Period and
          // Filter combination, or undefined if a Location does not have a value that matches this criteria.
          const measureValuesForEachLocation = Object.keys(
            combination.measures,
          ).reduce((acc, measure) => {
            const measureValues = allAvailableLocationNames.map(
              locationName => {
                const resultForLocationName = resultsGroupedByLocationName[
                  locationName
                ].find(
                  result =>
                    result.timePeriod === combination.timePeriod &&
                    isEqual(combination.filters, result.filters),
                );
                return resultForLocationName?.measures[measure];
              },
            );
            return {
              ...acc,
              [measure]: measureValues,
            };
          }, {});

          // Now for each measure, combine the values gathered from all of the Locations into a single value per measure.
          const mergedMeasurements = mapValues(
            measureValuesForEachLocation,
            measurementsMergeStrategy,
          );

          // Return the Time Period / Filter combination, now with the merged measurement values from all of the
          // Locations
          return {
            ...combination,
            measures: mergedMeasurements,
          };
        },
      );

      return [...unaffectedResults, ...deduplicatedResults];
    },
  );
}

/**
 * This is a strategy for merging measurement values from several duplicate Locations into a single value.
 * This strategy produces a summation by adding up any numerical values from each Location for this measurement,
 * and ignoring any non-numerical values.
 *
 * If no numerical values are found, this will return the first non-falsy non-numerical value found.
 *
 * ${@param measurementValues} represents the multiple values from each Location for this measurement.
 */
const sumNumericValuesMergeStrategy: MeasurementsMergeStrategy = (
  measurementValues: (string | undefined)[],
) => {
  const numericalValuesExist = measurementValues.some(
    value => value && !Number.isNaN(parseFloat(value)),
  );

  if (!numericalValuesExist) {
    return measurementValues.find(value => value) ?? '';
  }

  const total = sum(
    measurementValues.map(value => {
      const numeric = value ? parseFloat(value) : 0;
      return !Number.isNaN(numeric) ? numeric : 0;
    }),
  );
  return `${total}`;
};
