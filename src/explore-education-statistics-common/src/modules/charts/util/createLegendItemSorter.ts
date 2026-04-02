import { DataSetCategoryConfig } from '@common/modules/charts/util/getDataSetCategoryConfigs';
import { LegendPayload } from 'recharts/types/component/DefaultLegendContent';

/**
 * Creates a sorting function for Recharts Legend based on dataSetConfig item order,
 * to match order of bars/tooltips etc.
 * * @param dataSetCategoryItems - The array of objects defining the correct order.
 * @returns A function compatible with Recharts' itemSorter prop.
 */
export default function createLegendItemSorter(
  dataSetCategoryItems: DataSetCategoryConfig[],
) {
  // Build the lookup dictionary
  const orderMap = new Map<string, number>(
    dataSetCategoryItems.map((item, index) => [item.dataKey, index]),
  );

  // Return the function type Recharts expects
  return (item: LegendPayload): number => {
    // If Recharts passes an item with no dataKey, send it to the back
    if (!item.dataKey) return 9999;

    const index = orderMap.get(item.dataKey.toString());

    // Return the mapped index, or a high number if it's an unexpected label
    return index !== undefined ? index : 9999;
  };
}
