import { MapDataSetCategory } from '@common/modules/charts/components/utils/createMapDataSetCategories';
import generateLegendDataGroups, {
  LegendDataGroup,
} from '@common/modules/charts/components/utils/generateLegendDataGroups';
import { MapDataSetCategoryConfig } from '@common/modules/charts/util/getMapDataSetCategoryConfigs';
import generateHslColour from '@common/utils/colour/generateHslColour';
import {
  MapFeature,
  MapFeatureCollection,
} from '@common/modules/charts/components/MapBlock';

export default function generateFeaturesAndDataGroups({
  dataSetCategories,
  selectedDataSetConfig,
}: {
  dataSetCategories: MapDataSetCategory[];
  selectedDataSetConfig: MapDataSetCategoryConfig;
}): {
  features: MapFeatureCollection;
  dataGroups: LegendDataGroup[];
} {
  const selectedDataSetKey = selectedDataSetConfig.dataKey;
  const { unit, decimalPlaces } = selectedDataSetConfig.dataSet.indicator;

  const colour =
    selectedDataSetConfig.config.colour ??
    generateHslColour(selectedDataSetConfig.dataKey);

  // Extract only the numeric values out of relevant data sets
  const values = dataSetCategories.reduce<number[]>((acc, category) => {
    const value = category.dataSets[selectedDataSetKey]?.value;

    if (Number.isFinite(value)) {
      acc.push(value);
    }

    return acc;
  }, []);

  const dataGroups = generateLegendDataGroups({
    colour,
    dataGrouping: selectedDataSetConfig.dataGrouping,
    decimalPlaces,
    values,
    unit,
  });

  // Default to white for areas not covered by custom data sets
  // to make it clearer which aren't covered by the groups.
  const defaultColour =
    selectedDataSetConfig.dataGrouping?.type === 'Custom'
      ? 'rgba(255, 255, 255, 1)'
      : 'rgba(0,0,0,0)';

  const features: MapFeatureCollection = {
    type: 'FeatureCollection',
    features: dataSetCategories.reduce<MapFeature[]>(
      (acc, { dataSets, filter, geoJson }) => {
        const value = dataSets?.[selectedDataSetKey]?.value;

        const matchingDataGroup = Number.isFinite(value)
          ? dataGroups.find(dataClass => {
              const roundedValue = Number(
                value.toFixed(dataClass.decimalPlaces),
              );
              return (
                dataClass.minRaw <= roundedValue &&
                roundedValue <= dataClass.maxRaw
              );
            })
          : undefined;

        acc.push({
          ...geoJson,
          id: filter.id,
          properties: {
            ...geoJson.properties,
            dataSets,
            // Default to transparent if no match
            colour: matchingDataGroup?.colour ?? defaultColour,
            data: value,
          },
        });

        return acc;
      },
      [],
    ),
  };

  return { features, dataGroups };
}
