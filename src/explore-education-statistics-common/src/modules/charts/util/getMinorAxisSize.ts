import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import parseNumber from '@common/utils/number/parseNumber';

interface Props {
  dataSetCategories: DataSetCategory[];
  minorAxisSize: number | undefined;
  minorAxisDecimals: number | undefined;
  minorAxisUnit: string;
}

/**
 * Get the highest value used on the axis and determine the axis width based on its length.
 * 12 is a fairly arbitrary width here, more than needed for just the numbers to take into account commas etc.
 */
const getMinorAxisSize = ({
  dataSetCategories,
  minorAxisSize,
  minorAxisDecimals = 0,
  minorAxisUnit,
}: Props) => {
  if (minorAxisSize) {
    return parseNumber(minorAxisSize);
  }

  const highestValueLength = dataSetCategories
    .reduce((acc, dataSetCategory) => {
      const value = Object.values(dataSetCategory.dataSets).reduce(
        (acc2 = acc, dataSet) => (dataSet.value > acc2 ? dataSet.value : acc2),
        0,
      );
      return value > acc ? value : acc;
    }, 0)
    .toString().length;

  return (highestValueLength + minorAxisUnit.length + minorAxisDecimals) * 12;
};

export default getMinorAxisSize;
