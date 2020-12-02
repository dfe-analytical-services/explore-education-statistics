import { DataSetCategoryConfig } from '@common/modules/charts/util/getDataSetCategoryConfigs';

/**
 * Get the decimal places needed for the minor axis of a
 * chart given its {@param categoryDataSets}.
 *
 * We just get the largest number of decimal places for
 * all the indicators chosen. It should be noted, this
 * could lead to incorrect decimal places being shown for
 * an indicator if there is another indicator that has
 * a higher number of decimal places.
 *
 * Currently, this should be acceptable as we don't display
 * multiple minor axes anyway so it's not advisable to be
 * using significantly different indicators on the same chart.
 */
export default function getMinorAxisDecimalPlaces(
  categoryDataSets: DataSetCategoryConfig[],
): number | undefined {
  return categoryDataSets.reduce<number | undefined>((acc, { dataSet }) => {
    if (typeof dataSet.indicator.decimalPlaces === 'undefined') {
      return acc;
    }

    if (dataSet.indicator?.decimalPlaces > (acc ?? 0)) {
      return dataSet.indicator.decimalPlaces;
    }

    return acc;
  }, undefined);
}
