import { DataSetCategoryConfig } from '@common/modules/charts/util/getDataSetCategoryConfigs';

const getUnit = (dataSetCategoryConfigs: DataSetCategoryConfig[]): string => {
  const units = dataSetCategoryConfigs.map(config => {
    return config.dataSet.indicator.unit;
  });

  // Check if all units are the same, leave blank if not.
  return units.length === 0 || !units.every(unit => unit === units[0])
    ? ''
    : units[0];
};
export default getUnit;
