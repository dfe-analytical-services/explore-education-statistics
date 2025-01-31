import { DataSetCategory } from '@common/modules/charts/types/dataSet';

export default function getCategoryLabel(dataSetCategories: DataSetCategory[]) {
  return (tickId: string) =>
    dataSetCategories.find(category => category.filter.id === tickId)?.filter
      .label ?? tickId;
}
