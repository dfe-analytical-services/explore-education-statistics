import { MapDataSetCategory } from '@common/modules/charts/components/utils/createMapDataSetCategories';
import generateLegendDataGroups from '@common/modules/charts/components/utils/generateLegendDataGroups';
import { MapDataSetCategoryConfig } from '@common/modules/charts/util/getMapDataSetCategoryConfigs';
import generateHslColour from '@common/utils/colour/generateHslColour';
import {
  MapFeature,
  MapFeatureCollection,
} from '@common/modules/charts/components/MapBlock';
import {
  MapCategoricalData,
  MapLegendItem,
} from '@common/modules/charts/types/chart';

export default function generateFeaturesAndDataGroups({
  deprecatedCategoricalDataConfig,
  dataSetCategories,
  selectedDataSetConfig,
}: {
  /**
   * Deprecated as this information is now on the data set config.
   * This is kept as a fallback for pre-existing maps.
   * @deprecated
   */
  deprecatedCategoricalDataConfig?: MapCategoricalData[];
  dataSetCategories: MapDataSetCategory[];
  selectedDataSetConfig: MapDataSetCategoryConfig;
}): {
  features: MapFeatureCollection;
  legendItems: MapLegendItem[];
  categoricalDataConfig?: MapCategoricalData[];
} {
  const selectedDataSetKey = selectedDataSetConfig.dataKey;

  const isCategoricalData =
    selectedDataSetConfig?.categoricalDataConfig?.length ||
    deprecatedCategoricalDataConfig?.length;

  const { features, legendItems } = isCategoricalData
    ? getCategoricalDataFeatures({
        categoricalDataConfig: selectedDataSetConfig.categoricalDataConfig
          ?.length
          ? selectedDataSetConfig.categoricalDataConfig
          : deprecatedCategoricalDataConfig,
        dataSetCategories,
        selectedDataSetKey,
      })
    : getNumericDataFeatures({
        dataSetCategories,
        selectedDataSetConfig,
        selectedDataSetKey,
      });

  return { features, legendItems };
}

function getCategoricalDataFeatures({
  categoricalDataConfig,
  dataSetCategories,
  selectedDataSetKey,
}: {
  categoricalDataConfig?: MapCategoricalData[];
  dataSetCategories: MapDataSetCategory[];
  selectedDataSetKey: string;
}) {
  const defaultColour = 'rgba(0,0,0,0)';

  const features: MapFeatureCollection = {
    type: 'FeatureCollection',
    features: dataSetCategories.reduce<MapFeature[]>(
      (acc, { dataSets, filter, geoJson }) => {
        const value = dataSets?.[selectedDataSetKey]?.value;

        const matchingDataGroup = categoricalDataConfig?.find(
          dataClass => dataClass.value === value,
        );

        acc.push({
          ...geoJson,
          id: filter.id,
          properties: {
            ...geoJson.properties,
            dataSets,
            colour: matchingDataGroup?.colour ?? defaultColour,
            data: value,
          },
        });

        return acc;
      },
      [],
    ),
  };

  return {
    features,
    categoricalDataConfig,
    legendItems: categoricalDataConfig ?? [],
  };
}

function getNumericDataFeatures({
  dataSetCategories,
  selectedDataSetConfig,
  selectedDataSetKey,
}: {
  dataSetCategories: MapDataSetCategory[];
  selectedDataSetConfig: MapDataSetCategoryConfig;
  selectedDataSetKey: string;
}) {
  const { unit, decimalPlaces } = selectedDataSetConfig.dataSet.indicator;

  const colour =
    selectedDataSetConfig.config.colour ??
    generateHslColour(selectedDataSetConfig.dataKey);

  // Default to white for areas not covered by custom data sets
  // to make it clearer which aren't covered by the groups.
  const defaultColour =
    selectedDataSetConfig.dataGrouping?.type === 'Custom'
      ? 'rgba(255, 255, 255, 1)'
      : 'rgba(0,0,0,0)';

  // Extract only the numeric values out of relevant data sets
  const values: number[] = dataSetCategories.reduce<number[]>(
    (acc, category) => {
      const value = category.dataSets[selectedDataSetKey]?.value;
      if (Number.isFinite(value)) {
        acc.push(Number(value));
      }

      return acc;
    },
    [],
  );

  const dataGroups = generateLegendDataGroups({
    colour,
    dataGrouping: selectedDataSetConfig.dataGrouping,
    decimalPlaces,
    values,
    unit,
  });

  const features: MapFeatureCollection = {
    type: 'FeatureCollection',
    features: dataSetCategories.reduce<MapFeature[]>(
      (acc, { dataSets, filter, geoJson }) => {
        const value = Number(dataSets?.[selectedDataSetKey]?.value);

        const matchingDataGroup = dataGroups.find(dataClass => {
          const roundedValue = Number(value.toFixed(dataClass.decimalPlaces));
          return (
            dataClass.minRaw <= roundedValue && roundedValue <= dataClass.maxRaw
          );
        });

        acc.push({
          ...geoJson,
          id: filter.id,
          properties: {
            ...geoJson.properties,
            dataSets,
            colour: matchingDataGroup?.colour ?? defaultColour,
            data: value,
          },
        });

        return acc;
      },
      [],
    ),
  };

  return {
    features,
    legendItems: dataGroups.map(group => {
      return {
        colour: group.colour,
        value: `${group.min} to ${group.max}`,
      };
    }),
    categoricalDataGroups: undefined,
  };
}
