import { DataSetCategoryConfig } from '@common/modules/charts/util/getDataSetCategoryConfigs';

const getUnitOrDefault = (
  configUnit: string | undefined,
  dataSetCategoryConfigs: DataSetCategoryConfig[],
) => {
  if (configUnit) {
    return configUnit;
  }

  const units = dataSetCategoryConfigs.map(config => {
    return config.dataSet.indicator.unit;
  });

  // Check if all units are the same, leave blank if not.
  // Don't display £ as unable to position them before the value. (EES-2278)
  return units.length === 0 ||
    units[0] === '£' ||
    !units.every(unit => unit === units[0])
    ? ''
    : units[0];
};
export default getUnitOrDefault;
